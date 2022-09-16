/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.Exceptions;

namespace SharePodLib.DataTypes
{
	/// <summary>
	/// Wraps an iPod-format rating and a human-readable Star Rating
	/// </summary>
    public class IPodRating : IComparable
    {

        byte _rating;
        string _ratingString;

        internal IPodRating(byte iTunesRating)
        {
            if (iTunesRating < 0 || iTunesRating > 100)
            {
                iTunesRating = 0;
            }
            _rating = iTunesRating;
            _ratingString = new string('*', (int)_rating / 20);
        }

        public IPodRating(int starRating)
        {
            if (starRating < 0)
                starRating = 0;
            else if (starRating > 5)
                starRating = 5;
            
            _rating = (byte)(starRating * 20);
            _ratingString = new string('*', starRating);
        }


        public int StarRating
        {
            get { return (int)_rating / 20; }
        }

        internal byte ITunesRating
        {
            get { return _rating; }
        }

        public override string ToString()
        {
            return _ratingString;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return StarRating.CompareTo(((IPodRating)obj).StarRating);
        }

        #endregion
    }
}
