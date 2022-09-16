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

namespace SharePodLib.Parsers.iTunesSD
{
    class ITunesSD
    {
        IPod _iPod;
        Header _header;
        public ITunesSD(IPod iPod)
        {
            _iPod = iPod;
            _header = new Header(iPod);
        }

        public void Backup()
        {
            string iTunesSDPath = _iPod.FileSystem.ITunesSDPath;
            if (_iPod.FileSystem.FileExists(iTunesSDPath))
            {
                File.Copy(iTunesSDPath, iTunesSDPath + ".spbackup", true);
            }
        }

        
        public void Generate()
        {
            string iTunesSDPath = _iPod.FileSystem.ITunesSDPath;
            FileStream fs = new FileStream(iTunesSDPath, FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(fs);
            
            _header.Write(writer);
            writer.Flush();
            writer.Close();
        }
    }
}
