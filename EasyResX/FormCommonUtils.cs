using System;
using System.Threading;
using System.Windows.Forms;

namespace EasyResX
{
    /// <summary>
    /// Gui utils
    /// </summary>
    public class FormCommonUtils
    {
        public static void ControlEnabled(Control ctrl, bool isDisable)
        {
            ControlEnabled(ctrl, isDisable, true);
        }

        public static void ControlEnabled(Control ctrl, bool isDisable, bool allowRecurse)
        {
            if ((allowRecurse && ctrl.HasChildren))
            {
                foreach (Control c in ctrl.Controls)
                {
                    ControlEnabled(c, isDisable, true);
                }
            }

            if (!(ctrl is Form))
            {
                ActivateControl(ctrl, isDisable);                
            }
        }

        public static void SetControlText(string text, Control ctrl)
        {
            if (ctrl.InvokeRequired)
            {
                ctrl.BeginInvoke(
                    new MethodInvoker(delegate() { SetControlText(text, ctrl); })
                );
            }
            else
            {
                ctrl.Text = text;
            }
        }

        public static void AppendToTextBox(string text, TextBox txtBox)
        {
            if (txtBox.InvokeRequired)
            {
                txtBox.BeginInvoke(
                    new MethodInvoker(delegate() { AppendToTextBox(text, txtBox); })
                );
            }
            else
            {
                txtBox.AppendText(text);
            }
        }
        
        // clear textbox text
        public static void ClearTextBox(TextBox txtBox)
        {
            SetControlText(string.Empty, txtBox);
        }
        
        // prints symbol to indicate progress on long thread execution
        public static void PrintStatusCustom(Thread thr, TextBox txt, string symbol)
        {
            int i = 0, j = 0;
            AppendToTextBox("\r\n", txt);
            while (thr.ThreadState != System.Threading.ThreadState.Stopped)
            {
                if (i == 50)
                {
                    i = 0;
                    if (j == 40)
                    {
                        AppendToTextBox("\r\n", txt);
                        j = 0;
                    }
                    AppendToTextBox(symbol, txt);
                }
                Thread.Sleep(10);
                Application.DoEvents();
                i++; j++;
            }
        }
        
        // confirm dialog
        public static bool ConfirmDialog(string text, string caption)
        {
            bool res = false;
            DialogResult dlr = MessageBox.Show(text, caption,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dlr == DialogResult.Yes)
            {
                res = true;
            }

            return res;
        }
        
        // is form alive
        public static bool FormAliveCheck(string aTarget)
        {
            bool flagAlive = false;
            
            FormCollection frmCol = Application.OpenForms;
            foreach (Form frm in frmCol)
            {
                if (string.Compare(frm.Name, aTarget, true) == 0)
                {
                    flagAlive = true;
                    break;
                }
            }

            return flagAlive;
        }

        // predefined message box
        public static void MyMessageBox(string aCaption, string aText)
        {
            MessageBox.Show(aText, aCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // activate / deactivate control
        public static void ActivateControl(Control aControl, bool aActive)
        {
            if (aControl.InvokeRequired)
            {
                aControl.BeginInvoke(
                    new MethodInvoker(delegate() { ActivateControl(aControl, aActive); })
                );
            }
            else
            {
                aControl.Enabled = aActive;
            }
        }
    }
}