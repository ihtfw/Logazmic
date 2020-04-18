using System;
using System.Threading.Tasks;

namespace Logazmic.Services
{
    public abstract class DialogService
    {

        #region Ambient Context

        private static DialogService _current;

        public static DialogService Current
        {
            get => _current ?? (_current = new MetroDialogService());
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (_current != null)
                    throw new InvalidOperationException("You cannot set ambient context twice");
                _current = value;
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
