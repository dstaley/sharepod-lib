/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers;

namespace SharePodLib.Exceptions
{
	/// <summary>
	/// Thrown when the iPod database version is below 0x14. iTunes 7.1 and above create 0x14(+) databases.
	/// If the version is below 0x14, SharePodLib will try and read it, but will not enable modifications.
	/// </summary>
    public class UnsupportedITunesVersionException : BaseSharePodLibException
    {
        private CompatibilityType _compatibility;

        public UnsupportedITunesVersionException(string message, CompatibilityType compatibility)
            : base(message)
        {
            Category = "iPod database not supported";
            _compatibility = compatibility;
        }

        public CompatibilityType Compatibility
        {
            get { return _compatibility; }
        }
    }
}
