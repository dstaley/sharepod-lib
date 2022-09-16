using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers.iTunesDB;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using SharePodLib.DatabaseHash;
using System.Threading;

namespace SharePodLib.Parsers.iTunesCDB
{
    class Entry
    {
        public long Id { get; set; }
        public string Value { get; set; }
    }

    class AlbumEntry : Entry
    {
        public long ArtistId { get; set; }
    }

    class SqliteTables
    {
        protected IPod _iPod;
        protected List<Entry> _genres;
        protected List<AlbumEntry> _albums;
        protected List<Entry> _artists;
        protected List<Entry> _baseLocations;

        protected Dictionary<string, int> _locationKinds = new Dictionary<string, int>();
        
        protected int _nextGenreId=1, _nextKindId=1, _nextBaseLocationId = 1;
        protected long _nextAlbumId=1, _nextArtistId=1;
        protected bool _updatedLocationsDb, _updatedDynamicDb;
        protected SQLiteConnection _libraryConnection, _locationsConnection, _dynamicConnection;
        protected SQLiteCommand _insertItemCommand, _insertLocationCommand, _insertVideoInfoCommand;

        protected string _localLibraryFile, _localLocationsFile, _localDynamicFile;
        
        public SqliteTables(IPod iPod)
        {
            _iPod = iPod;

            Trace.WriteLine("Downloading and connecting to sqlite databases...");

            Initialize();

            _localLibraryFile = Path.Combine(iPod.Session.TempFilesPath, "library.itdb");
            _localLocationsFile = Path.Combine(iPod.Session.TempFilesPath, "locations.itdb");
            _localDynamicFile = Path.Combine(iPod.Session.TempFilesPath, "dynamic.itdb");
                       
            
            if (!File.Exists(_localLibraryFile))
            {
                _iPod.FileSystem.CopyFileFromDevice(_iPod.FileSystem.ITunesFolderPath + "\\iTunes Library.itlp\\Library.itdb", _localLibraryFile);
                _iPod.FileSystem.CopyFileFromDevice(_iPod.FileSystem.ITunesFolderPath + "\\iTunes Library.itlp\\Locations.itdb", _localLocationsFile);
                _iPod.FileSystem.CopyFileFromDevice(_iPod.FileSystem.ITunesFolderPath + "\\iTunes Library.itlp\\Dynamic.itdb", _localDynamicFile);
            }

            

            _libraryConnection = new SQLiteConnection("Data Source=" + _localLibraryFile + ";Version=3;");
            _libraryConnection.Open();

            _locationsConnection = new SQLiteConnection("Data Source=" + _localLocationsFile + ";Version=3;");
            _locationsConnection.Open();

            _dynamicConnection = new SQLiteConnection("Data Source=" + _localDynamicFile + ";Version=3;");
            _dynamicConnection.Open();

            Trace.WriteLine("Done downloading and connecting to sqlite databases");
        }

        internal static bool HaveLocalSqlliteDbs(IPod ipod)
        {
            return File.Exists(Path.Combine(ipod.Session.TempFilesPath, "library.itdb"));
        }

        public virtual void Initialize()
        {
            SharePodLib.UnpackEmbeddedResource("sqlite3.dll", false);

            SQLiteFunction.RegisterFunction(typeof(IcuDataForString));
            SQLiteFunction.RegisterFunction(typeof(IcuSectionDataForString));
        }

