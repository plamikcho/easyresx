using System;
using System.Windows.Forms;

namespace EasyResX
{
    public delegate void SetText(string text);
    public delegate string GetString();

    // thread-safe ToolStripStatusLabel
    public class ToolStripStatusLabelCrossThread : ToolStripStatusLabel
    {
        private string GetBaseText()
        {
            return base.Text;
        }

        private void SetBaseText(string text)
        {
            base.Text = text;
        }

        public override string Text
        {
            get
            {
                // Make sure that the container is already built
                if ((base.Parent != null) && (base.Parent.InvokeRequired))   // Is Invoke required?
                {
                    GetString getTextDel = new GetString(this.GetBaseText);
                    string text = String.Empty;
                    try
                    {
                        // Invoke the SetText operation from the 
                        // Parent of the ToolStripStatusLabel
                        text = (string)base.Parent.Invoke(getTextDel, null);
                    }
                    catch
                    {
                    }

                    return text;
                }
                else
                {
                    return base.Text;
                }
            }
            set
            {
                // Get from the container if Invoke is required
                // Make sure that the container is already built
                if ((base.Parent != null) && (base.Parent.InvokeRequired))   // Is Invoke required?     
                {
                    SetText setTextDel = new SetText(this.SetBaseText);
                    try
                    {
                        // Invoke the SetText operation from the
                        // Parent of the ToolStripStatusLabel
                        base.Parent.Invoke(setTextDel, new object[] { value });
                    }

                    catch
                    {
                    }
                }
                else
                {
                    base.Text = value;
                }
            }
        }
    }
}