/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SharePodLib.Parsers
{
    public enum CompatibilityType
    {
        Unknown,
        Compatible,
        NotWritable,
        SourceDoesntMatchOutput,
        UnsupportedNewDeviceOrFirmware
    }

    /// <summary>
    /// Static class containing some helpful functions.
    /// </summary>
    public static class Helpers
    {
		/// <summary>
		/// Returns a DateTime from an iPod-format timestamp
		/// </summary>
		/// <param name="timestamp"></param>
		/// <returns></returns>
        public static DateTime GetDateTimeFromTimeStamp(uint timestamp)
        {
            DateTime origin = new DateTime(1904, 1, 1);
            return origin.AddSeconds(timestamp);
        }

		/// <summary>
		/// Returns iPod-format timestamp from specified DateTime
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
        public static uint GetTimeStampFromDate(DateTime date)
        {
            DateTime origin = new DateTime(1904, 1, 1);
            TimeSpan ts = new TimeSpan(date.Ticks - origin.Ticks);
            return (uint)ts.TotalSeconds;
        }

		/// <summary>
		/// Replaces all ":" characters with "\"
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
        public static string iPodPathToStandardPath(string path){
            return path.Replace(":", "\\");
        }

		/// <summary>
		/// Replaces all "\" characters with ":"
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
        public static string StandardPathToiPodPath(string path)
        {
            return path.Replace("\\", ":").Replace("/", ":");
        }

        
        /// <summary>
        /// Returns iTunesSD int format (big-endian 3 byte)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] IntToITunesSDFormat(int value)
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(value >> 16);
            bytes[1] = (byte)(value >> 8);
            bytes[2] = (byte)(value);
            return bytes;
        }

        public static uint SwapUnsignedInt(uint source)
        {
            return (uint)(((source & 0x000000FF << 24)
               | ((source & 0x0000FF00) << 8)
               | ((source & 0x00FF0000) >> 8)
               | ((source & 0xFF000000) >> 24)));

        }

		/// <summary>
		/// Returns hh:mm:ss string from specified number of seconds
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
        public static string GetTimeString(long seconds)
        {
            int minutes = (int)(seconds / 60);
            int hours = minutes / 60;

            minutes = minutes - hours * 60;
            seconds = seconds - minutes * 60 - hours * 3600;

            return hours.ToString("00") + minutes.ToString(":00") + seconds.ToString(":00");
        }

		/// <summary>
		/// Returns string describing the specified filesize.  MB or GB will be displayed depending how large the number is
		/// </summary>
		/// <param name="fileSizeBytes"></param>
		/// <param name="decimalPoints"></param>
		/// <returns></returns>
        public static string GetFileSizeString(long fileSizeBytes, int decimalPoints)
        {
            double mbSize = (double)fileSizeBytes / 1048576;

            if (mbSize > 1024)
            {
                double gbSize = (double)fileSizeBytes / 1073741824;
                return Math.Round(gbSize, 1, MidpointRounding.ToEven).ToString() + "GB";
            }
            else
            {
                return Math.Round(mbSize, decimalPoints, MidpointRounding.AwayFromZero).ToString() + "MB";
            }
        }

		/// <summary>
		/// creates folders down to the given DirectoryInfo if they dont already exist
		/// </summary>
        public static void EnsureDirectoryExists(DirectoryInfo directory)
        {
            if (!directory.Exists)
                directory.Create();
        }

        
        /// <summary>
        /// Can only be called before making changes to iPod, otherwise looses point
        /// Makes sure we can write back the iTunesDB file exactly as it was from our object model before making any changes.
        /// In some cases this will fail - eg If there are some invalid Star Ratings in the database SharePodLib will automatically reset them to 0
        /// then the write-back will be different...
        /// </summary>
        /// <returns></returns>
        internal static CompatibilityType TestCompatibility(string originalDBFile, string generatedDBFile)
        {
            FileStream f1 = null, f2 = null;
            try
            {
                f1 = new FileStream(generatedDBFile, FileMode.Open);
                f2 = new FileStream(originalDBFile, FileMode.Open);
            }
            catch (Exception) { }

            // Compare files 
            int i, j;
            try
            {
                do
                {
                    i = f1.ReadByte();
                    j = f2.ReadByte();
                    if (i != j) break;
                } while (i != -1 && j != -1);
            }
            catch (IOException ex)
            {
                DebugLogger.LogException(ex);
                return CompatibilityType.SourceDoesntMatchOutput;
            }
            finally
            {
                f1.Close();
                f2.Close();
                File.Delete(generatedDBFile);
            }

            if (i != j)
                return CompatibilityType.SourceDoesntMatchOutput;
            else
                return CompatibilityType.Compatible;
        }

        internal static string GetFileTypeDescription(FileInfo file)
        {
            string extension = file.Extension.ToLower();
            if (extension == ".mp4")
                return "MPEG-4 video file";
            else if (extension == ".m4v")
                return "MPEG-4 video file";
            else if (extension == ".mp3")
                return "MPEG audio file";
            else if (extension == ".m4a")
                return "AAC audio file";
            else if (extension == ".wav")
                return "Wav audio file";
            else if (extension == ".m4r")
                return "Ringtone";
            else
                return String.Empty;
        }

        internal static string DrawArray(byte[] array)
        {
            if (array == null) return "";
            StringBuilder sb = new StringBuilder();
            foreach (byte b in array)
            {
                sb.Append(((int)b).ToString("000") + ",");
            }
            return sb.ToString();
        }
    }
}
