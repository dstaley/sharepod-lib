using System;
using System.Collections.Generic;
using System.Text;

namespace SharePodLib.Parsers.iTunesDB
{
    abstract class StringMHOD : BaseMHODElement
    {
        public abstract string Data { get; set; }
    }
}
