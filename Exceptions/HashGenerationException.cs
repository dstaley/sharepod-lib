using System;
using System.Collections.Generic;
using System.Text;

namespace SharePodLib.Exceptions
{
    /// <summary>
    /// Thrown when a hash has failed to generate for an iPod database file
    /// </summary>
    public class HashGenerationException : BaseSharePodLibException
    {
        public HashGenerationException(string message)
            : base(message)
        {
            Category = "iPod database hash generation failed";
        }
    }
}
