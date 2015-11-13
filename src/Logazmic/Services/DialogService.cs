using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Logazmic.Services
{
    public abstract class DialogService
    {

        #region Ambient Context

        private static DialogService current;

        public static DialogService Current
        {
            get { return current ?? (current = new MetroDialogService()); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (current != null)
                    throw new InvalidOperationException("You cannot set ambient context twice");
                current = value;
            }
        }

        #endregion

        public void ShowErrorMessageBox(Exception e)
        {
           ShowErrorMessageBox(e.Message);
        }

        public void ShowErrorMessageBox(string message)
        {
            ShowMessageBox("Error", message);
        }
        
        public abstract void ShowMessageBox(string title, string message);

        public abstract bool ShowOpenDialog(out string path, string defaultExt, string filter, string initialDir = null);

        public abstract Task<string> ShowInputDialog(string title, string message);

        public abstract Task<bool?> ShowQuestionMessageBox(string title, string message);
    }
}   
