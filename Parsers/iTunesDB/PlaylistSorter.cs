/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;

namespace SharePodLib.Parsers.iTunesDB
{
    class PlaylistSorter : IComparer<Playlist>
    {
        #region IComparer<Playlist> Members

        public int Compare(Playlist x, Playlist y)
        {
            if (x.IsMaster != y.IsMaster)
                return y.IsMaster.CompareTo(x.IsMaster);
            
            return x.Name.CompareTo(y.Name);
        }

        #endregion
    }
}
