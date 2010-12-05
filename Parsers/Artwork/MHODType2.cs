/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing.Imaging;
using SharePodLib.Parsers.iTunesDB;

namespace SharePodLib.Parsers.Artwork
{

    /// <summary>
    /// 
    /// </summary>
    class MHODType2 : BaseMHODElement
    {
        private IPodImageFormat _childElement;
              
        internal MHODType2()
        {
            _requiredHeaderSize = 16;
            _headerSize = 24;
            _identifier = "mhod".ToCharArray();
            _type = 2;
            _childElement = new IPodImageFormat(false);
        }

        
        #region IDatabaseElement Members

        internal override void Read(IPod iPod, BinaryReader reader)
        {            
            _childElement.Read(iPod, reader);
        }        

        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            _childElement.Write(writer);
        }        

        internal override int GetSectionSize()
        {
            return _headerSize + _childElement.GetSectionSize();
        }
       
        #endregion

        internal IPodImageFormat ArtworkFormat
        {
            get { return _childElement; }
        }

                
        internal void Create(IPod iPod, SupportedArtworkFormat format, byte[] imageData)
        {
            _iPod = iPod;
            _unusedHeader = new byte[_headerSize - _requiredHeaderSize];
            _childElement = new IPodImageFormat(false);
            _childElement.Create(_iPod, format, imageData);
            
        }
    }
}
