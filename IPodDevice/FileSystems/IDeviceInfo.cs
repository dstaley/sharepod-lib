/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Parsers.Artwork;

namespace SharePodLib.IPodDevice.FileSystems
{
    /// <summary>
    /// Holds information about the iPod (Type, FirewireId, Serial #, supported artwork formats etc.)
    /// </summary>
    public interface IDeviceInfo
    {
        /// <summary>
        /// Exception which occured while retrieving device information
        /// </summary>
        Exception ReadException { get; }
        /// <summary>
        /// FirewireId of iPod. Used to generate iTunesDB database hash.
        /// </summary>
        string FirewireId { get; }
        /// <summary>
        /// Serial number of iPod.
        /// </summary>
        string SerialNumber { get; }

        /// <summary>
        /// Serial number of iPod used for hashing schemes.  May be the same or different to SerialNumber
        /// </summary>
        string SerialNumberForHashing { get; }
        /// <summary>
        /// Type of iPod.
        /// </summary>
        IPodFamily Family { get; }
        /// <summary>
        /// If Family is unknown, FamilyId can be used until SharePodLib is updated to include the new value in the IPodFamily enum.
        /// </summary>
        int FamilyId { get; }
        /// <summary>
        /// List of artwork formats supported by this iPod.
        /// </summary>
        List<SupportedArtworkFormat> SupportedArtworkFormats { get; }
        
        /// <summary>
        /// List of photo formats supported by this iPod.
        /// </summary>
        List<SupportedArtworkFormat> SupportedPhotoFormats { get; }

        /// <summary>
        /// Most disk-based iPod's can provide a device descriptor when queried.  This is the raw result. Useful if you need more information about
        /// the iPod than IDeviceInfo provides.
        /// </summary>
        string RawDeviceDescriptor { get; }

        string Version { get; }
    }
}
