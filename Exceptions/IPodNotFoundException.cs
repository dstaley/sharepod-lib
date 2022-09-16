/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 


using System;
using System.Collections.Generic;
using System.Text;

namespace SharePodLib.Exceptions
{
	/// <summary>
	/// Thrown when an iPod couldnt be found during a call to SharePod.GetConnectedIPod()
	/// </summary>
    public class IPodNotFoundException : BaseSharePodLibException
    {
        public IPodNotFoundException(string message)
            : base(message)
        {
            Category = "iPod could not be found";
        }


    }
}
