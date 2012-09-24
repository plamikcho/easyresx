using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace EasyResX
{
    public partial class Form1 : Form
    {
        // processing ended
        public event ProcessingEndedEventHandler OnProcessingEnded;
        // folder patterns for searching resx
        private List<string> patterns;
        // zipped file with exported translation
        private byte[] zippedPackage;
        // status label
        private ToolStripStatusLabel toolStripStatusLabel;
        // save file dialog
        private SaveFileDialog saveFileDlg;
        // to fix the strange dropdown behavior on create package
        private bool falseTabEnter;        
        // separate thread for some of the processing
        private Thread thr;

        public Form1()
        {
            InitializeComponent();
            toolStripStatusLabel = new ToolStripStatusLabelCrossThread();
            saveFileDlg = new SaveFileDialog();
            this.statusStrip1.Items.Add(toolStripStatusLabel);
            this.falseTabEnter = false;
        }

        #region Form controls events

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.ReadOnly = true;
            folderBrowserDialog1.SelectedPath = 
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.openFileDialog1.InitialDirectory = 
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.openFileDialog1.FileName = string.Empty;
            this.openFileDialog1.Title = EasyResXResources.MergeFileDialogTitle;

            toolStripStatusLabel.Text = EasyResXResources.ReadyMessage;

            patterns = new List<string>(EasyResXConfiguration.CurrentConfig.OperatedFolders);
            
            OnProcessingEnded += new ProcessingEndedEventHandler(Form1_OnProcessingEnded);
            saveFileDlg.FileOk +=new CancelEventHandler(saveFileDlg_FileOk);
        }
                
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (null != thr)
            {
                if (thr.IsAlive || thr.ThreadState != ThreadState.Stopped)
                {
                    thr.Abort();
                }
            }
        }

        // load project folder dialog
        private void button1_Click(object sender, EventArgs e)
        {            
            DialogResult fdr = this.folderBrowserDialog1.ShowDialog();
            if (fdr == DialogResult.OK)
            {
                label1.Text = folderBrowserDialog1.SelectedPath;
            }            
        }

        // synchronize
        private void button2_Click(object sender, EventArgs e)
        {
            StartProcessing();
            thr = new Thread(new ThreadStart(ProcessSync));
            thr.Start();
        }

        // create package
        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                StartProcessing();
                thr = new Thread(new ParameterizedThreadStart(ProcessPackage));
                thr.TrySetApartmentState(ApartmentState.STA); // this fixes saveFileDialog exception
                thr.Start(comboBox1.SelectedItem);
            }
        }

        // Load back for merging
        private void button5_Click(object sender, EventArgs e)
        {
            if (this.OpenMergeDialog())
            {
                string message = EasyResXResources.MergePackageResultFail;
                if (Zipper.ExtractTranslatedZip(this.openFileDialog1.FileName, this.label1.Text, true))
                {
                    message = EasyResXResources.MergePackageResultOk;
                }

                FormCommonUtils.AppendToTextBox(message, this.textBox1);
            }
        }

        // create new culture
        private void button6_Click(object sender, EventArgs e)
        {            
            if (comboBox2.SelectedItem != null) 
            {
                StartProcessing();
                thr = new Thread(new ParameterizedThreadStart(ProcessNewCulture));            
                thr.Start(comboBox2.SelectedItem);
            }
        }

        // help open
        private void button7_Click(object sender, EventArgs e)
        {
            var fh = new FormHelp();
            fh.ShowDialog();
        }

        // available cultures for packaging
        private void tabPage2_Enter(object sender, EventArgs e)
        {
            CheckBeforeStart();
            ProcessAvailableCultures();
        }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
            FormCommonUtils.ClearTextBox(textBox1);
        }

        // available cultures to create
        private void tabPage4_Enter(object sender, EventArgs e)
        {
            CheckBeforeStart();
            ProcessAvailableCultures();

            comboBox2.Items.Clear();
            List<string> availableCultures = new List<string>();
            foreach (var availableCulture in comboBox1.Items)
            {
                availableCultures.Add(ResXOperator.GetCultureFromCombo(availableCulture.ToString()));
            }

            foreach (var culture in ResXOperator.GetCulturesList(CultureTypes.FrameworkCultures))
            {
                if (!comboBox2.Items.Contains(culture) && !availableCultures.Contains(culture)) // 
                {
                    comboBox2.Items.Add(culture);
                }
            }

            this.falseTabEnter = false;
        }

        // operation ended
        private void Form1_OnProcessingEnded(object sender, ProcessingEndedEventArgs e)
        {
            FormCommonUtils.ControlEnabled(this, true);
            toolStripStatusLabel.Text = EasyResXResources.ReadyMessage;

            if (e.OperationMode == ResXOperationModes.CreatePackage)
            {
                OpenSaveDialog(e.SelectedCulture);
            }
        }

        // save file to disk
        private void saveFileDlg_FileOk(object sender, CancelEventArgs e)
        {
            if (!Zipper.ByteArrayToFile(saveFileDlg.FileName, this.zippedPackage))
            {
                FormCommonUtils.MyMessageBox(EasyResXResources.SaveFileDlgCaption, EasyResXResources.SaveFileDlgMessage);
            }
        }
 
        #endregion

        #region Helper methods

        // save file dialog setup
        private void OpenSaveDialog(string selectedCulture)
        {
            saveFileDlg.FileName = string.Format(EasyResXResources.PackageFormat,
                            ResXOperator.GetCultureFromCombo(selectedCulture).ToLower());
            saveFileDlg.InitialDirectory = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments);
            saveFileDlg.DefaultExt = Zipper.ZipDefaultExtension;
            saveFileDlg.Filter = Zipper.ZipFilter; // throws exception in case of wrong format
            var dlr = saveFileDlg.ShowDialog();
            if (dlr == DialogResult.OK)
            {
                FormCommonUtils.AppendToTextBox(EasyResXResources.CreatePackageOk, textBox1);
            }
        }

        // open file dialog for merge action
        private bool OpenMergeDialog()
        {
            this.openFileDialog1.DefaultExt = Zipper.ZipDefaultExtension;
            this.openFileDialog1.Filter = Zipper.ZipFilter;
            return (this.openFileDialog1.ShowDialog() == DialogResult.OK);
        }

        // print status
        private void PrintStatus(string text)
        {
            FormCommonUtils.AppendToTextBox(text + "\r\n", this.textBox1);
        }

        // executed in main thread
        private void CheckBeforeStart()
        {
            if (this.label1.Text.Length < 2)
            {
                throw new Exception(EasyResXResources.LoadProjectExceptionMessage);
            }

            FormCommonUtils.SetControlText(string.Empty, textBox1);
        }

        // executed in all threads
        private void StartProcessing()
        {
            CheckBeforeStart();
            toolStripStatusLabel.Text = EasyResXResources.ProcessingMessage;
            FormCommonUtils.ControlEnabled(this, false);
        }

        // reactivate controls
        private void EndProcessing(ResXOperationModes operationMode, string selectedCulture)
        {
            if (OnProcessingEnded != null)
            {
                OnProcessingEnded(this, new ProcessingEndedEventArgs(operationMode, selectedCulture));
            }
        }

        private void EndProcessing(ResXOperationModes operationMode)
        {
            EndProcessing(operationMode, string.Empty);
        }

        // get available cultures from resx
        private void ProcessAvailableCultures()
        {
            bool flag = false;
            if (this.InvokeRequired)
            {
                bool res = false;
                var action = new Action<Form1>(c => res = c.falseTabEnter);
                this.Invoke(action, this);
                flag = res;
            }

            flag = this.falseTabEnter;

            if(flag) 
            {
                return;
            }

            ResXOperator fo = new ResXOperator(patterns,
                new ResXOperationSettings
                {
                    OperationMode = ResXOperationModes.GetAvailableCultures
                },
                    this.PrintStatus);
            fo.ProcessDirectory(label1.Text);
            foreach (var item in fo.AvailableCultures)
            {
                if (!comboBox1.Items.Contains(item))
                {
                    var itemtoadd = new KeyValuePair<string, string>(item.Key, item.Value);
                    comboBox1.Items.Add(itemtoadd);
                    if (item.Key.ToLower().Equals("default"))
                    {
                        comboBox1.SelectedItem = itemtoadd;
                    }
                }
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action<Form1>(c => c.falseTabEnter = true));
            }
            else 
            {
                this.falseTabEnter = true;
            }

            EndProcessing(ResXOperationModes.GetAvailableCultures);
        }

        // create package for selected culture
        private void ProcessPackage(object selectedCulture)
        {
            ResXOperator fo = new ResXOperator(patterns,
                new ResXOperationSettings
                {
                    OperationMode = ResXOperationModes.CreatePackage,
                    SelectedCulture = selectedCulture.ToString()
                },
                this.PrintStatus);
            fo.ProcessDirectory(label1.Text);
            zippedPackage = Zipper.ZipFolder(fo.ResXFiles);
            EndProcessing(ResXOperationModes.CreatePackage, selectedCulture.ToString());
        }

        // synchronize resx
        private void ProcessSync()
        {            
            ResXOperator fo = new ResXOperator(patterns,
                new ResXOperationSettings
                {
                    OperationMode = ResXOperationModes.Synchronize,
                    Backup = checkBox1.Checked,
                    AddOnly = checkBox2.Checked
                },
                    this.PrintStatus);

            fo.ProcessDirectory(label1.Text);
            EndProcessing(ResXOperationModes.Synchronize);
        }

        // create new culture
        private void ProcessNewCulture(object selectedCulture)
        {
            ResXOperator fo = new ResXOperator(patterns,
                new ResXOperationSettings
                {
                    OperationMode = ResXOperationModes.GetAvailableCultures
                },
                    this.PrintStatus);
            fo.ProcessDirectory(label1.Text);

            fo.ChangeOperationSettings(
                new ResXOperationSettings
                {
                    OperationMode = ResXOperationModes.CreateNewCulture,
                    SelectedCulture = selectedCulture.ToString()
                });

            fo.ProcessDirectory(label1.Text);
            EndProcessing(ResXOperationModes.CreateNewCulture, selectedCulture.ToString());
        }

        #endregion        
    }
}