/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers.iTunesDB;
using System.IO;
using SharePodLib.DataTypes;

namespace SharePodLib.Parsers.PlayCounts
{
    class PlayCounts
    {
        Header _header;
        MusicDatabase _iTunesDB;

        public PlayCounts(MusicDatabase iTunesDB)
        {
            _iTunesDB = iTunesDB;
            string playCountsPath = _iTunesDB.iPod.FileSystem.PlayCountsPath;                           
            if (!_iTunesDB.iPod.FileSystem.FileExists(playCountsPath))
            {
                return;
            }

            Stream fs = _iTunesDB.iPod.FileSystem.OpenFile(playCountsPath, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            if (br.BaseStream.Length < 16)
            {
                br.Close();
                return;
            }

            _header = new Header();
            _header.Read(_iTunesDB.iPod, br);
            br.Close();
        }

        public void MergeChanges()
        {
            //If we didnt read the file, cant merge any changes.
            if (_header == null)
                return;

            if (_header.EntryCount != _iTunesDB.TracksList.Count)
                return;

            int currentIndex = 0;
            
            foreach (Entry entry in _header.Entries())
            {
                Track track = _iTunesDB.TracksList[currentIndex];
                if (entry.PlayCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Updated playcount for " + track.Artist + " " + track.Title);
                    track.PlayCount += entry.PlayCount;
                    track.DateLastPlayed = new IPodDateTime(entry.DateLastPlayed);
                }
                if (track.Rating.StarRating != entry.Rating)
                {
                    track.Rating = new IPodRating(entry.Rating);
                }

                currentIndex++;
            }

            string playCountsPath = _iTunesDB.iPod.FileSystem.PlayCountsPath;
            _iTunesDB.iPod.FileSystem.DeleteFile(playCountsPath);
        }
    }
}
