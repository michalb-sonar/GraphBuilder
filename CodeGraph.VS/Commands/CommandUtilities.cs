using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Progression;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace CodeGraph.VS

{
    internal static class CommandUtilities
    {
        public static IGraphDocumentWindowPane CreateNewWindow(IServiceProvider serviceProvider, string fileNameTemplate)
        {
            // See:
            //private bool RunDocumentCreation(string filenameTemplate, WindowGraphType graphType)
            //Declaring Type: Microsoft.VisualStudio.Progression.GraphGenerationController 
            //Assembly: Microsoft.VisualStudio.Progression.Common, Version=11.0.0.0 

            IGraphDocumentWindowPane pane = null;

            IGraphDocumentManager service = serviceProvider.GetService(typeof(IGraphDocumentManager)) as IGraphDocumentManager;
            string uniqueWindowCaption = service.GetUniqueWindowCaption(fileNameTemplate);
            if (string.IsNullOrEmpty(uniqueWindowCaption))
            {
                uniqueWindowCaption = fileNameTemplate;
            }
            try
            {
                pane = service.CreateNewDgmlDocument(uniqueWindowCaption);
            }
            catch (Exception ex)
            {
                // Suppress non-critical exceptions
                if (ErrorHandler.IsCriticalException(ex))
                {
                    throw;
                }
            }
            return pane;
        }

        public static void ShowMessage(IServiceProvider serviceProvider, string title, string message, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                message = string.Format(System.Globalization.CultureInfo.CurrentCulture, message, args);
            }

            IVsUIShell uiShell = (IVsUIShell)serviceProvider.GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       title,
                       message,
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }


        public static void SafeExecute(IServiceProvider serviceProvider, string errorTitle, string ErrorMessage, System.Action op)
        {
            try
            {
                op();
            }
            catch(Exception ex)
            {
                if (ErrorHandler.IsCriticalException(ex))
                {
                    throw;
                }

                ShowMessage(serviceProvider, errorTitle, ErrorMessage + "\r\n" + ex.ToString());
            }
        }

    }
}
