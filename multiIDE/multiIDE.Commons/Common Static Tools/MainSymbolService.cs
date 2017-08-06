using System;
using System.Windows.Forms;

namespace multiIDE
{
    #region Events' members
    public class ShowMessageEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public string Caption { get; private set; }
        public MessageBoxIcon Icon { get; private set; }

        public ShowMessageEventArgs(string message, string caption = "", MessageBoxIcon icon = MessageBoxIcon.None)
        {
            Message = message;
            Caption = caption;
            Icon = icon;
        }
    }
    //
    public class AskUserEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public string Caption { get; private set; }
        public MessageBoxButtons Buttons { get; private set; }
        public MessageBoxDefaultButton DefaultButton { get; private set; }
        public MessageBoxIcon Icon { get; private set; }
        //
        public DialogResult Answer { get; set; } = DialogResult.None;

        public AskUserEventArgs(string message, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            Message = message;
            Caption = caption;
            Buttons = buttons;
            DefaultButton = defaultButton;
            Icon = icon;
        }
    }
    //
    public class SaveFileByUserEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public string InitialDirectory { get; set; }
        public string Filter { get; set; }
        public int FilterIndex { get; set; }
        public bool RestoreDirectory { get; set; }
        //
        public DialogResult Answer { get; set; } = DialogResult.None;
    }
    //
    public class OpenFileByUserEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public string InitialDirectory { get; set; }
        public string Filter { get; set; }
        public int FilterIndex { get; set; }
        public bool RestoreDirectory { get; set; }
        //
        public DialogResult Answer { get; set; } = DialogResult.None;
    }

    #endregion

    public static class MainSymbolService
    {
        #region Message service subs
        public static void ShowMessage(object sender, ShowMessageEventArgs e)
        {
            MessageBox.Show(e.Message, e.Caption, MessageBoxButtons.OK, e.Icon);
        }

        public static void AskUser(object sender, AskUserEventArgs e)
        {
            e.Answer = MessageBox.Show(e.Message, e.Caption, e.Buttons, e.Icon, e.DefaultButton);
        }

        public static void SaveFileByUser(object sender, SaveFileByUserEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.FileName = e.FileName;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.InitialDirectory = e.InitialDirectory;
            saveFileDialog.Filter = e.Filter;
            saveFileDialog.FilterIndex = e.FilterIndex;
            saveFileDialog.RestoreDirectory = e.RestoreDirectory;

            e.Answer = saveFileDialog.ShowDialog();
        }

        public static void OpenFileByUser(object sender, OpenFileByUserEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.FileName = e.FileName;
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = e.InitialDirectory;
            openFileDialog.Filter = e.Filter;
            openFileDialog.FilterIndex = e.FilterIndex;
            openFileDialog.RestoreDirectory = e.RestoreDirectory;

            e.Answer = openFileDialog.ShowDialog();
            e.FileName = openFileDialog.FileName;
        }
        #endregion
    }
}
