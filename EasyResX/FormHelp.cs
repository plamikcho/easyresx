using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace EasyResX
{
    public partial class FormHelp : Form
    {
        public FormHelp()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Navigate(linkLabel1.Text);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Navigate(linkLabel2.Text);
        }

        private void Navigate(string url)
        {
            Process.Start(url);
        }

        private void FormHelp_Load(object sender, EventArgs e)
        {
            this.label1.Select();
        }
    }
}