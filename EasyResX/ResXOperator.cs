using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ResxWebEditor.Code;
using ScrewTurn.ResxSynchronizer;

[assembly: CLSCompliant(true)]
namespace EasyResX
{
    // message sender to the UI
    public delegate void MessageCaller(string text);

    /// <summary>
    /// Resx utility class
    /// </summary>
    public class ResXOperator
    {
        // delegate member
        private MessageCaller mMessage;

        private ResXOperator()
        {
        }

        // ctor
        public ResXOperator(List<string> patterns, ResXOperationSettings operationSettings, MessageCaller print)
        {
            this.AvailableCultures = new Dictionary<string, string>();
            this.ResXFiles = new List<string>();
            this.PatternList = patterns;
            this.OperationSettings = operationSettings; 
            this.mMessage = print;
        }

        // directory patterns
        public List<string> PatternList { get; private set; }

        // operation settings class
        public ResXOperationSettings OperationSettings { get; private set; }

        // available cultures found in all resx files
        public Dictionary<string, string> AvailableCultures { get; private set; }

        // processed resx files
        public List<string> ResXFiles { get; private set; }

        // set new operation settings
        public void ChangeOperationSettings(ResXOperationSettings operationSettings)
        {
            this.OperationSettings = operationSettings;
        }

        // processing method
        public void ProcessDirectory(string dir)
        {
            foreach (var pattern in PatternList)
            {
                this.TraverseDirectories(new DirectoryInfo(dir), pattern);
            }
        }

        // remove unneeded characters from full culture name
        public static string GetCultureFromCombo(string comboItem)
        {
            var ar = comboItem.Split(new char[] { ',' });
            return ar[0].Trim(new char[] { '[', ']', ',' });
        }

        // gets culture list for the chosen culture alphabetically ordered
        public static List<string> GetCulturesList(CultureTypes cultureType)
        {
            List<string> cultures = new List<string>();
            CultureInfo.GetCultures(cultureType).ToList().ForEach(ci =>
            {
                if (!string.IsNullOrEmpty(ci.Name))
                {
                    cultures.Add(ci.Name);
                }
            });
            cultures.Sort();

            return cultures;
        }

        // process directories
        protected void TraverseDirectories(DirectoryInfo dir, string pattern)
        {
            List<string> processed = new List<string>();
            // Subdirs
            try         // Avoid errors such as "Access Denied"
            {
                foreach (DirectoryInfo iInfo in dir.GetDirectories())
                {
                    if (iInfo.Name.StartsWith(pattern))
                    {
                        this.mMessage(EasyResXResources.EnteringFolderMessage + iInfo.FullName);                        
                    }

                    TraverseDirectories(iInfo, pattern);
                }
            }
            catch (Exception ex)
            {
                mMessage(ex.ToString());
            }

            ProcessFiles(dir, processed);
        }

