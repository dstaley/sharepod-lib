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
    public class BaseSharePodLibException : Exception
    {
        private string _category;
        public BaseSharePodLibException(string message) : base(message) 
        {
            _category = "SharePodLib Exception";            
        }

        public BaseSharePodLibException(string message, Exception innerException)
            : base(message, innerException)
        {
            _category = "SharePodLib Exception";
        }

        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }
    }
}
