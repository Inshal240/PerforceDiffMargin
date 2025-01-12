using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Perforce.P4;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;

namespace PerforceDiffMargin.Perforce
{
    public class PerforceCommands
    {
        public event EventHandler ConnectionChanged;

        private static PerforceCommands instance = null;

        public enum ConnectionState
        {
            Unknown = 0, // default state, _repository and _connection are null, can't get P4USER, P4PORT or P4CLIENT
            Initialized = 1, //_repository and _connection are initialized, but not connected. can't get P4USER, P4PORT or P4CLIENT
            Connected = 2, //_repository and _connection are initialized and connected. can get P4USER, P4PORT or P4CLIENT No login and no logic checks
            Success = 3, // everything is known and initialized
        }

        private readonly IServiceProvider _serviceProvider;
        private Server _server;
        private Repository _repository;
        private Connection _connection;
        private string _perforceRoot;
        private ConnectionState _state = ConnectionState.Unknown;

        #region Public Methods

        public static PerforceCommands GetInstance(IServiceProvider serviceProvider = null)
        {
            if (instance == null)
                instance = new PerforceCommands(serviceProvider);
            return instance;
        }

        // P4USER, P4PORT and P4CLIENT should be set. 

        public void SetNewPort(string uri)
        {
            DisconnectImpl();

            Init(uri);
        }

        public void Login(string password)
        {
            DisconnectImpl();
            Init();

            _connection.Login(password);
            _state = ConnectionState.Success;
        }

