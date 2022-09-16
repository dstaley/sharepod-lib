/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharePodLib.Exceptions;
using SharePodLib.Parsers.iTunesDB;

namespace SharePodLib.Parsers
{
	/// <summary>
	/// Internally used by SharePodLib.  Should not be used from external code. 
	/// </summary>
    public abstract class BaseDatabaseElement
    {
        protected char[] _identifier;
        protected int _headerSize;
        protected int _sectionSize;
        protected byte[] _unusedHeader;
        protected int _requiredHeaderSize;
        protected IPod _iPod;
        
        protected bool ValidateHeader(string validIdentifier)
        {
            string strIdentifier = new string(_identifier);
            if (strIdentifier != validIdentifier)
            {
                throw new ParseException(validIdentifier + " expected, but " + strIdentifier + " found", null);
            }

            if (_headerSize < _requiredHeaderSize)
            {
                throw new UnsupportedITunesVersionException(string.Format("Expected {0} section with length {1}, but found length {2}.", strIdentifier, _requiredHeaderSize, _headerSize),
                    CompatibilityType.NotWritable);
            }
            return true;
        }

        protected void ReadToHeaderEnd(BinaryReader reader)
        {
            int unusedHeaderSize = _headerSize - _requiredHeaderSize;
            _unusedHeader = new byte[unusedHeaderSize];
            _unusedHeader = reader.ReadBytes(unusedHeaderSize);
        }

        internal virtual void Read(IPod iPod, BinaryReader reader)
        {
            _iPod = iPod;
        }

        internal abstract void Write(BinaryWriter writer);
        internal abstract int GetSectionSize();

        internal StringMHOD GetChildByType(List<BaseMHODElement> children, int type)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is StringMHOD && children[i].Type == type)
                {
                    return (StringMHOD)children[i];
                }
            }
            return null;
        }

        internal string GetDataElement(List<BaseMHODElement> children, int type)
        {
            StringMHOD mhod = GetChildByType(children, type);
            if (mhod != null)
                return mhod.Data;
            else
                return String.Empty;
        }

        //protected void SetDataElement(List<BaseMHODElement> children, MHODElementType type, string data)
        //{
        //    StringMHOD mhod = GetChildByType(children, type);
        //    if (mhod != null)
        //    {
        //        mhod.Data = data;
        //    }
        //    else
        //    {
        //        StringMHOD newSection = new StringMHOD(type);
        //        newSection.Data = data;
        //        children.Add(newSection);
        //    }
        //    _isDirty = true;
        //}
    }
}
