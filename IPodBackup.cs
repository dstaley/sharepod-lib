/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SharePodLib
{
	/// <summary>
	/// Provides static methods for backing up and restoring the iPod database.
	/// </summary>
    public static class IPodBackup
    {
        private static bool _backupPerformed;
		private static string _overrideBackupsFolder;
		private static int _numberBackupsToKeep = 1;
		private static bool _enableBackups = true;

		/// <summary>
		/// If not set, this defaults to [ApplicationData]\SharePod\Backups.
		/// </summary>
		public static string BackupsFolder
		{
			get { return _overrideBackupsFolder; }
			set { _overrideBackupsFolder = value; }
		}
		
		/// <summary>
		/// Number of backup files to keep before deleting old files.  
		/// Defaults to 1
		/// </summary>
		public static int NumberBackupsToKeep
		{
			get { return IPodBackup._numberBackupsToKeep; }
			set { IPodBackup._numberBackupsToKeep = value; }
		}

		/// <summary>
		/// Enable/disable backup creation.  By default this is set to true (Enabled)
		/// </summary>
		public static bool EnableBackups
		{
			get { return _enableBackups; }
			set { _enableBackups = value; }
		}

        /// <summary>
        /// Will backup the iPod's database (iTunesDB, ArtworkDB files) if it hasnt been backed up this session already.
        /// </summary>
        internal static void BackupDatabase(IPod iPod)
        {
            if (_backupPerformed || (_enableBackups == false))
                return;

            string backupFolder = GetBackupsFolder(iPod);
            
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(backupFolder);
                List<FileInfo> backupFiles = new List<FileInfo>(di.GetFiles("*DB_*.spbackup"));

                while (backupFiles.Count > (NumberBackupsToKeep - 1) * 2 && backupFiles.Count > 0)
                {
                    FileInfo oldestFile = backupFiles[0];
                    foreach (FileInfo backupFile in backupFiles)
                    {
                        if (backupFile.CreationTime < oldestFile.CreationTime)
                        {
                            oldestFile = backupFile;
                        }
                    }
                    oldestFile.Delete();
                    backupFiles.Remove(oldestFile);
                }
            }

			if (NumberBackupsToKeep > 0)
			{
                //Guid guid = Guid.NewGuid();
                //string backupName = backupFolder + "\\iTunesDB_" + guid.ToString() + ".spbackup";
                //iPod.FileSystem.CopyFileFromDevice(iPod.FileSystem.ITunesDBPath, backupName);
                //if (iPod.FileSystem.FileExists(iPod.FileSystem.ArtworkDBPath))
                //{
                //    backupName = backupFolder + "\\ArtworkDB_" + guid.ToString() + ".spbackup";
                //    iPod.FileSystem.CopyFileFromDevice(iPod.FileSystem.ArtworkDBPath, backupName);
                //}

				_backupPerformed = true;
			}
        }

        /// <summary>
        /// Returns a list of SharePodLib backup files (files called *.spbackup in backups folder)
        /// </summary>
        /// <returns></returns>
        public static FileInfo[] GetBackups(IPod iPod)
        {
            string backupFolder = GetBackupsFolder(iPod);

            if (!Directory.Exists(backupFolder))
            {
                return null;
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(backupFolder);
                return di.GetFiles("ITunesDB_*.spbackup");
            }
        }

        private static string GetBackupsFolder(IPod iPod)
        {
            //FirewireId should only ever be null for 3rd gen (old) iPods which dont support the SCSI device query
            string firewireId = iPod.DeviceInfo.FirewireId ?? "";

            if (_overrideBackupsFolder != null)
            {
                return Path.Combine(_overrideBackupsFolder, firewireId);
            }
            else
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SharePod\\Backups\\" + firewireId);
            }
        }

        /// <summary>
        /// Restores the iPod's database from the given file.
		/// Use with care - no checking is done to ensure this is a valid iTunesDB, ArtworkDB.
        /// See GetBackups() for available backups
        /// </summary>
        public static void RestoreDatabase(IPod iPod, string filename)
        {
            //string backupGuid = filename.Substring(filename.IndexOf("ITunesDB_") + 9, 36);
            //iPod.FileSystem.CopyFileToDevice(filename, iPod.FileSystem.ITunesDBPath);

            //string artBackupFile = Path.Combine(Path.GetDirectoryName(filename),  "ArtworkDB_" + backupGuid + ".spbackup");
            //if (File.Exists(artBackupFile))
            //{
            //    iPod.FileSystem.CopyFileToDevice(artBackupFile, iPod.FileSystem.ArtworkDBPath);
            //}
        }
    }
}
