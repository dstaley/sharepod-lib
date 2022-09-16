/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharePodLib;

namespace SharePodLib.DatabaseHash
{
	internal class DatabaseHasher
	{
        public static void Hash(FileStream file, IPod iPod)
        {
            if (iPod.DeviceInfo.FirewireId == null || iPod.DeviceInfo.FirewireId.Length != 16)
                return;

            byte[] hash = null;

            BinaryReader reader = new BinaryReader(file);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            byte[] contents = new byte[reader.BaseStream.Length];
            reader.Read(contents, 0, contents.Length);

            Zero(ref contents, 0x18, 8);
            Zero(ref contents, 0x32, 20);
            Zero(ref contents, 0x58, 20);

            hash = Hash58.GenerateDatabaseHash(iPod.DeviceInfo.FirewireId, contents);

            BinaryWriter writer = new BinaryWriter(file);
            writer.Seek(0x58, SeekOrigin.Begin);
            writer.Write(hash, 0, hash.Length);

            if (iPod.ITunesDB.HashingScheme >= 2)
            {
                Zero(ref contents, 0x72, 46);

                hash = Hash72.GenerateDatabaseHash(iPod.DeviceInfo.SerialNumberForHashing, contents);

                writer.Seek(0x72, SeekOrigin.Begin);
                writer.Write(hash, 0, 46);
            }

            writer.Close();
        }


		public static void Zero(ref byte[] buffer, int index, int length)
		{
			for (int i = index; i < index + length; i++)
			{
				buffer[i] = 0;
			}
		}
	}
}
