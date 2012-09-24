using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Ionic.Zip;

namespace EasyResX
{
    /// <summary>
    /// Zipper class
    /// </summary>
    public class Zipper
    {
        public const string ZipDefaultExtension = "zip";

        public const string ZipFilter = "Zip archive (*.zip)|*.zip";

        // save byte array to file
        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
	    {
            // Open file for reading
            using (FileStream fileStream = new FileStream(fileName,
                    FileMode.Create, System.IO.FileAccess.Write))
            {
                try
                {
                    // Writes a block of bytes to this stream using data from a byte array.
                    fileStream.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    // Error
                    Console.WriteLine("Exception caught in process: {0}", ex.ToString());
                }
                finally
                {
                    // close file stream
                    fileStream.Close();
                }
            }            
    	    
	        // error occured, return false
	        return false;
	    }

        // extracts and merges previously created translator package into the project directory
        public static bool ExtractTranslatedZip(string fileName, string targetDir, bool action)
        {
            bool res = true;
            string dirSepWin = @"\", dirSep = "/";
            using (ZipFile z = new ZipFile(fileName))
            {                
                int dindex = targetDir.LastIndexOf(dirSepWin);
                string ddir = targetDir.Substring(dindex + 1, targetDir.Length - dindex - 1);
                foreach (var entry in z.Entries)                
                {
                    if (entry.IsDirectory)
                    {
                        continue;
                    }

                    string x = entry.FileName;
                    string nn = Regex.Replace(x, @"^(.+\/)*" + ddir + @"\/", string.Empty)
                                        .Replace(dirSep, dirSepWin);
                    string checkFile = targetDir + dirSepWin+ nn;
                    if (!File.Exists(checkFile))
                    {
                        res = false;
                        break;
                    }

                    if (action)
                    {
                        using (FileStream fs = new FileStream(checkFile, FileMode.Create, FileAccess.Write))
                        {
                            entry.Extract(fs);
                        }
                    }
                }
            }

            return res;
        }
                
        // zip filelist to folder
        public static byte[] ZipFolder(List<string> fileList)
        {
            byte[] outputBytes = new byte[] { 0 };
            if (fileList.Count > 0)
            {
                using (ZipFile z = new ZipFile())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {                        
                        z.AddFiles(fileList);
                        
                        z.Save(ms);
                        outputBytes = ms.GetBuffer();                        
                    }
                }
            }

            return outputBytes;
        }
    }
}