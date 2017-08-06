using System;
using System.Threading;
using System.Windows.Forms;

namespace multiIDE
{
    static class Program
    {
        public static readonly string VersionSuffix = " pre-alpha";
        //
        public static IWorkplace TheWorkplace; // there can be only one Workplace in a program instance
        public static IMainForm TheMainForm; // MainForm is single too

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread.CurrentThread.Name = "Main Program Thread";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TheMainForm = new mdiMultiIDE();
            TheWorkplace = new Workplace(TheMainForm);

            Application.Run((Form)TheMainForm);
        }
    }
}
