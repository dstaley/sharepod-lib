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
    /// Thrown when the iPod is not supported.  This can be achieved by setting iPod.IsWritable=false.
    /// 
    /// </summary>
    public class UnsupportedIPodException : BaseSharePodLibException
    {
        public UnsupportedIPodException(string message)
            : base(message)
        {
            Category = "iPod not fully supported";
        }
    }
}
