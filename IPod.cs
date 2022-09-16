/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers.iTunesDB;
using SharePodLib.Parsers.Artwork;
using SharePodLib.Parsers.PlayCounts;
using SharePodLib.Parsers.iTunesSD;
using System.IO;
using SharePodLib.Forms;
using SharePodLib.Exceptions;
using SharePodLib.Parsers;
using SharePodLib.IPodDevice.FileSystems;
using System.Diagnostics;
using System.Drawing;

namespace SharePodLib
{
    /// <summary>
    /// Enumeration of the actions SharePodLib can take when loading the iPod.
    /// </summary>
    public enum IPodLoadAction
    {
        /// <summary>
        /// Don't do any synchronisation
        /// </summary>
        NoSync,
        /// <summary>
        /// Sync the Play Counts file (contains the number of times each song has been played since last docked)
        /// </summary>
        SyncPlayCounts
    }

    /// <summary>
    /// Represents an Apple iPod device  
    /// </summary>
    public class IPod
    {
        private MusicDatabase _musicDatabase;
        private ArtworkDB _artworkDB;
        private PhotoDB _photoDB;
        private DeviceFileSystem _fileSystem;
        private IDeviceInfo _deviceInfo;
        private List<SupportedArtworkFormat> _supportedArtworkFormats = new List<SupportedArtworkFormat>();
        private IdGenerator _idGenerator;
        internal Session Session { get; private set; }

        #region Properties

        /// <summary>
        /// List of all tracks on the iPod.  Use this to add/remove/enumerate tracks
        /// </summary>
        public TrackList Tracks
        {
            get { return _musicDatabase.TracksList; }
        }

        /// <summary>
        /// List of all photo albums on the iPod.
        /// </summary>
        public ImageAlbumList Photos
        {
            get { return _photoDB.PhotoAlbumList; }
        }

        /// <summary>
        /// List of all playlists on the iPod.  Use this to add/remove/enumerate playlists
        /// </summary>
        public PlaylistList Playlists
        {
            get { return _musicDatabase.PlaylistsList; }
        }

        internal ArtworkDB ArtworkDB
        {
            get { return _artworkDB; }
        }

        /// <summary>
        /// Version number of the iPod's iTunesDB database.
        /// </summary>
        public int DatabaseVersion
        {
            get { return _musicDatabase.DatabaseRoot.VersionNumber; }
        }


        /// <summary>
        /// Type and generation of iPod (Mini, Shuffle, Video etc.)
        /// Only supported for later-model iPods which support the SCSI data enquiry command.
        /// </summary>
        public IDeviceInfo DeviceInfo
        {
            get { return _deviceInfo; }
        }

        /// <summary>
        /// The DeviceFileSystem is an abstraction of the file system used by a specific iPod.  Currently, this is either standard or iPhone
        /// </summary>
        public DeviceFileSystem FileSystem
        {
            get { return _fileSystem; }
        }

        internal MusicDatabase ITunesDB
        {
            get { return _musicDatabase; }
        }

        internal IdGenerator IdGenerator
        {
            get { return _idGenerator; }
        }

        #endregion


        internal IPod(DeviceFileSystem fileSystem, IPodLoadAction loadAction)
        {
            _fileSystem = fileSystem;
            _fileSystem.IPod = this;
            Trace.WriteLine("iPod found, profile: " + _fileSystem.Name);

            Refresh();

            if (_deviceInfo.Family == IPodFamily.IPhone_ITouch4)
                IsWritable = false;

            if (loadAction == IPodLoadAction.SyncPlayCounts)
            {
                PlayCounts playCounts = new PlayCounts(_musicDatabase);
                playCounts.MergeChanges();
            }

            Session = new Session(this);
        }

        /// <summary>
        /// Reloads all iPod databases. Useful if you know another app has updated the iPod since SharePod loaded it. (As a general rule, however, 
        /// you should avoid the situation of different apps reading/updating the iPod at one time!)
        /// </summary>
        public void Refresh()
        {
            _deviceInfo = _fileSystem.QueryDeviceInfo(this);

            _musicDatabase = new MusicDatabase(this);

            _musicDatabase.Parse();


            // Database version 25 is where the firewireId started being required for generating the iTunesDB hash.  If we couldn't get it,
            // throw an exception here. (Wrong hash value -> no songs shown on the iPod)
            if (_musicDatabase.Version >= 25)
            {
                if (_deviceInfo.ReadException != null)
                {
                    throw _deviceInfo.ReadException;
                }
            }

            _artworkDB = new ArtworkDB(this);
            _artworkDB.Parse();

            _photoDB = new PhotoDB(this);
            _photoDB.Parse();

            _idGenerator = new IdGenerator(this);
        }

        /// <summary>
        /// Opens the ipod_control\iTunes\iTunesLock file and locks it for exclusive access.  This means if the user tries to 'Safely remove hardware'
        /// from the system tray, Windows will disallow removal.  Otherwise, the device will likely be able to be disconnected.
        /// Note: You must call ReleaseLock() when you're done with the iPod.
        /// </summary>
        public void AcquireLock()
        {
            _fileSystem.AcquireLock();
        }

        /// <summary>
        /// Closes and deletes the iTunesLock file locked in the AcquireLock() call.
        /// </summary>
        public void ReleaseLock()
        {
            _fileSystem.ReleaseLock();
        }