        // process files
        protected void ProcessFiles(DirectoryInfo dir, List<string> processed)
        {
            // Subfiles
            try         // Avoid errors such as "Access Denied"
            {
                foreach (FileInfo iInfo in dir.GetFiles())
                {
                    if (Path.GetExtension(iInfo.FullName).ToLower().EndsWith("resx"))
                    {
                        // default culture resx file
                        string defaultFile = Path.Combine(dir.FullName,
                            ResXUnified.GetBaseName(iInfo.FullName) + ".resx");
                        
                        // only the culture name
                        string culture = ResXUnified.FindCultureInFilename(iInfo.FullName);

                        switch (this.OperationSettings.OperationMode)
                        {
                            case ResXOperationModes.GetAvailableCultures:
                                this.AddCultureToAvailables(culture);

                                break;

                            case ResXOperationModes.CreateNewCulture:
                                if (this.ContinueCondition(processed, defaultFile))
                                {
                                    continue;
                                }

                                // build target file name
                                var targetFile = Path.Combine(dir.FullName,ResXUnified.GetBaseName(iInfo.FullName) + 
                                    "." + OperationSettings.SelectedCulture + ".resx");
                                // copy default resx to the new resx
                                    if (!File.Exists(targetFile) && File.Exists(defaultFile))
                                {
                                    File.Copy(defaultFile, targetFile, false);
                                    mMessage(targetFile);
                                }

                                processed.Add(defaultFile);

                                break;

                            case ResXOperationModes.Synchronize:
                                if (this.ContinueCondition(processed, defaultFile))
                                {
                                    continue;
                                }
                                // some kind of bug here with the full path concatenation - quick & dirty fix
                                if (File.Exists(defaultFile))
                                {
                                    mMessage(defaultFile);
                                    processed.Add(defaultFile);
                                    SynchronizeMultipleFiles(defaultFile, this.OperationSettings.Backup,
                                        this.OperationSettings.AddOnly, true);
                                }

                                break;

                            case ResXOperationModes.CreatePackage:
                                // buffer default resx culture file
                                string translatedFile = defaultFile;
                                // get selected culture for exporting
                                string selectedCulture = GetCultureFromCombo(OperationSettings.SelectedCulture);
                                // process all cultures except the default
                                if (culture.ToLower() != "default")
                                {
                                    translatedFile = Path.Combine(dir.FullName,
                                        ResXUnified.GetBaseName(iInfo.FullName) + "." + culture + ".resx");
                                }

                                if (this.ContinueCondition(ResXFiles, translatedFile))
                                {
                                    continue;
                                }

                                if (selectedCulture.ToLower().Equals(culture.ToLower()))
                                {
                                    ResXFiles.Add(translatedFile);
                                }

                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mMessage(ex.ToString()); // send message to the status textbox
            }
        }

        // code taken from screwturn
        protected void SynchronizeMultipleFiles(string source, bool backup, bool addOnly, bool verbose)
        {
            // Prepare filelist
            string[] files = Directory.GetFiles(
                Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source) + ".*.resx");

            Synchronizer sync;
            int added = 0, removed = 0, addedTemp, removedTemp;

            // Iterate over the files
            for (int i = 0; i < files.Length; i++)
            {
                mMessage(EasyResXResources.SynchronizingFileMessage + " " + Path.GetFileName(files[i]));

                sync = new Synchronizer(source, files[i]);
                addedTemp = 0;
                removedTemp = 0;
                try
                {
                    sync.SyncronizeResources(backup, addOnly, verbose, out addedTemp, out removedTemp);
                }
                catch
                {
                    mMessage(EasyResXResources.SynchronizingErrorMessage + "\r\n   " +
                        source + "\r\n   " + files[i]);
                }
                added += addedTemp;
                removed += removedTemp;
                if (addedTemp != 0 || removedTemp != 0)
                {
                    mMessage("   " + addedTemp.ToString() +
                    " " + EasyResXResources.KeysAddedMessage + " " +
                    removedTemp.ToString() + " " + EasyResXResources.KeysRemovedMessage);
                }
            }

            if (added != 0 || removed != 0)
            {
                mMessage(EasyResXResources.SynchronizationCompleteMessage + "   " +
                    added.ToString() + " " + EasyResXResources.KeysAddedMessage + "  " +
                    removed.ToString() + " " + EasyResXResources.KeysRemovedMessage);
            }
        }
        
        // add culture to list of available ones
        private void AddCultureToAvailables(string culture)
        {
            if (!AvailableCultures.ContainsKey(culture))
            {
                if (culture.ToLower() == "default")
                {
                    AvailableCultures.Add(culture, string.Empty);
                }
                else
                {
                    AvailableCultures.Add(culture, new CultureInfo(culture).EnglishName);
                }
            }       
        }
        
        // check for continue condition
        private bool ContinueCondition(List<string> processed, string defaultFile)
        {
            if (processed.Contains(defaultFile) ||
                                    defaultFile.ToLower().Contains(EasyResXResources.BackupOfMessage))
            {
                return true;
            }

            return false;
        }
    }
}