///*
// *      SharePodLib - A library for interacting with an iPod.
// *      Jeffrey Harris 2006-2010
// *      Website: http://www.getsharepod.com/fordevelopers
// */ 


//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using System.Web;
//using SharePodLib.Parsers.iTunesDB;

//namespace SharePodLib.Export
//{
//    class ITunesFileWriter
//    {
//        StreamWriter _file;
//        bool _tracksAdded;
//        string _fileName;
//        Dictionary<string, long> _fileIds;
//        private long _fileId = DateTime.Now.Ticks;

//        public ITunesFileWriter(string folder)
//        {
//            _fileName = folder + "SharePod_iTunes_Import.xml";
//            _file = File.CreateText(_fileName);
//            _fileIds = new Dictionary<string, long>();
//            WriteFileHeader();
//        }

//        private void WriteFileHeader()
//        {
//            _file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
//            _file.WriteLine("<plist version=\"1.0\">");
//        }
//        private void WriteFileFooter()
//        {
//            _file.WriteLine("</plist>");
//        }

//        private void WriteTracksHeader()
//        {
//            _file.WriteLine("<dict>");
//            _file.WriteLine("<key>Show Content Ratings</key><true/>");
//            _file.WriteLine("<key>Tracks</key>");
//            _file.WriteLine("<dict>");
//        }

//        private void WriteTracksFooter()
//        {
//            _file.WriteLine("</dict>");
//        }

//        public void StartTracksSection()
//        {
//            WriteTracksHeader();
//        }

//        public void EndTracksSection()
//        {
//            WriteTracksFooter();
//        }

//        public void StartPlaylistsSection()
//        {
//            WritePlaylistsHeader();
//        }

//        public void EndPlaylistsSection()
//        {
//            WritePlaylistsFooter();
//        }

//        private void WritePlaylistsHeader()
//        {
//            _file.WriteLine("<key>Playlists</key>");
//            _file.WriteLine("<array>");
//        }

//        private void WritePlaylistsFooter()
//        {
//            _file.WriteLine("</array>");
//            _file.WriteLine("</dict>");
//        }

//        public long WriteFileEntry(string filename, Track iPodTrack)
//        {
//            if (_fileIds.ContainsKey(filename))
//            {
//                return _fileIds[filename];
//            }

//            string urlFileName = "file://localhost/" + UrlEncode(filename);


//            _file.WriteLine(string.Format("<key>{0}</key>", _fileId));
//            _file.WriteLine("<dict>");
//            _file.WriteLine(string.Format("<key>Track ID</key><integer>{0}</integer>", _fileId));
//            _file.WriteLine(string.Format("<key>Rating</key><integer>{0}</integer>", iPodTrack.Rating.ITunesRating));
//            _file.WriteLine(string.Format("<key>Location</key><string>{0}</string>", urlFileName));
//            _file.WriteLine(string.Format("<key>Play Count</key><integer>{0}</integer>", iPodTrack.PlayCount));
//            _file.WriteLine(string.Format("<key>Play Date</key><integer>{0}</integer>", iPodTrack.DateLastPlayed.TimeStamp));
//            _file.WriteLine("</dict>");

//            _tracksAdded = true;
//            _fileIds.Add(filename, _fileId);
//            _fileId++;
//            return _fileId-1;
//        }

//        public void WritePlaylistEntry(string name, List<long> trackIds)
//        {
//            if (trackIds == null)
//                return;

//            _file.WriteLine("<dict>");
//            _file.WriteLine(string.Format("<key>Name</key><string>{0}</string>", name));
//            _file.WriteLine("<key>All Items</key><true/>");
//            _file.WriteLine("<key>Playlist Items</key>");
//            _file.WriteLine("<array>");

//            foreach (long trackId in trackIds)
//            {
//                _file.WriteLine(string.Format("<dict><key>Track ID</key><integer>{0}</integer></dict>", trackId));
//            }

//            _file.WriteLine("</array>");
//            _file.WriteLine("</dict>");
//        }

//        public void Save()
//        {
//            WriteFileFooter();
//            _file.Close();

//            if (_tracksAdded == false)
//            {
//                File.Delete(_fileName);
//            }
//        }

//        private string UrlEncode(string str)
//        {
//            string urlString = "";
//            str = str.Replace("\\", "/");
//            urlString = HttpUtility.UrlEncode(str);
//            urlString = urlString.Replace("%2f", "/");
//            urlString = urlString.Replace("+", "%20");
//            urlString = urlString.Replace("%3a", ":");
//            return urlString;

            
//        }
//    }
//}

