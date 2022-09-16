/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;

namespace SharePodLib
{
    /// <summary>
    /// Enumeration of the different iPod types
    /// </summary>
    public enum IPodFamily
    {
        Unknown = 0,
        iPod_Gen1_Gen2 = 1,
        iPod_Gen3 = 2,
        iPod_Mini = 3,
        iPod_Gen4 = 4,
        iPod_Gen4_2 = 5,
        iPod_Gen5 = 6,
        iPod_Nano_Gen1 = 7,
        iPod_Nano_Gen2 = 9,
        iPod_Classic = 11,
        iPod_Nano_Gen3 = 12,
        iPod_Nano_Gen4 = 15,
        iPod_Nano_Gen5 = 16,
        iPod_Shuffle_Gen1 = 128,
        iPod_Shuffle_Gen2 = 130,
        iPod_Shuffle_Gen3 = 132,
        IPhone_ITouch = 2000,
        IPhone_ITouch2 = 2001,
        IPhone_ITouch3 = 2002,
        IPhone_ITouch4 = 2003
    }    
}
