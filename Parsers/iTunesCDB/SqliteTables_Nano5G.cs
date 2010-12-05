﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using SharePodLib.Parsers.iTunesDB;

namespace SharePodLib.Parsers.iTunesCDB
{
    class SqliteTables_Nano5G : SqliteTables
    {
        protected List<Entry> _trackArtists;
        long _nextTrackArtistId = 1;

        public SqliteTables_Nano5G(IPod iPod) : base(iPod)
        {
        }

        protected override void ReadLookupTables()
        {
            base.ReadLookupTables();

            _trackArtists = new List<Entry>();
            using (SQLiteCommand cmd = new SQLiteCommand("select pid, name from track_artist order by pid", _libraryConnection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        if (reader.IsDBNull(1))
                            continue;

                        string name = reader.GetString(1);
                        _trackArtists.Add(new Entry { Id = id, Value = name });
                        _nextTrackArtistId = id + 1;
                    }
                }
            }
        }

        public override SQLiteCommand GetUpdateTrackCommand()
        {
            SQLiteCommand updateCmd = new SQLiteCommand();
            updateCmd.CommandText = "update item set " +
                "year=@year, is_compilation=@is_compilation" +
                ",sort_title=@sort_title,title=@title" +
                ",track_number=@track_number, disc_number=@disc_number,genre_id=@genre_id" +
                ",sort_artist=@sort_artist,artist_pid=@album_artist_pid,artist=@artist" +
                ",sort_album=@sort_album,album_pid=@album_pid,album=@album" +
                ",album_artist=@album_artist,track_artist_pid=@artist_pid" +
                ",is_song=@is_song,is_audio_book=@is_audio_book,is_music_video=@is_music_video,is_movie=@is_movie,is_tv_show=@is_tv_show,is_ringtone=@is_ringtone" +
                ",date_modified=@date_modified,remember_bookmark=@remember_bookmark,artwork_status=@artwork_status,artwork_cache_id=@artwork_cache_id" +
                " where pid=@pid";

            return updateCmd;
        }

        public override SQLiteCommand GetInsertTrackCommand()
        {
            SQLiteCommand command = new SQLiteCommand();
            command.CommandText = "insert into item (pid, media_kind, is_song, is_audio_book, is_music_video, is_movie, is_tv_show, is_ringtone" +
                    ",is_voice_memo, is_rental, is_podcast, date_modified, date_backed_up, year, content_rating, content_rating_level, is_compilation" +
                     ",is_user_disabled, remember_bookmark, exclude_from_shuffle, artwork_status, artwork_cache_id, start_time_ms, total_time_ms, track_number" +
                     ",track_count, disc_number, disc_count, bpm, relative_volume, genius_id, genre_id, category_id, album_pid, artist_pid, composer_pid, title" +
                     ",artist, album, album_artist, track_artist_pid, composer, comment, description, description_long" +
                     ") " +

                    " VALUES (@pid, @media_kind, @is_song, @is_audio_book, @is_music_video, @is_movie, @is_tv_show, @is_ringtone" +
                    ",0, 0, 0, @date_modified, 0, @year, 0, 0, @is_compilation" +
                    ",0, @remember_bookmark, 0, @artwork_status, @artwork_cache_id, 0, @total_time_ms, @track_number" +
                    ",@track_count, @disc_number, @disc_count, 0, 0, 0, @genre_id, 0, @album_pid, @album_artist_pid, @composer_pid, @title" +
                    ",@artist, @album, @album_artist, @artist_pid, @composer, @comment, @description, @description_long" +
                    ");" +
                    "insert into avformat_info(item_pid) values (@pid)";
            return command;
        }

        protected override void CreatePlaylist(Playlist pl)
        {
            base.CreatePlaylist(pl);

            using (SQLiteCommand cmd = new SQLiteCommand("insert into container_ui (container_pid, play_order, is_reversed, album_field_order, repeat_mode, shuffle_items, has_been_shuffled) values (@pid, 1, 0, 1, 0, 0, 0)", _dynamicConnection))
            {
                cmd.Parameters.AddWithValue("@pid", pl.Id);
                cmd.ExecuteNonQuery();
            }
            _updatedDynamicDb = true;
        }

