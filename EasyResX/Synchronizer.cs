using System;
using System.Collections;
using System.IO;
using System.Resources;

namespace ScrewTurn.ResxSynchronizer
{

	/// <summary>
	/// Allows to synchronize two RESX files.
	/// </summary>
	public class Synchronizer {

		string sourceFile, destinationFile;

		/// <summary>
		/// Initialises a new instance of the <b>Syncronizer</b> class.
		/// </summary>
		/// <param name="sFile">The source RESX file path.</param>
		/// <param name="dFile">The destination RESX file path.</param>
		/// <remarks>Both files must exist.</remarks>
		public Synchronizer(string sFile, string dFile) {
			sourceFile = sFile;
			destinationFile = dFile;
		}

		/// <summary>
		/// Performs the Synchronization of the RESX files.
		/// </summary>
		/// <param name="backup">Specifies whether to backup the destination file before modifying it.</param>
		/// <param name="addOnly">Specifies whether to only add new keys, without removing deleted ones.</param>
		/// <param name="verbose">Specifies whether to print additional information.</param>
		/// <param name="added">The number of added keys.</param>
		/// <param name="removed">The number of removed keys.</param>
		public void SyncronizeResources(bool backup, bool addOnly, bool verbose, out int added, out int removed) {
			added = 0;
			removed = 0;
			if(backup) {
				string destDir = Path.GetDirectoryName(destinationFile);
				string file = Path.GetFileName(destinationFile);
				File.Copy(destinationFile, destDir + "\\Backup of " + file, true);
			}

			string tempFile = Path.GetDirectoryName(destinationFile) + "\\__TempOutput.resx";

			// Load files in memory
			MemoryStream sourceStream = new MemoryStream(), destinationStream = new MemoryStream();
			FileStream fs;
			int read;
			byte[] buffer = new byte[1024];

			fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			read = 0;
			do {
				read = fs.Read(buffer, 0, buffer.Length);
				sourceStream.Write(buffer, 0, read);
			} while(read > 0);
			fs.Close();

			fs = new FileStream(destinationFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			read = 0;
			do {
				read = fs.Read(buffer, 0, buffer.Length);
				destinationStream.Write(buffer, 0, read);
			} while(read > 0);
			fs.Close();

			sourceStream.Position = 0;
			destinationStream.Position = 0;

			// Create resource readers
			ResXResourceReader source = new ResXResourceReader(sourceStream);
			ResXResourceReader destination = new ResXResourceReader(destinationStream);
			
			// Create resource writer
			if(File.Exists(tempFile)) File.Delete(tempFile);
			ResXResourceWriter writer = new ResXResourceWriter(tempFile);

			// Compare source and destination:
			// for each key in source, check if it is present in destination
			//    if not, add to the output
			// for each key in destination, check if it is present in source
			//    if so, add it to the output

			// Find new keys and add them to the output
			foreach(DictionaryEntry d in source) {
				bool found = false;
				foreach(DictionaryEntry dd in destination) {
					if(d.Key.ToString().Equals(dd.Key.ToString())) {
						// Found key
						found = true;
						break;
					}
				}
				if(!found) {
					// Add the key
					writer.AddResource(d.Key.ToString(), d.Value);
					added++;
					if(verbose) {
						Console.WriteLine("Added new key '" + d.Key.ToString() + "' with value '" + d.Value.ToString() + "'\n");
					}
				}
			}

			if(addOnly) {
				foreach(DictionaryEntry d in destination) {
					writer.AddResource(d.Key.ToString(), d.Value);
				}
			}
			else {
				int tot = 0;
				int rem = 0;
				// Find un-modified keys and add them to the output
				foreach(DictionaryEntry d in destination) {
					bool found = false;
					tot++;
					foreach(DictionaryEntry dd in source) {
						if(d.Key.ToString().Equals(dd.Key.ToString())) {
							// Found key
							found = true;
						}
					}
					if(found) {
						writer.AddResource(d.Key.ToString(), d.Value);
						rem++;
					}
					else if(verbose) {
						Console.WriteLine("Removed deleted key '" + d.Key.ToString() + "' with value '" + d.Value.ToString() + "'\n");
					}
				}
				removed = tot - rem;
			}

			source.Close();
			destination.Close();
			writer.Close();

			// Copy tempFile into destinationFile
			File.Copy(tempFile, destinationFile, true);
			File.Delete(tempFile);
		}

	}

}