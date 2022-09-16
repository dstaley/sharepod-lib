using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers.iTunesDB;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using SharePodLib.DatabaseHash;

namespace SharePodLib.Parsers.iTunesCDB
{
    /// <summary>
    /// Implements the Sqlite tables for version 3.1 and above
    /// </summary>
    class SqliteTables_31 : SqliteTables
    {
        protected List<Entry> _albumArtists;
        protected long _nextAlbumArtistId;
        
        public SqliteTables_31(IPod iPod) : base(iPod)
        {            
        }

        public override void Initialize()
        {
            base.Initialize();
            SQLiteFunction.RegisterFunction(typeof(ML3Section));
            SQLiteFunction.RegisterFunction(typeof(ML3SortCollation));
            SQLiteFunction.RegisterFunction(typeof(ML3SearchCollation));
            
        }
        
        protected override void ReadLookupTables()
        {
            _baseLocations = new List<Entry>();
            _genres = new List<Entry>();
            _albums = new List<AlbumEntry>();
            _artists = new List<Entry>();
            _albumArtists = new List<Entry>();

            Trace.WriteLine("Getting baselocations");

            using (SQLiteCommand cmd = new SQLiteCommand("select id, path from base_location order by id", _locationsConnection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        if (reader.IsDBNull(1))
                            continue;

                        string path = reader.GetString(1);
                        _baseLocations.Add(new Entry { Id = id, Value = path });
                        _nextBaseLocationId = id + 1;
                    }
                }
            }

            Trace.WriteLine("Getting genres");

            using (SQLiteCommand cmd = new SQLiteCommand("select id, genre from genre_map order by id", _libraryConnection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        if (!reader.IsDBNull(1))
                            _genres.Add(new Entry { Id = id, Value = reader.GetString(1) });
                        _nextGenreId = id + 1;
                    }
                }
            }

            Trace.WriteLine("Getting item_artist");

            using (SQLiteCommand cmd = new SQLiteCommand("select pid, name from item_artist order by pid", _libraryConnection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        if (!reader.IsDBNull(1))
                            _artists.Add(new Entry { Id = id, Value = reader.GetString(1) });
                        _nextArtistId = id + 1;
                    }
                }
            }

            Trace.WriteLine("Getting album_artist");

            using (SQLiteCommand cmd = new SQLiteCommand("select pid, name from album_artist order by pid", _libraryConnection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        if (!reader.IsDBNull(1))
                            _albumArtists.Add(new Entry { Id = id, Value = reader.GetString(1) });
                        _nextAlbumArtistId = id + 1;
                    }
                }
            }

            Trace.WriteLine("Getting album");

            using (SQLiteCommand cmd = new SQLiteCommand("select pid, artist_pid, name from album order by pid", _libraryConnection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        if (!reader.IsDBNull(2))
                            _albums.Add(new AlbumEntry { Id = id, ArtistId = reader.GetInt64(1), Value = reader.GetString(2) });
                        _nextAlbumId = id + 1;
                    }
                }
            }

            Trace.WriteLine("Getting locationkinds");

            using (SQLiteCommand cmd = new SQLiteCommand("select id, kind from location_kind_map order by id", _libraryConnection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        if (!reader.IsDBNull(1))
                            _locationKinds.Add(reader.GetString(1), id);
                        _nextKindId = id + 1;
                    }
                }
            }
        }


        protected override long GetAlbumId(Track track, long artistId)
        {
            AlbumEntry entry = _albums.Find(a => a.Value.Equals(track.Album, StringComparison.InvariantCultureIgnoreCase) && a.ArtistId == artistId);

            if (entry == null)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into album (pid, kind, name, sort_name, artist_pid, artwork_status, artwork_item_pid) values(@pid, @kind, @name, @sort_name, @artist_pid, 0, 0)", _libraryConnection))
                {
                    cmd.Parameters.AddWithValue("@pid", _nextAlbumId);
                    cmd.Parameters.AddWithValue("@kind", 2); //music?
                    cmd.Parameters.AddWithValue("@name", track.Album);
                    cmd.Parameters.AddWithValue("@sort_name", GetSortString(track.Album));
                    cmd.Parameters.AddWithValue("@artist_pid", artistId);
                    cmd.ExecuteNonQuery();
                }
                entry = new AlbumEntry { Id = _nextAlbumId++, ArtistId = artistId, Value = track.Album };
                _albums.Add(entry);
            }
            return entry.Id;
        }

        protected override long GetArtistId(string artist)
        {
            Entry artistEntry = _artists.Find(a => a.Value.Equals(artist, StringComparison.InvariantCultureIgnoreCase));
            if (artistEntry == null)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into item_artist (pid, name, sort_name, artwork_album_pid) values(@pid, @name, @sort_name, 0)", _libraryConnection))
                {
                    cmd.Parameters.AddWithValue("@pid", _nextArtistId);
                    cmd.Parameters.AddWithValue("@name", artist);
                    cmd.Parameters.AddWithValue("@sort_name", GetSortString(artist));
                    cmd.ExecuteNonQuery();
                }
                artistEntry = new Entry { Value = artist, Id = _nextArtistId++ };
                _artists.Add(artistEntry);
            }
            return artistEntry.Id;
        }

        protected override long GetAlbumArtistId(string artist)
        {
            Entry artistEntry = _albumArtists.Find(a => a.Value.Equals(artist, StringComparison.InvariantCultureIgnoreCase));
            if (artistEntry == null)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into album_artist (pid, kind, name, sort_name, artwork_status, artwork_album_pid) values(@pid, @kind, @name, @sort_name, 0, 0)", _libraryConnection))
                {
                    cmd.Parameters.AddWithValue("@pid", _nextAlbumArtistId);
                    cmd.Parameters.AddWithValue("@kind", 2); //music?
                    cmd.Parameters.AddWithValue("@name", artist);
                    cmd.Parameters.AddWithValue("@sort_name", GetSortString(artist));
                    cmd.ExecuteNonQuery();
                }
                artistEntry = new Entry { Value = artist, Id = _nextAlbumArtistId++ };
                _albumArtists.Add(artistEntry);
            }
            return artistEntry.Id;
        }

        public override void Cleanup()
        {
            //delete entries from album table where there are no matching remaining
            string sql = "delete from album where pid not in " +
                        "(select album_pid from item )";

            SQLiteCommand cmd = new SQLiteCommand(sql, _libraryConnection);
            int rows = cmd.ExecuteNonQuery();
            cmd.Dispose();

            //delete entries from item_artist table where there are no matching remaining
            sql = "delete from item_artist where pid not in " +
                   "(select artist_pid from item )";
            cmd = new SQLiteCommand(sql, _libraryConnection);
            rows = cmd.ExecuteNonQuery();
            cmd.Dispose();

            //delete entries from album_artist table where there are no matching remaining
            sql = "delete from album_artist where pid not in " +
                   "(select artist_pid from album )";
            cmd = new SQLiteCommand(sql, _libraryConnection);
            rows = cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
    }
}
