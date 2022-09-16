using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using SharePodLib.Exceptions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SharePodLib.DatabaseHash
{
    /// <summary>
    /// Currently this shells out to a native c console app that computes and writes the correct hash for a given iTunesDB file
    /// 
    /// TODO: Convert that c app (lots of complex c/asm code) to c#...
    /// </summary>
    internal static class Hash72
    {
        [DllImport("hash72.dll")]
        static extern IntPtr CalcDBHash(byte[] serial, byte[] digest);
        [DllImport("hash72.dll")]
        static extern int UpdateITunesDBHash72(string filename, byte[] serial);


        public static byte[] GenerateDatabaseHash(string serialNbr, byte[] contents)
        {
            if (serialNbr == null || serialNbr.Length != 40)
            {
                throw new Exception("serialNbr must be 40 characters!");
            }

            SharePodLib.UnpackEmbeddedResource("hash72.dll", true);

            SHA1 sha1 = SHA1Managed.Create();
            byte[] sha1Digest = sha1.ComputeHash(contents);

            byte[] hash = CalcuateHash(sha1Digest, serialNbr);
            return hash;            
        }


        public static byte[] CalcuateHash(byte[] digest, string serialNbr)
        {
            SharePodLib.UnpackEmbeddedResource("hash72.dll", true);

            //call into hash72.dll
            IntPtr ret = CalcDBHash(StringToByteArray(serialNbr), digest);
            if (ret == IntPtr.Zero)
            {
                string msg = String.Format("Hash72 calculation failed.\r\n\r\nChanges have not been saved to your iPod.");
                throw new HashGenerationException(msg);
            }

            byte[] hash = new byte[46];
            Marshal.Copy(ret, hash, 0, 46);
            return hash;
        }
        
        private static byte[] StringToByteArray(string hex)
        {
            try
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
            catch (Exception ex)
            {
                return Encoding.ASCII.GetBytes(hex);
            }
        }
    }
}
