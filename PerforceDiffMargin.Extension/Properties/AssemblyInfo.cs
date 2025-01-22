using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("PerforceDiffMargin.Extension")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Andrii Radchenko")]
[assembly: AssemblyProduct("PerforceDiffMargin.Extension")]
[assembly: AssemblyCopyright("Andrii Radchenko")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e4701f35-8030-418e-8e8c-6ae72e229138")]

[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\PerforceDiffMargin.15.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\PerforceDiffMargin.16.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\PerforceDiffMargin.Commands.dll")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0.0")]
