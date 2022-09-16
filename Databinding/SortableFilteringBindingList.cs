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
	/// Base Class for DataBoundList.  
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SortableFilteringBindingList<T> : BindingList<T>, IBindingListView
	{

		private bool _isSorted;
		//private ListSortDirection _sortDirection;
		//private PropertyDescriptor _sortedProperty;
        ListSortDescriptionCollection _lastSort;
        string _lastFilter;

		private bool _isFiltered;
		private List<T> _originalList = new List<T>();

		internal SortableFilteringBindingList()
			: base()
		{
			//_sortDirection = ListSortDirection.Ascending;
            
		}

		protected List<T> OriginalList
		{
			get { return _originalList; }
			set { _originalList = value; }
		}


		protected override bool SupportsSearchingCore
		{
			get { return true; }
		}

		protected override bool IsSortedCore
		{
			get { return _isSorted; }
		}

		protected override bool SupportsSortingCore
		{
			get { return true; }
		}

        /*
		protected override ListSortDirection SortDirectionCore
		{
			get { return _sortDirection; }
		}

		protected override PropertyDescriptor SortPropertyCore
		{
			get { return _sortedProperty; }
		}*/

		protected override int FindCore(PropertyDescriptor property, object key)
		{
			for (int i = 0; i < Count; i++)
			{
				T item = this[i];
				if (property.GetValue(item).Equals(key))
				{
					return i;
				}
			}
			return -1; // Not found
		}

        
		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			SaveOriginalList();

			//_sortDirection = direction;
			//_sortedProperty = property;

			// Get list to sort
			List<T> items = this.Items as List<T>;

			// Apply and set the sort, if items to sort
			if (items != null)
			{
				PropertyComparer<T> pc = new PropertyComparer<T>(property, direction);
				items.Sort(pc);
				_isSorted = true;
			}
			else
			{
				_isSorted = false;
			}

			this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected override void RemoveSortCore()
		{
            if (_isSorted)
            {
                RestoreOriginalList();
                _isSorted = false;

                if (_isFiltered)
                    ApplyFilter(_lastFilter);
            }
		}

		#region IBindingListView Members

		public void ApplySort(ListSortDescriptionCollection sorts)
		{
            SaveOriginalList();

            _lastSort = sorts;

            //_sortDirection = direction;
            //_sortedProperty = property;

            // Get list to sort
            List<T> items = this.Items as List<T>;

            // Apply and set the sort, if items to sort
            if (items != null)
            {
                PropertyComparer<T> pc = new PropertyComparer<T>(sorts);
                items.Sort(pc);
                _isSorted = true;
            }
            else
            {
                _isSorted = false;
            }

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		public string Filter
		{
			get
			{
				return String.Empty;
			}
			set
			{
				ApplyFilter(value);
			}
		}


		public ListSortDescriptionCollection SortDescriptions
		{
			get { return null; }
		}

		public bool SupportsAdvancedSorting
		{
			get { return true; }
		}

		public bool SupportsFiltering
		{
			get { return true; }
		}

		private void SaveOriginalList()
		{
			if (_originalList.Count == 0)
			{
				_originalList.AddRange(this);
			}
		}

		private void RestoreOriginalList()
		{
			this.ClearItems();
			foreach (T item in _originalList)
			{
				this.Items.Add(item);
			}
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, 0));
		}

		private void ApplyFilter(string filter)
		{
            _lastFilter = filter;

			SaveOriginalList();

			bool wasSorted = _isSorted;
			
			if (_isSorted)
			{
				this.RemoveSortCore();
			}


			int equalsPos = filter.IndexOf('=');

			// Get property name
			string propName = filter.Substring(0, equalsPos).Trim();

			// Get filter criteria
			string criteria = filter.Substring(equalsPos + 1, filter.Length - equalsPos - 1).Trim();

			// Strip leading and trailing quotes
			if (criteria.Contains("\"") || criteria.Contains("'"))
			{
				criteria = criteria.Substring(1, criteria.Length - 2).ToLower();
			}

			// Get a property descriptor for the filter property
			PropertyDescriptor propDesc = TypeDescriptor.GetProperties(typeof(T))[propName];

			List<T> currentCollection = new List<T>(this);

			this.ClearItems();

			foreach (T item in currentCollection)
			{
				object value = propDesc.GetValue(item);
				if (value.ToString().ToLower().Contains(criteria))
				{
					this.Items.Add(item);
				}
			}
			_isFiltered = true;

			if (wasSorted && _lastSort != null)
			{
				this.ApplySort(_lastSort);
			}
			else
			{
				//Only call this if we werent sorted - ApplySortCore calls this for us.
				this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			}
		}

		public void RemoveFilter()
		{
			if (_isFiltered)
			{
				_isFiltered = false;
				RestoreOriginalList();

                if (_isSorted)
                    ApplySort(_lastSort);
			}

		}

		#endregion

        public void ApplyCustomSort(IComparer<T> comparer)
        {
            // Get list to sort
            List<T> items = this.Items as List<T>;

            // Apply and set the sort, if items to sort
            if (items != null)
            {
                items.Sort(comparer);
                _isSorted = true;
            }
            else
            {
                _isSorted = false;
            }

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
	}
}
