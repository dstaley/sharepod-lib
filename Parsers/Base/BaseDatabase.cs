/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharePodLib.Exceptions;

namespace SharePodLib.Parsers
{
    internal abstract class BaseDatabase
    {
        public event EventHandler DatabaseWritten;

        protected CompatibilityType _compatibility;
        protected IPod _iPod;
        protected string _databaseFilePath;
        
        public IPod iPod
        {
            get { return _iPod; }
        }

        public CompatibilityType Compatibility
        {
            get { return _compatibility; }
            set { _compatibility = value; }
        }

        public abstract int Version { get; }
        public abstract void Parse();
        public abstract void Save();
        public abstract bool IsDirty { get; }


        protected void ReadDatabase(BaseDatabaseElement root)
        {
            string parseFilePath = GetParseFileName();

            FileStream fs = new FileStream(parseFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);
            
            try
            {
                root.Read(iPod, reader);
                reader.Close();
                _compatibility = TestCompatibility(parseFilePath, root);
            }
            catch (Exception ex)
            {
                DebugLogger.LogException(ex);
                string message = string.Format("The iPod database '{0}' could not be read. Please run iTunes with your iPod connected, then re-open SharePod. \r\n(Error at 0x{1})", Path.GetFileName(_databaseFilePath), reader.BaseStream.Position.ToString("X"));
                throw new ParseException(message, ex);
            }
            finally
            {
                reader.Close();
                CleanUpParseFile(parseFilePath);
            }
        }

        protected void WriteDatabase(BaseDatabaseElement root)
        {
            string tempDB = Path.GetTempFileName();
            FileStream fs = new FileStream(tempDB, FileMode.Create, FileAccess.ReadWrite);
            BinaryWriter writer = new BinaryWriter(fs);
            root.Write(writer);
            writer.Flush();
            DoActionOnWriteDatabase(fs);

            if (fs.CanWrite)
            {
                writer.Flush();
            }
            writer.Close();
            
            //overwrite real database with temp
            _iPod.FileSystem.CopyFileToDevice(tempDB, _databaseFilePath);

            if (DatabaseWritten != null)
                DatabaseWritten(this, null);
        }

        public virtual void DoActionOnWriteDatabase(FileStream fileStream) { }


        public virtual void AssertIsWritable()
        {
            string msg;
            if (_compatibility == CompatibilityType.NotWritable)
            {
                msg = String.Format("Your iPod ({0}, database version {1}) is not writable.\r\n\r\nAll iPod update features are disabled, but you can still copy files to your computer.", _iPod.DeviceInfo.Family, Version);
                throw new UnsupportedIPodException(msg);
            }
            else if (_compatibility == CompatibilityType.UnsupportedNewDeviceOrFirmware)
            {
                msg = String.Format("Looks like you have a new iPod! This version of SharePod does not fully support it yet.\r\n\r\nYou can only copy files from your iPod to your computer.", _iPod.DeviceInfo.Family, Version);
                throw new UnsupportedIPodException(msg);
            }
            else if (_compatibility == CompatibilityType.SourceDoesntMatchOutput)
            {
                msg = "The iPod database failed to pass the SharePodLib compatibility test.  All iPod update features are disabled.  Upgrading the iPod to the latest iTunes version may fix the issue.";
                throw new UnsupportedITunesVersionException(msg, _compatibility);
            }
        }

        internal CompatibilityType TestCompatibility(string dbFilePath, BaseDatabaseElement root)
        {
            string tempDB = Path.GetTempFileName();
            FileStream fs = new FileStream(tempDB, FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(fs);
            root.Write(writer);
            writer.Close();
            return Helpers.TestCompatibility(dbFilePath, tempDB);
        }

        protected string GetParseFileName()
        {
            if (_iPod.FileSystem.ParseDbFilesLocally)
            {
                string parseFilePath = Path.GetTempFileName();
                _iPod.FileSystem.CopyFileFromDevice(_databaseFilePath, parseFilePath);
                return parseFilePath;
            }
            else
            {
                return _databaseFilePath;
            }
        }

        protected void CleanUpParseFile(string parseFileUsed)
        {
            if (_iPod.FileSystem.ParseDbFilesLocally)
            {
                File.Delete(parseFileUsed);
            }
        }
    }
}
