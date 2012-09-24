using System;
using System.Windows.Forms;

namespace EasyResX
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.Run(new Form1());
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(string.Format(EasyResXResources.ApplicationExceptionFormat,
                e.Exception.Message), EasyResXResources.ApplicationExceptionMessage, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}