        public void Save()
        {
            Trace.WriteLine("Uploading sqlite databases...");
            _libraryConnection.Close();
            _locationsConnection.Close();
            _dynamicConnection.Close();
            _libraryConnection.Dispose();
            _locationsConnection.Dispose();
            _dynamicConnection.Dispose();

            Thread.Sleep(200);

            _iPod.FileSystem.CopyFileToDevice(_localLibraryFile, _iPod.FileSystem.ITunesFolderPath + "\\iTunes Library.itlp\\Library.itdb");
            Trace.WriteLine("Uploaded Library sqlite database");
            if (_updatedLocationsDb)
            {
                _iPod.FileSystem.CopyFileToDevice(_localLocationsFile, _iPod.FileSystem.ITunesFolderPath + "\\iTunes Library.itlp\\Locations.itdb");
                Trace.WriteLine("Uploaded Locations sqlite database");
            }
            if (_updatedDynamicDb)
            {
                _iPod.FileSystem.CopyFileToDevice(_localDynamicFile, _iPod.FileSystem.ITunesFolderPath + "\\iTunes Library.itlp\\Dynamic.itdb");
                Trace.WriteLine("Uploaded Dynamic sqlite database");
            }
            Trace.WriteLine("Done uploading sqlite databases");
        }

        
        protected virtual void ReadLookupTables()
        {
            _baseLocations = new List<Entry>();
            _genres = new List<Entry>();
            _albums = new List<AlbumEntry>();
            _artists = new List<Entry>();

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

            using (SQLiteCommand cmd = new SQLiteCommand("select pid, name from artist order by pid", _libraryConnection))
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

        public virtual SQLiteCommand GetUpdateTrackCommand()
        {
            SQLiteCommand updateCmd = new SQLiteCommand();
            updateCmd.CommandText = "update item set " +
                "year=@year, is_compilation=@is_compilation" +
                ",sort_title=@sort_title,title=@title" +
                ",track_number=@track_number, disc_number=@disc_number,genre_id=@genre_id" +
                ",sort_artist=@sort_artist,artist_pid=@artist_pid,artist=@artist" +
                ",sort_album=@sort_album,album_pid=@album_pid,album=@album" +
                ",album_artist=@album_artist" +
                ",is_song=@is_song,is_audio_book=@is_audio_book,is_music_video=@is_music_video,is_movie=@is_movie,is_tv_show=@is_tv_show,is_ringtone=@is_ringtone" +
                ",date_modified=@date_modified,remember_bookmark=@remember_bookmark,artwork_status=@artwork_status,artwork_cache_id=@artwork_cache_id" +
                " where pid=@pid";

            return updateCmd;
        }

        public virtual SQLiteCommand GetInsertTrackCommand()
        {
            SQLiteCommand command = new SQLiteCommand();
            command.CommandText = "insert into item (pid, media_kind, is_song, is_audio_book, is_music_video, is_movie, is_tv_show, is_ringtone" +
                    ",is_voice_memo, is_rental, is_podcast, date_modified, date_backed_up, year, content_rating, content_rating_level, is_compilation" +
                     ",is_user_disabled, remember_bookmark, exclude_from_shuffle, artwork_status, artwork_cache_id, start_time_ms, total_time_ms, track_number" +
                     ",track_count, disc_number, disc_count, bpm, relative_volume, genius_id, genre_id, category_id, album_pid, artist_pid, composer_pid, title" +
                     ",artist, album, album_artist, composer, comment, description, description_long, in_songs_collection" +
                     ",title_blank,artist_blank,album_artist_blank,album_blank,composer_blank, grouping_blank) " +

                    " VALUES (@pid, @media_kind, @is_song, @is_audio_book, @is_music_video, @is_movie, @is_tv_show, @is_ringtone" +
                    ",0, 0, 0, @date_modified, 0, @year, 0, 0, @is_compilation" +
                    ",0, @remember_bookmark, 0, @artwork_status, @artwork_cache_id, 0, @total_time_ms, @track_number" +
                    ",@track_count, @disc_number, @disc_count, 0, 0, 0, @genre_id, 0, @album_pid, @artist_pid, @composer_pid, @title" +
                    ",@artist, @album, @album_artist, @composer, @comment, @description, @description_long, 1" +
                    ",0,0,0,0,0,1);" +
                    "insert into avformat_info(item_pid) values (@pid)";
            return command;
        }


        public virtual List<SQLiteParameter> GetTrackParameters()
        {
            List<SQLiteParameter> parameters = new List<SQLiteParameter>();
            parameters.Add(new SQLiteParameter("@pid"));
            parameters.Add(new SQLiteParameter("@is_song"));
            parameters.Add(new SQLiteParameter("@is_audio_book"));
            parameters.Add(new SQLiteParameter("@is_music_video"));
            parameters.Add(new SQLiteParameter("@is_movie"));
            parameters.Add(new SQLiteParameter("@is_tv_show"));
            parameters.Add(new SQLiteParameter("@is_ringtone"));
            parameters.Add(new SQLiteParameter("@date_modified"));
            parameters.Add(new SQLiteParameter("@remember_bookmark"));
            parameters.Add(new SQLiteParameter("@artwork_status"));
            parameters.Add(new SQLiteParameter("@artwork_cache_id"));

            parameters.Add(new SQLiteParameter("@year"));
            parameters.Add(new SQLiteParameter("@is_compilation"));
            parameters.Add(new SQLiteParameter("@track_number"));
            parameters.Add(new SQLiteParameter("@disc_number"));
            parameters.Add(new SQLiteParameter("@genre_id"));
            parameters.Add(new SQLiteParameter("@album_pid"));
            parameters.Add(new SQLiteParameter("@artist_pid"));
            parameters.Add(new SQLiteParameter("@title"));
            parameters.Add(new SQLiteParameter("@sort_title"));
            parameters.Add(new SQLiteParameter("@artist"));
            parameters.Add(new SQLiteParameter("@sort_artist"));
            parameters.Add(new SQLiteParameter("@album"));
            parameters.Add(new SQLiteParameter("@sort_album"));
            parameters.Add(new SQLiteParameter("@album_artist"));
            parameters.Add(new SQLiteParameter("@album_artist_pid"));

            parameters.Add(new SQLiteParameter("@media_kind"));
            parameters.Add(new SQLiteParameter("@total_time_ms"));
            parameters.Add(new SQLiteParameter("@track_count"));
            parameters.Add(new SQLiteParameter("@disc_count"));
            parameters.Add(new SQLiteParameter("@composer_pid"));
            parameters.Add(new SQLiteParameter("@composer"));
            parameters.Add(new SQLiteParameter("@comment"));
            parameters.Add(new SQLiteParameter("@description"));
            parameters.Add(new SQLiteParameter("@description_long"));
            return parameters;
        }

        public virtual void FillTrackParameters(SQLiteCommand cmd, Track track)
        {
            cmd.Parameters["@pid"].Value = track.DBId;
            cmd.Parameters["@is_song"].Value = track.MediaType == MediaType.Audio ? 1 : 0;
            cmd.Parameters["@is_audio_book"].Value = track.MediaType == MediaType.Audiobook ? 1 : 0;
            cmd.Parameters["@is_music_video"].Value = track.MediaType == MediaType.MusicVideo ? 1 : 0;
            cmd.Parameters["@is_movie"].Value = track.MediaType == MediaType.Video ? 1 : 0;
            cmd.Parameters["@is_tv_show"].Value = track.MediaType == MediaType.TVShow ? 1 : 0;
            cmd.Parameters["@is_ringtone"].Value = track.MediaType == MediaType.Ringtone ? 1 : 0;
            cmd.Parameters["@date_modified"].Value = 0; // t.DateLastModified.TimeStamp;
            cmd.Parameters["@remember_bookmark"].Value = track.RememberPlaybackPosition ? 1 : 0;
            cmd.Parameters["@artwork_status"].Value = track.Artwork.Count > 0 ? 2 : 0;
            cmd.Parameters["@artwork_cache_id"].Value = track.ArtworkIdLink;
            cmd.Parameters["@year"].Value = track.Year;
            cmd.Parameters["@is_compilation"].Value = track.IsCompilation ? 1 : 0;
            cmd.Parameters["@track_number"].Value = track.TrackNumber;
            cmd.Parameters["@disc_number"].Value = track.DiscNumber;
            cmd.Parameters["@genre_id"].Value = GetGenreId(track.Genre);

            long artistId = GetArtistId(track.Artist);
            long albumArtistId = GetAlbumArtistId(track.AlbumArtist);
            cmd.Parameters["@artist_pid"].Value = artistId;
            cmd.Parameters["@album_artist_pid"].Value = albumArtistId;
            cmd.Parameters["@album_pid"].Value = GetAlbumId(track, albumArtistId);
            cmd.Parameters["@title"].Value = track.Title;
            cmd.Parameters["@sort_title"].Value = track.SortTitle;
            cmd.Parameters["@sort_artist"].Value = track.SortArtist;
            cmd.Parameters["@sort_album"].Value = track.SortAlbum;
            cmd.Parameters["@artist"].Value = track.Artist;
            cmd.Parameters["@album"].Value = track.Album;
            cmd.Parameters["@album_artist"].Value = track.AlbumArtist;

            cmd.Parameters["@media_kind"].Value = track.MediaType;
            cmd.Parameters["@total_time_ms"].Value = (double)track.Length.MilliSeconds;
            cmd.Parameters["@track_count"].Value = 0;
            cmd.Parameters["@disc_count"].Value = track.TotalDiscCount;
            cmd.Parameters["@composer_pid"].Value = 0;
            cmd.Parameters["@composer"].Value = null;
            cmd.Parameters["@comment"].Value = track.Comment;
            cmd.Parameters["@description"].Value = track.DescriptionText;
        }

        public void UpdateTracks(List<Track> tracks)
        {
            Trace.WriteLine("Updating tracks in sqlite databases...");

            ReadLookupTables();

            SQLiteCommand updateCommand = GetUpdateTrackCommand();
            updateCommand.Parameters.AddRange(GetTrackParameters().ToArray());
            updateCommand.Connection = _libraryConnection;

            using (SQLiteTransaction transaction = _libraryConnection.BeginTransaction())
            {
                SQLiteCommand existsCmd = new SQLiteCommand("select count(*) from item where pid = @pid", _libraryConnection);
                existsCmd.Parameters.Add(new SQLiteParameter("@pid"));

                foreach (Track t in tracks)
                {
                    existsCmd.Parameters["@pid"].Value = t.DBId;

                    bool itemExists = ((long)existsCmd.ExecuteScalar()) > 0;

                    if (!itemExists)
                    {
                        InsertNewTrack(t);
                    }
                    else
                    {
                        FillTrackParameters(updateCommand, t);
                        int rows = updateCommand.ExecuteNonQuery();
                    }
                }

                existsCmd.Dispose();

                RemoveDeletedTracks();

                Cleanup();
                transaction.Commit();
            }

            updateCommand.Dispose();
            if (_insertItemCommand != null)
            {
                _insertItemCommand.Dispose();
                _insertItemCommand = null;
                _insertLocationCommand.Dispose();
                _insertLocationCommand = null;
                _insertVideoInfoCommand.Dispose();
                _insertVideoInfoCommand = null;
            }

            Trace.WriteLine("Done updating tracks in sqlite databases");
        }


        
        /// <summary>
        /// Insert a new track into the Sqlite database.  Library.itdb and Locations.itdb are both updated.
        /// </summary>
        private void InsertNewTrack(Track track)
        {
            if (_insertItemCommand == null)
            {
                _insertItemCommand = GetInsertTrackCommand();
                _insertItemCommand.Connection = _libraryConnection;
                _insertItemCommand.Parameters.AddRange(GetTrackParameters().ToArray());
                
                _insertLocationCommand = new SQLiteCommand(_locationsConnection);
                _insertLocationCommand.CommandText = "insert into location (item_pid, sub_id, base_location_id, location_type, location, extension, kind_id, file_size)" +
                    "values (@item_pid, 0, @base_location_id, @location_type, @location, @extension, @kind_id, @file_size)";
                _insertLocationCommand.Parameters.Add(new SQLiteParameter("@item_pid"));
                _insertLocationCommand.Parameters.Add(new SQLiteParameter("@base_location_id"));
                _insertLocationCommand.Parameters.Add(new SQLiteParameter("@location_type"));
                _insertLocationCommand.Parameters.Add(new SQLiteParameter("@location"));
                _insertLocationCommand.Parameters.Add(new SQLiteParameter("@extension"));
                _insertLocationCommand.Parameters.Add(new SQLiteParameter("@kind_id"));
                _insertLocationCommand.Parameters.Add(new SQLiteParameter("@file_size"));

                _insertVideoInfoCommand = new SQLiteCommand("insert into video_info (item_pid, has_alternate_audio, has_subtitles, characteristics_valid" +
                    ",has_closed_captions, is_self_contained, is_compressed, is_anamorphic, season_number, audio_language, audio_track_index, audio_track_id" +
                    ",subtitle_language, subtitle_track_index, subtitle_track_id, episode_sort_id) values (@item_pid, 0,0,0,0,1,0,0,0,0,0,0,0,0,0,0)", _libraryConnection);
                _insertVideoInfoCommand.Parameters.Add(new SQLiteParameter("@item_pid"));
            }

            FillTrackParameters(_insertItemCommand, track);
            int rows = _insertItemCommand.ExecuteNonQuery();

            _insertLocationCommand.Parameters["@item_pid"].Value = track.DBId;
            _insertLocationCommand.Parameters["@base_location_id"].Value = GetBaseLocation(track.FilePath).Id;
            _insertLocationCommand.Parameters["@location_type"].Value = 1179208773;

            Entry location = GetBaseLocation(track.FilePath);
            _insertLocationCommand.Parameters["@location"].Value = track.FilePath.Substring(location.Value.Length + 1).Replace("\\", "/");
            
            _insertLocationCommand.Parameters["@extension"].Value = GetExtensionId(track.FilePath);
            _insertLocationCommand.Parameters["@kind_id"].Value = GetKindId(track.FileType);
            _insertLocationCommand.Parameters["@file_size"].Value = track.FileSize.ByteCount;
            rows = _insertLocationCommand.ExecuteNonQuery();

            if (track.IsVideo)
            {
                _insertVideoInfoCommand.Parameters["@item_pid"].Value = track.DBId;
                _insertVideoInfoCommand.ExecuteNonQuery();
            }
            _updatedLocationsDb = true;
        }

        /// <summary>
        /// Remove all tracks which have been deleted this session.
        /// </summary>
        private void RemoveDeletedTracks()
        {
            Trace.WriteLine("Removing tracks from sqlite databases...");
            SQLiteCommand deleteLocationCmd = new SQLiteCommand("delete from location where item_pid=@pid", _locationsConnection);
            SQLiteCommand deleteItemCmd = new SQLiteCommand("delete from item where pid=@pid;delete from item_to_container where item_pid=@pid;" +
                "delete from avformat_info where item_pid=@pid;delete from video_characteristics where item_pid=@pid;/*delete from video_info where item_pid=@pid*/", _libraryConnection);
            deleteLocationCmd.Parameters.Add(new SQLiteParameter("@pid"));
            deleteItemCmd.Parameters.Add(new SQLiteParameter("@pid"));

            foreach (Track track in _iPod.Session.DeletedTracks)
            {
                deleteItemCmd.Parameters["@pid"].Value = track.DBId;
                deleteLocationCmd.Parameters["@pid"].Value = track.DBId;
                deleteItemCmd.ExecuteNonQuery();
                deleteLocationCmd.ExecuteNonQuery();
                Trace.WriteLine("Deleted " + track.Title + " from sqlite database");
                _updatedLocationsDb = true;
            }
            deleteItemCmd.Dispose();
            deleteLocationCmd.Dispose();
            Trace.WriteLine("Done removing tracks from sqlite databases");
        }


        public virtual void Cleanup()
        {
            //delete entries from album table where there are no matching remaining
            string sql = "delete from album where pid not in " +
                        "(select album_pid from item)";

            SQLiteCommand cmd = new SQLiteCommand(sql, _libraryConnection);
            int rows = cmd.ExecuteNonQuery();
            cmd.Dispose();

            //delete entries from artist table where there are no matching remaining
            sql = "delete from artist where pid not in " +
                   "(select artist_pid from item)";
            cmd = new SQLiteCommand(sql, _libraryConnection);
            rows = cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        private int GetExtensionId(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            switch (ext)
            {
                case ".mp3":
                    return 1297101600;
                case ".mp4":
                    return 1297101856;
                case ".m4a":
                    return 1295270176;
                case ".m4r":
                    return 1295274528;
                case ".m4v":
                    return 1295275552;
                case ".wav":
                    return 1463899680;
                case ".aif":
                    return 1095321120;
                default:
                    return 1297101600; //default to mp3?  todo?
                //throw new NotImplementedException();
            }
        }

        protected virtual Entry GetBaseLocation(string path)
        {
            path = path.Replace("\\", "/");
            Entry entry = _baseLocations.Find(l => path.StartsWith(l.Value, StringComparison.InvariantCultureIgnoreCase));
            if (entry == null)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into base_location (id, path) values(@id, @path)", _locationsConnection))
                {
                    cmd.Parameters.AddWithValue("@id", _nextBaseLocationId);
                    if (path.ToLower().Contains("itunes_control/ringtones"))
                        cmd.Parameters.AddWithValue("@path", "iTunes_Control/Ringtones");
                    else
                        cmd.Parameters.AddWithValue("@path", "iTunes_Control/Music");
                    cmd.ExecuteNonQuery();
                }
                entry = new Entry { Id = _nextBaseLocationId++, Value = path };
                _baseLocations.Add(entry);
            }
            return entry;
        }

        protected virtual long GetGenreId(string genre)
        {
            Entry entry = _genres.Find(g => g.Value.Equals(genre, StringComparison.InvariantCultureIgnoreCase));
            if (entry == null)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into genre_map (id, genre) values(@id, @genre)", _libraryConnection))
                {
                    cmd.Parameters.AddWithValue("@id", _nextGenreId);
                    cmd.Parameters.AddWithValue("@genre", genre);
                    cmd.ExecuteNonQuery();
                }
                entry = new Entry { Id = _nextGenreId++, Value = genre };
                _genres.Add(entry);
            }
            return entry.Id;
        }

