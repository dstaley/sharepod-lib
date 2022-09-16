using System;
using System.Collections.Generic;
using System.Text;

namespace SharePodLib.Exceptions
{
    public class FileLockException : BaseSharePodLibException
    {
        public FileLockException(string message)
            : base(message)
        {
            Category = "File could not be locked";
        }
    }
}
