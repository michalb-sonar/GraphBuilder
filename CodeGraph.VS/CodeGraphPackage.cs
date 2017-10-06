//------------------------------------------------------------------------------
// <copyright file="CodeGraphPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace CodeGraph.VS
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(CodeGraphPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class CodeGraphPackage : Package
    {
        /// <summary>
        /// CodeGraphPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "dd6de5ba-e13f-481f-8ec8-4cec1032031c";

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGraphPackage"/> class.
        /// </summary>
        public CodeGraphPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.

            RegisterAssemblyResolver();
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            CodeGraph.VS.ShowDeadCodeCommand.Initialize(this);
            CodeGraph.VS.ShowAllCodeCommand.Initialize(this);
        }

        #endregion

        #region HACK - Assembly resolution
        // VS2015.3: for some reason, the required version of System.Collections.Immutable isn't found, and there
        // doesn't presumably isn't an appropriate binding redirect for it.
        // Hacky workaround - resolve it to the first currently loaded version, if there is one.

        // Info: the NuGet package info for Microsoft.CodeAnalysis.Common v1.3.1 and v1.3.2 (http://www.nuget.org/packages/Microsoft.CodeAnalysis.Common/1.3.2)
        // lists System.Immutable.Collections v1.2.0 as a dependency.
        // However, Microsoft.CodeAnalysis.dll v1.3.1 (which ships in VS2015.U3) is built against
        // System.Immutable.Collections v1.1.37, which is what ships in VS and is what the binding
        // redirects in devenv.exe.config are targeting.
        // It looks like an error in the NuGet package dependencies.

        private static void RegisterAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;
        }

        private static void UnregisterAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= HandleAssemblyResolve;
        }

        private static System.Reflection.Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyName = args.Name;

            if (assemblyName.StartsWith("System.Collections.Immutable"))
            {
                Assembly asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith("System.Collections.Immutable"));

                if (asm != null)
                {
                    UnregisterAssemblyResolver();
                }

                return asm;
            }

            return null;
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UnregisterAssemblyResolver();
                }
                disposedValue = true;
            }
        }

        #endregion

    }
}
