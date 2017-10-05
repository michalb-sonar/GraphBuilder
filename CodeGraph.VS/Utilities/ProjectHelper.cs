using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace CodeGraph.VS
{
    internal static class ProjectHelper
    {
        public static Project TryGetActiveVsProject(IServiceProvider serviceProvider)
        {
            return GetSelectedItem(serviceProvider) as Project;
        }

        public static object GetSelectedItem(IServiceProvider serviceProvider)
        {
            object selectedObject = null;

            IntPtr hierarchyPtr;
            uint itemId;
            IVsMultiItemSelect multiSelect;
            IntPtr selectionContainerPtr;
            int hresult;

            var selectionService = serviceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

            hresult = selectionService.GetCurrentSelection(out hierarchyPtr, out itemId, out multiSelect, out selectionContainerPtr);
            if (!ErrorHandler.Succeeded(hresult))
            {
                return null;
            }

            try
            {
                IVsHierarchy hierarchy = Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof(IVsHierarchy)) as IVsHierarchy;

                if (hierarchy != null)
                {
                    hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out selectedObject);
                }
            }
            finally
            {
                Marshal.Release(hierarchyPtr);
            }

            return selectedObject;
        }

        public static Microsoft.CodeAnalysis.Project TryGetActiveRoslynProject(IServiceProvider serviceProvider)
        {
            Project project = ProjectHelper.TryGetActiveVsProject(serviceProvider);

            if (project == null)
            {
                return null;
            }

            string projectFilePath = project.FullName;
            if (projectFilePath == null)
            {
                return null;
            }

            IComponentModel mefService = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            VisualStudioWorkspace workspace = mefService.GetService<VisualStudioWorkspace>();

            Microsoft.CodeAnalysis.Project roslynProject = workspace.CurrentSolution.Projects.FirstOrDefault(
                p => projectFilePath.Equals(p.FilePath, StringComparison.InvariantCultureIgnoreCase));

            return roslynProject;
        }

    }
}