        public void RefreshConnection()
        {
            try
            {
                RefreshConnectionImpl();
            }
            catch (P4Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Disconnect()
        {
            DisconnectImpl();
            ConnectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void DisconnectImpl()
        {
            if (_connection != null)
            {
                _connection.Disconnect();
                _state = ConnectionState.Initialized;
            }
        }

        public IEnumerable<HunkRangeInfo> GetDiffFor(ITextDocument textDocument, ITextSnapshot snapshot)
        {
            if (!CanGetDiff(textDocument.FilePath))
                yield break;

            var depotPath = GetPerforcePath(textDocument.FilePath);

            IList<FileDiff> target = null;
            try
            {
                // get diff using p4 diff
                GetDepotFileDiffsCmdOptions opts = new GetDepotFileDiffsCmdOptions(GetDepotFileDiffsCmdFlags.Unified, 0, 0, "", "", "");
                IList<FileSpec> fsl = new List<FileSpec>();
                FileSpec fs = new FileSpec(new DepotPath(depotPath));
                fsl.Add(fs);
                target = _repository.GetFileDiffs(fsl, opts);
            }
            catch (P4Exception)
            {
                DisconnectImpl();
                yield break;
            }

            // TODO: implement comparison with yellow changes not "file on disk" vs "file in depot" but "file in RAM in VS" vs "file in depot"
            //var content = GetCompleteContent(textDocument, snapshot);
            //if (content == null) yield break;

            // TODO: after debugging remove the second and the third arguments from c'tor and next code, are they useless?
            var uniDiffParser = new UnifiedFormatDiffParser(target[0].Diff, 0, false);
            var hunkRangeInfos = uniDiffParser.Parse();

            foreach (var hunkRangeInfo in hunkRangeInfos)
            {
                yield return hunkRangeInfo;
            }
        }

        public void StartExternalDiff(ITextDocument textDocument)
        {
            if (textDocument == null || string.IsNullOrEmpty(textDocument.FilePath))
                return;

            var filename = textDocument.FilePath;

            if (!IsDiffPerformed(filename))
                return;

            var depotPath = GetPerforcePath(filename);
            FileSpec fs = new FileSpec(new DepotPath(depotPath));

            var tempFileName = Path.GetTempFileName();
            var printOptions = new GetFileContentsCmdOptions(GetFileContentsCmdFlags.Suppress, tempFileName);
            _repository.GetFileContents(printOptions, fs);

            IVsDifferenceService differenceService = _serviceProvider.GetService(typeof(SVsDifferenceService)) as IVsDifferenceService;
            string leftFileMoniker = tempFileName;
            // The difference service will automatically load the text from the file open in the editor, even if
            // it has changed. Don't use the original path here.
            string rightFileMoniker = filename;
            string caption = "p4 diff";

            string leftLabel = string.Format("{0}#head", depotPath);
            string rightLabel = filename;
            string inlineLabel = null;
            string tooltip = null;
            string roles = null;

            __VSDIFFSERVICEOPTIONS grfDiffOptions = __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary;
            differenceService.OpenComparisonWindow2(leftFileMoniker, rightFileMoniker, caption, tooltip, leftLabel, rightLabel, inlineLabel, roles, (uint)grfDiffOptions);

            // Since the file is marked as temporary, we can delete it now
            // Perforce can create read-only file, so set FileAttributes.Normal in order to safe delete it
            System.IO.File.SetAttributes(tempFileName, FileAttributes.Normal);
            System.IO.File.Delete(tempFileName);
        }

        public bool IsDiffPerformed(string path)
        {
            switch (_state)
            {
                case ConnectionState.Unknown:
                case ConnectionState.Initialized:
                    return true; // can reconnect later
                case ConnectionState.Connected:
                    return !IsPerforceRootFound() || // can reconnect later
                        IsFileUnderPerforceRoot(path);
                case ConnectionState.Success:
                    return IsFileUnderPerforceRoot(path);
                default:
                    return true; // can reconnect later
            }
        }

        public string GetP4EnvironmentVar(string varName)
        {
            string res = null;
            Init();

            if (_connection != null)
            {
                res = _connection.GetP4EnvironmentVar(varName);
            }
            return res;
        }

        public void SetP4EnvironmentVar(string varName, string val)
        {
            Init();

            if (_connection != null)
            {
                _connection.SetP4EnvironmentVar(varName, val);
            }
        }

        #endregion

        #region Private Methods

        private string GetStateDescription(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Unknown:
                    return "Unknown state, connection is not initialized";
                case ConnectionState.Initialized:
                    return "Not connected to local p4. Can't get environmental variables like P4USER, P4PORT or P4CLIENT";
                case ConnectionState.Connected:
                    {
                        string p4user = "";
                        try
                        {
                            p4user = GetP4EnvironmentVar("P4USER");
                        }
                        catch (P4Exception)
                        {
                            // nothig to do, empty string remains
                        }

                        string p4port = "";
                        try
                        {
                            p4port = GetP4EnvironmentVar("P4PORT");
                        }
                        catch (P4Exception)
                        {
                            // nothig to do, empty string remains
                        }

                        string p4client = "";
                        try
                        {
                            p4client = GetP4EnvironmentVar("P4CLIENT");
                        }
                        catch (P4Exception)
                        {
                            // nothig to do, empty string remains
                        }

                        return String.Format("Connected to local p4 but can't connect to the server. P4USER, P4PORT and P4CLIENT perforce environment varialbes should be set. Login should be done. Currently thier values are: \nP4USER={0} \nP4PORT={1} \nP4CLIENT={2}",
                            p4user, p4port, p4client);
                    }
                case ConnectionState.Success:
                    return "Successfully connected!";
                default:
                    return "PerforceDiffCommand plugin internal error. Please report the issue."; // TODO: add link?
            }
        }

        // Probably will be required to get content
        private static byte[] GetCompleteContent(ITextDocument textDocument, ITextSnapshot snapshot)
        {
            var currentText = snapshot.GetText();

            var content = textDocument.Encoding.GetBytes(currentText);

            var preamble = textDocument.Encoding.GetPreamble();
            if (preamble.Length == 0) return content;

            var completeContent = new byte[preamble.Length + content.Length];
            Buffer.BlockCopy(preamble, 0, completeContent, 0, preamble.Length);
            Buffer.BlockCopy(content, 0, completeContent, preamble.Length, content.Length);

            return completeContent;
        }

        private void RefreshConnectionImpl()
        {
            DisconnectImpl();
            Init();

            _perforceRoot = _repository.GetClientMetadata().Root;

            var error_msg_start = GetStateDescription(ConnectionState.Connected);

            if (!IsPerforceRootFound())
            {
                DisconnectImpl();
                throw new P4Exception(ErrorSeverity.E_FAILED, error_msg_start + " \nError: \nWorkspace root is unset or doesn't exist");
            }

            if (!_connection.connectionEstablished() || _connection.GetActiveTicket() == null)
            {
                DisconnectImpl();
                throw new P4Exception(ErrorSeverity.E_FAILED, error_msg_start);
            }

            // check ticket exists and valid. 
            var cmd = new P4Command(_connection, "login", true, new string[] { "-s" }); // p4 login -s get status of the ticket
            try
            {
                // P4Exception will be thrown in case user is not logged in
                cmd.Run();
            }
            catch (P4Exception ex)
            {
                DisconnectImpl();
                throw new P4Exception(ErrorSeverity.E_FAILED, error_msg_start + " \nError: \n" + ex.Message);
            }

            // this ticket should belong to current user
            var output = cmd.TextOutput;
            if (cmd.TaggedOutput[0]["User"] != _connection.UserName)
            {
                DisconnectImpl();
                throw new P4Exception(ErrorSeverity.E_FAILED, error_msg_start + " \nError: \nP4USER variable don't correspond to logged in user.");
            }

            _state = ConnectionState.Success;
        }

        private void Init(string uri = null)
        {
            string corrected_uri = String.IsNullOrEmpty(uri) ? "" : uri;
            if (_server == null ||
                (_server.Address.Uri != corrected_uri && !String.IsNullOrEmpty(corrected_uri)))
            {
                _server = new Server(new ServerAddress(corrected_uri));
                _repository = new Repository(_server);
            }

            _connection = _repository.Connection;

            if (_state == ConnectionState.Unknown || _state == ConnectionState.Initialized || !_connection.connectionEstablished())
            {
                try
                {
                    _connection.UserName = "";
                    _connection.Client = new Client();
                    _connection.Client.Name = "";

                    _connection.Connect(null);
                    _state = ConnectionState.Connected;
                }
                catch (P4Exception ex)
                {
                    _state = ConnectionState.Initialized;
                    throw ex;
                }
            }
        }

        private bool IsPerforceRootFound()
        {
            return _perforceRoot != null && Directory.Exists(_perforceRoot);
        }

        private bool IsFileUnderPerforceRoot(string absolutePath)
        {
            Debug.Assert(IsPerforceRootFound());

            return absolutePath.StartsWith(_perforceRoot, StringComparison.OrdinalIgnoreCase);
        }

        private string GetPerforcePath(string absolutePath)
        {
            Debug.Assert(IsDiffPerformed(absolutePath));
            Debug.Assert(IsPerforceRootFound());

            string perforcePath = absolutePath.Substring(_perforceRoot.Length, absolutePath.Length - _perforceRoot.Length);
            perforcePath = perforcePath.Replace('\\', '/');
            perforcePath = perforcePath.Insert(0, "/");

            return perforcePath;
        }

        private PerforceCommands(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            try
            {
                RefreshConnection();
            }
            catch (P4Exception)
            {
                // nothing to do
            }
        }

        private bool CanGetDiff(string path)
        {
            return _state == ConnectionState.Success && IsFileUnderPerforceRoot(path);
        }

        #endregion
    }
}