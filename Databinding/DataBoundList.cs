/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace SharePodLib.Databinding
{
	/// <summary>
	/// Class for DataBinding lists.  (See TracksList, PlaylistList, Playlist).  
	/// </summary>
	/// <typeparam name="T"></typeparam>
    public class DataBoundList<T> : SortableFilteringBindingList<T>
    {
        internal DataBoundList()
        {
            this.AllowRemove = false;
            this.AllowEdit = true;
            this.AllowNew = false;
            this.ListChanged += new ListChangedEventHandler(DataBoundTrackList_ListChanged);
            this.RaiseListChangedEvents = false;            
        }
        

        protected override void RemoveItem(int index)
        {
            if (this.OriginalList.Count > 0)
            {
                this.OriginalList.Remove(this[index]);
            }
            base.RemoveItem(index);
        }

        void DataBoundTrackList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (this.OriginalList.Count > 0)
                {
                    this.OriginalList.Add(this[e.NewIndex]);
                }
            }
        }

        public T Find(Predicate<T> match)
        {
            return ((List<T>)this.Items).Find(match);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            return ((List<T>)this.Items).FindAll(match);
        }
    }
}