        protected virtual long GetAlbumId(Track track, long artistId)
        {
            AlbumEntry entry = _albums.Find(a => a.Value.Equals(track.Album, StringComparison.InvariantCultureIgnoreCase) && a.ArtistId == artistId);

            if (entry == null)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into album (pid, kind, name, artist_pid, artwork_status, artwork_item_pid) values(@pid, @kind, @name, @artist_pid, 0, 0)", _libraryConnection))
                {
                    cmd.Parameters.AddWithValue("@pid", _nextAlbumId);
                    cmd.Parameters.AddWithValue("@kind", 2); //music?
                    cmd.Parameters.AddWithValue("@name", track.Album);
                    cmd.Parameters.AddWithValue("@artist_pid", artistId);
                    cmd.ExecuteNonQuery();
                }
                entry = new AlbumEntry { Id = _nextAlbumId++, ArtistId = artistId, Value = track.Album };
                _albums.Add(entry);
            }
            return entry.Id;
        }

        protected virtual long GetArtistId(string artist)
        {
            Entry artistEntry = _artists.Find(a => a.Value.Equals(artist, StringComparison.InvariantCultureIgnoreCase));
            if (artistEntry == null)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into artist (pid, kind, name) values(@pid, @kind, @name)", _libraryConnection))
                {
                    cmd.Parameters.AddWithValue("@pid", _nextArtistId);
                    cmd.Parameters.AddWithValue("@kind", 2); //music?
                    cmd.Parameters.AddWithValue("@name", artist);
                    cmd.ExecuteNonQuery();
                }
                artistEntry = new Entry { Value = artist, Id = _nextArtistId++ };
                _artists.Add(artistEntry);
            }
            return artistEntry.Id;
        }

        protected virtual long GetAlbumArtistId(string artist)
        {
            return GetArtistId(artist);
        }

        private int GetKindId(string kind)
        {
            if (!_locationKinds.ContainsKey(kind))
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into location_kind_map (id, kind) values(@id, @kind)", _libraryConnection))
                {
                    cmd.Parameters.AddWithValue("@id", _nextKindId);
                    cmd.Parameters.AddWithValue("@kind", kind); //music?
                    cmd.ExecuteNonQuery();
                }
                _locationKinds.Add(kind, _nextKindId);
                _nextKindId++;
            }
            return _locationKinds[kind];
        }

        /// <summary>
        /// Syncs up the Sqlite playlists with the iTunesCDB playlists.
        /// </summary>
        /// <param name="playlists"></param>
        public void UpdatePlaylists(List<Playlist> playlists)
        {
            Trace.WriteLine("Updating playlists in sqlite databases...");
            SQLiteCommand select = new SQLiteCommand("select item_pid from item_to_container where container_pid=@pid", _libraryConnection);
            SQLiteCommand insert = new SQLiteCommand("insert into item_to_container (item_pid, container_pid) values (@item_pid, @container_pid)", _libraryConnection);
            insert.Parameters.Add(new SQLiteParameter("@item_pid"));
            insert.Parameters.Add(new SQLiteParameter("@container_pid"));
            SQLiteCommand delete = new SQLiteCommand("delete from item_to_container where item_pid = @item_pid and container_pid = @container_pid", _libraryConnection);
            delete.Parameters.Add(new SQLiteParameter("@item_pid"));
            delete.Parameters.Add(new SQLiteParameter("@container_pid"));

            SQLiteTransaction transaction = _libraryConnection.BeginTransaction();

            foreach (Playlist pl in playlists)
            {
                long count = 0;
                // Handle creation/update of playlist
                using (SQLiteCommand cmd = new SQLiteCommand("select count(*) from container where pid = @pid", _libraryConnection))
                {
                    cmd.Parameters.AddWithValue("@pid", (Int64)pl.Id);
                    count = (long)cmd.ExecuteScalar();
                }
                if (count == 0)
                {
                    CreatePlaylist(pl);
                }
                else
                {
                    using (SQLiteCommand cmd = new SQLiteCommand("update container set name=@name where pid=@pid", _libraryConnection))
                    {
                        cmd.Parameters.AddWithValue("@pid", (Int64)pl.Id);
                        cmd.Parameters.AddWithValue("@name", pl.Name);
                        cmd.ExecuteNonQuery();
                    }
                }

                List<long> sqlItemIds = new List<long>();

                select.Parameters.AddWithValue("@pid", (Int64)pl.Id);
                using (SQLiteDataReader reader = select.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sqlItemIds.Add(reader.GetInt64(0));
                    }
                }

                // Update tracks just added to this playlist
                List<long> updatedIds = new List<long>();
                foreach (Track t in pl.Tracks)
                {
                    if (!sqlItemIds.Contains(t.DBId))
                    {
                        updatedIds.Add(t.DBId);
                    }
                }

                if (updatedIds.Count > 0)
                {
                    insert.Parameters["@container_pid"].Value = (Int64)pl.Id;
                    foreach (long id in updatedIds)
                    {
                        insert.Parameters["@item_pid"].Value = id;
                        insert.ExecuteNonQuery();
                    }
                }

                // Remove tracks just removed from this playlist
                updatedIds.Clear();
                foreach (long id in sqlItemIds)
                {
                    if (!pl.ContainsTrack(id))
                        updatedIds.Add(id);
                }

                if (updatedIds.Count > 0)
                {
                    delete.Parameters["@container_pid"].Value = (Int64)pl.Id;
                    foreach (long id in updatedIds)
                    {
                        delete.Parameters["@item_pid"].Value = id;
                        delete.ExecuteNonQuery();
                    }
                }
            }

            select.Dispose();
            insert.Dispose();
            delete.Dispose();

            //Delete playlists weve deleted from iTunesCDB
            delete = new SQLiteCommand("delete from container where pid=@container_pid; delete from item_to_container where container_pid=@container_pid", _libraryConnection);
            delete.Parameters.Add(new SQLiteParameter("@container_pid"));

            foreach (Playlist playlist in _iPod.Session.DeletedPlaylists)
            {
                delete.Parameters["@container_pid"].Value = (Int64)playlist.Id;
                delete.ExecuteNonQuery();
            }
            delete.Dispose();
            transaction.Commit();
            transaction.Dispose();

            Trace.WriteLine("Done updating playlists in sqlite databases");
        }

        protected virtual void CreatePlaylist(Playlist pl)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("insert into container (pid, distinguished_kind, name, parent_pid, media_kinds, workout_template_id, is_hidden, smart_is_folder)" +
                        "values (@pid, 0, @name, 0, @media_kinds, 0, 0, 0)", _libraryConnection))
            {
                cmd.Parameters.AddWithValue("@pid", (Int64)pl.Id);
                cmd.Parameters.AddWithValue("@name", pl.Name);
                cmd.Parameters.AddWithValue("@media_kinds", 1);
                cmd.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// This is where the magic happens.  Without this, the iPod will refuse to accept any changes.
        /// </summary>
        public void UpdateLocationsCbk()
        {
            if (!_updatedLocationsDb)
                return;

            Trace.WriteLine("Updating locations cbk file...");

            SHA1 sha1 = SHA1.Create();

            BinaryReader reader = new BinaryReader(File.Open(_localLocationsFile, FileMode.Open));

            byte[] sha1Buffer = new byte[((reader.BaseStream.Length / 1024)) * 20];
            int ptr = 0;
            int ptr2 = 0;
            while (true)
            {
                byte[] buffer = reader.ReadBytes(1024);
                byte[] hashed = sha1.ComputeHash(buffer);
                Array.Copy(hashed, 0, sha1Buffer, ptr2, 20);
                ptr += 1024;
                ptr2 += 20;
                if (reader.BaseStream.Position + 1024 > reader.BaseStream.Length)
                    break;
            }
            byte[] finalSha1Hash = sha1.ComputeHash(sha1Buffer);
            reader.Close();

            byte[] calculatedHash = Hash72.CalcuateHash(finalSha1Hash, _iPod.DeviceInfo.SerialNumberForHashing);

            string tempCbk = Path.GetTempFileName();
            BinaryWriter writer = new BinaryWriter(File.Open(tempCbk, FileMode.Create));
            writer.Write(calculatedHash);
            writer.Write(finalSha1Hash);
            writer.Write(sha1Buffer);
            writer.Close();
            _iPod.FileSystem.CopyFileToDevice(tempCbk, _iPod.FileSystem.ITunesFolderPath + "iTunes Library.itlp\\Locations.itdb.cbk");
            File.Delete(tempCbk);

            Trace.WriteLine("Done updating locations cbk file");
        }

        protected string GetSortString(string item)
        {
            if (item.StartsWith("The "))
                return item.Substring(4);
            else if (item.StartsWith("A "))
                return item.Substring(2);
            return item;
        }
    }
}