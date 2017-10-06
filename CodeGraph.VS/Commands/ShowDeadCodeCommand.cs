//------------------------------------------------------------------------------
// <copyright file="ShowDeadCodeCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using CodeGraph.Interfaces;
using CodeGraph.VS.Progression;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.Progression;
using Microsoft.VisualStudio.Shell;

namespace CodeGraph.VS
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ShowDeadCodeCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("00be1222-1134-4ee2-b589-a0ad9e099532");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowDeadCodeCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ShowDeadCodeCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.SafeOnShowDeadProject, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ShowDeadCodeCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new ShowDeadCodeCommand(package);
        }

        private void SafeOnShowDeadProject(object sender, EventArgs args)
        {
            CommandUtilities.SafeExecute(this.ServiceProvider,
                "Show dead code",
                "Error showing dead code",
                this.ShowDeadCode);
        }

        private void ShowDeadCode()
        {
            string message = "Error: unable to get current Roslyn project";

            Project activeProject = ProjectHelper.TryGetActiveRoslynProject(this.ServiceProvider);

            if (activeProject != null)
            {
                message = $@"Current project: {activeProject.Name}";
            }

            ICallGraph graph = null;

            ShowAsGraph(graph);
        }

        private void ShowAsGraph(ICallGraph callGraph)
        {
            // TODO:
            //Debug.Assert(callGraph != null, "Supplied call graph should not be null");
            //if (callGraph == null)
            //{
            //    return;
            //}

            Graph graph = CallGraphDgmlBuilder.Create(callGraph);
            IGraphDocumentWindowPane window = CommandUtilities.CreateNewWindow(this.ServiceProvider, "DeadCode{0}.dgml");
            window.Graph = graph;
        }

    }
}