        /// <summary>
        /// Drive the iPod is using.
        /// </summary>
        internal string DriveLetter
        {
            get { return _fileSystem.DriveLetter; }
        }

        /// <summary>
        /// Save changes to iPod database (DeviceFileSystem.iTunesDBPath, DeviceFileSystem.ArtworkDBPath).
        /// This will only perform a save if changes have been made, otherwise will immediately return.
        /// </summary>
        public void SaveChanges()
        {
            try
            {
                if (_musicDatabase.IsDirty)
                {

                    _fileSystem.StartSync();
                    _musicDatabase.Save();

                    Session.Clear();

                    if (_deviceInfo.Family == IPodFamily.iPod_Shuffle_Gen1 || _deviceInfo.Family == IPodFamily.iPod_Shuffle_Gen2)
                    {
                        ITunesSD iTunesSD = new ITunesSD(this);
                        iTunesSD.Backup();
                        iTunesSD.Generate();
                    }
                }

                if (_artworkDB.IsDirty)
                {
                    _fileSystem.StartSync();
                    _artworkDB.Save();
                }
            }
            finally
            {
                _fileSystem.EndSync();
            }
        }

        /// <summary>
        /// True if changes can be made to iPod.
        /// </summary>
        public bool IsWritable
        {
            get { return _musicDatabase.Compatibility == CompatibilityType.Compatible; }

            set
            {
                if (value)
                    _musicDatabase.Compatibility = CompatibilityType.Compatible;
                else
                    _musicDatabase.Compatibility = CompatibilityType.NotWritable;
            }
        }

        /// <summary>
        /// Throws UnsupportedITunesVersionException if writing is not supported.
        /// </summary>
        public void AssertIsWritable()
        {
            _musicDatabase.AssertIsWritable();
        }

        /// <summary>
        /// Returns true if there are in-memory changes that will be persisted to the iPod when SaveChanges() is called.
        /// </summary>
        public bool NeedsSaving
        {
            get { return _musicDatabase.IsDirty; }
        }


        /// <summary>
        /// Eject iPod from Windows.
        /// If you try to Eject the iPod, and your code is running on the iPod, an exception will be thrown directing the
        /// user to close the app and use the standard windows feature.
        /// </summary>
        public void Eject()
        {
            ReleaseLock();
            _fileSystem.Eject();
        }

        public void StartSync()
        {

            _fileSystem.StartSync();
        }

        public override string ToString()
        {
            return String.Format("{0} ({1})", _deviceInfo.Family.ToString(), _fileSystem.DriveLetter);
        }

        #region Statics

        /// <summary>
        /// Returns the first detected iPod.
        /// Do not sync PlayCounts file.
        /// </summary>
        /// <returns>IPod</returns>
        public static IPod GetConnectedIPod()
        {
            return GetConnectedIPod(IPodLoadAction.NoSync);
        }
        /// <summary>
        /// Returns the first detected iPod.
        /// </summary>
        public static IPod GetConnectedIPod(IPodLoadAction action)
        {
            foreach (DeviceFileSystem deviceFileSystem in SharePodLib.RegisteredFileSystems)
            {
                DeviceFileSystem connectedFS = deviceFileSystem.GetDevice();
                if (connectedFS != null)
                {
                    return new IPod(connectedFS, action);
                }
            }
            throw new IPodNotFoundException("No iPod could be found on your computer");
        }

        /// <summary>
        /// Returns all connected iPods. This is slower than GetConnectediPod as it must enumerate all drives, so only use if you know you need to
        /// handle multiple iPods at once.
        /// </summary>
        public static List<IPod> GetAllConnectedIPods(IPodLoadAction action)
        {
            List<IPod> iPods = new List<IPod>();
            foreach (DeviceFileSystem deviceFileSystem in SharePodLib.RegisteredFileSystems)
            {
                List<DeviceFileSystem> connections = deviceFileSystem.GetAllDevices();
                foreach (DeviceFileSystem connection in connections)
                {
                    iPods.Add(new IPod(connection, action));
                }
            }
            return iPods;
        }

        /// <summary>
        /// Loads the iPod from the specified drive.  If you already know the iPod's drive letter, this is more performant than calling GetConnectedIPod().
        /// </summary>
        /// <returns></returns>
        public static IPod GetiPodByDrive(string driveLetter, IPodLoadAction action)
        {
            DriveInfo drive = new DriveInfo(driveLetter);

            DeviceFileSystem deviceFS = GetDeviceFileSystemForDrive(drive);
            if (deviceFS != null)
            {
                IPod ipod = new IPod(deviceFS, action);
                return ipod;
            }
            else
            {
                throw new InvalidIPodDriveException(string.Format("The '{0}' drive is not an iPod.", driveLetter.ToUpper()));
            }
        }

        /// <summary>
        /// Will return a DeviceFileSystem from SharePodLib.RegisteredFileSystems or null if none match
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        internal static DeviceFileSystem GetDeviceFileSystemForDrive(DriveInfo drive)
        {
            foreach (DeviceFileSystem deviceFileSystem in SharePodLib.RegisteredFileSystems)
            {
                if (deviceFileSystem is StandardFileSystem)
                {
                    return ((StandardFileSystem)deviceFileSystem).GetDeviceByDrive(drive);
                }
            }
            return null;
        }

        #endregion

    }
}