        protected override long GetArtistId(string artist)
        {
            Entry entry = _trackArtists.Find(a => a.Value.Equals(artist, StringComparison.InvariantCultureIgnoreCase));

            if (entry == null)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into track_artist (pid, name, sort_name, has_songs, has_non_compilation_tracks) values(@pid, @name, @sort_name, 1, 1)", _libraryConnection))
                {
                    cmd.Parameters.AddWithValue("@pid", _nextTrackArtistId);
                    cmd.Parameters.AddWithValue("@name", artist);
                    cmd.Parameters.AddWithValue("@sort_name", GetSortString(artist));
                    cmd.ExecuteNonQuery();
                }
                entry = new Entry { Id = _nextTrackArtistId++, Value = artist };
                _trackArtists.Add(entry);
            }
            return entry.Id;
        }

        protected override long GetAlbumArtistId(string artist)
        {
            //The Nano's Artist table contains the AlbumArtists. The Track_Artist table contains the Artists.
            return base.GetArtistId(artist);
        }

        protected override long GetGenreId(string genre)
        {
            Entry entry = _genres.Find(g => g.Value.Equals(genre, StringComparison.InvariantCultureIgnoreCase));
            if (entry == null)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into genre_map (id, genre, is_unknown, has_music) values(@id, @genre, 0, 1)", _libraryConnection))
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

        protected override long GetAlbumId(Track track, long artistId)
        {
            AlbumEntry entry = _albums.Find(a => a.Value.Equals(track.Album, StringComparison.InvariantCultureIgnoreCase) && a.ArtistId == artistId);

            if (entry == null)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("insert into album (pid, kind, name, sort_name, artist_pid, artwork_status, artwork_item_pid, has_songs) values(@pid, @kind, @name, @sort_name, @artist_pid, 0, @artwork_item_pid, 1)", _libraryConnection))
                {
                    cmd.Parameters.AddWithValue("@pid", _nextAlbumId);
                    cmd.Parameters.AddWithValue("@kind", 2); //music?
                    cmd.Parameters.AddWithValue("@name", track.Album);
                    cmd.Parameters.AddWithValue("@sort_name", GetSortString(track.Album));
                    cmd.Parameters.AddWithValue("@artist_pid", artistId);
                    cmd.Parameters.AddWithValue("@artwork_item_pid", track.DBId);
                    cmd.ExecuteNonQuery();
                }
                entry = new AlbumEntry { Id = _nextAlbumId++, ArtistId = artistId, Value = track.Album };
                _albums.Add(entry);
            }
            return entry.Id;
        }

        public override void Cleanup()
        {
            //delete entries from album table where there are no matching remaining
            string sql = "delete from album where pid not in " +
                        "(select album_pid from item) and is_unknown=0;" +
                        "update album set has_songs = min(1, (select count(*) from item where album_pid = album.pid))";

            SQLiteCommand cmd = new SQLiteCommand(sql, _libraryConnection);
            int rows = cmd.ExecuteNonQuery();
            cmd.Dispose();

            //delete entries from artist table where there are no matching remaining
            sql = "delete from artist where pid not in " +
                   "(select artist_pid from item) and is_unknown=0; " +
                   "update artist set has_songs = min(1, (select count(*) from item where artist_pid = artist.pid))";
            cmd = new SQLiteCommand(sql, _libraryConnection);
            rows= cmd.ExecuteNonQuery();
            cmd.Dispose();

            //delete entries from track_artist table where there are no matching remaining
            sql = "delete from track_artist where pid not in " +
                   "(select track_artist_pid from item) and is_unknown=0;" +
                   "update track_artist set has_songs = min(1, (select count(*) from item where track_artist_pid = track_artist.pid))";
            cmd = new SQLiteCommand(sql, _libraryConnection);
            rows = cmd.ExecuteNonQuery();
            cmd.Dispose();

            sql = "update genre_map set has_music = min(1, (select count(*) from item where genre_id = genre_map.id))";
            cmd = new SQLiteCommand(sql, _libraryConnection);
            rows = cmd.ExecuteNonQuery();
            cmd.Dispose();

            ReindexTable("artist", "pid", "UPPER(coalesce(sort_name, name))", "name_order", "where is_unknown=0");
            ReindexTable("track_artist", "pid", "UPPER(coalesce(sort_name, name))", "name_order", "where is_unknown=0");
            ReindexTable("album", "pid", "UPPER(coalesce(sort_name, name))", "sort_order", "");
            ReindexTable("item", "pid", "UPPER(coalesce(sort_title, title))", "title_order", "");
            ReindexTable("genre_map", "id", "UPPER(genre)", "genre_order", "where is_unknown=0");
        }

        /// <summary>
        /// Updates the whole-table sort column so items appear on the iPod in alphabetical order
        /// </summary>
        private void ReindexTable(string table, string idColumn, string orderby, string orderColumn, string where)
        {
            List<long> pids = new List<long>();
            SQLiteCommand readCmd = new SQLiteCommand("select " + idColumn + " from " + table + " " + where + " order by " + orderby, _libraryConnection);
            using (SQLiteDataReader reader = readCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    pids.Add(reader.GetInt64(0));
                }
            }
            readCmd.Dispose();

            long name_order = 100;
            SQLiteCommand updateCmd = new SQLiteCommand("update " + table + " set " + orderColumn + " = @order where " + idColumn + "=@pid", _libraryConnection);
            updateCmd.Parameters.Add(new SQLiteParameter("@pid"));
            updateCmd.Parameters.Add(new SQLiteParameter("@order"));
            foreach (long pid in pids)
            {
                updateCmd.Parameters["@pid"].Value = pid;
                updateCmd.Parameters["@order"].Value = name_order;
                updateCmd.ExecuteNonQuery();
                name_order += 100;
            }
            updateCmd.Dispose();
        }
    }
}
