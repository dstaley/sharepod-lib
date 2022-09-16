/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 


using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace SharePodLib.Databinding
{
    class PropertyComparer<T> : System.Collections.Generic.IComparer<T>
    {
        // The following code contains code implemented by Rockford Lhotka:
        // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnadvnet/html/vbnet01272004.asp

        //private PropertyDescriptor _property;
        //private ListSortDirection _direction;
        private ListSortDescriptionCollection _sortDescriptionList;
        

        public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            //_property = property;
            //_direction = direction;
            ListSortDescription description = new ListSortDescription(property, direction);
            _sortDescriptionList = new ListSortDescriptionCollection(new ListSortDescription[] { description } );
            
        }

        public PropertyComparer(ListSortDescriptionCollection list)
        {
            _sortDescriptionList = list;
        }

        #region IComparer<T>

        public int Compare(T xWord, T yWord)
        {
            // Get property values
            //object xValue = GetPropertyValue(xWord, _property.Name);
            //object yValue = GetPropertyValue(yWord, _property.Name);
            
            return RecursiveCompareInternal(xWord, yWord, 0);
            //return CompareValues(xValue, yValue);

        }

        public int CompareValues(object xValue, object yValue, ListSortDirection direction)
        {
           
            // Determine sort order
            int result = 0;
            if (direction == ListSortDirection.Ascending)
            {
                return CompareAscending(xValue, yValue);
            }
            else
            {
                return CompareDescending(xValue, yValue);
            }

            //if (result == 0)
            //    return Compare(
        }



        private int RecursiveCompareInternal(T x, T y, int index)
        {
            if (index >= _sortDescriptionList.Count)
                return 0; // termination condition

            ListSortDescription listSortDesc = _sortDescriptionList[index];
            object xValue = listSortDesc.PropertyDescriptor.GetValue(x);
            object yValue = listSortDesc.PropertyDescriptor.GetValue(y);

            int retValue = CompareValues(xValue, yValue, listSortDesc.SortDirection);
            if (retValue == 0)
            {
                return RecursiveCompareInternal(x, y, ++index);
            }
            else
            {
                return retValue;
            }
        }

        public bool Equals(T xWord, T yWord)
        {
            return xWord.Equals(yWord);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        #endregion

        // Compare two property values of any type
        private int CompareAscending(object xValue, object yValue)
        {
            if (xValue == null || yValue == null)
                return 0;

            int result;

            // If values implement IComparer
            if (xValue is IComparable)
            {
                result = ((IComparable)xValue).CompareTo(yValue);
            }
            // If values don't implement IComparer but are equivalent
            else if (xValue.Equals(yValue))
            {
                result = 0;
            }
            // Values don't implement IComparer and are not equivalent, so compare as string values
            else result = xValue.ToString().CompareTo(yValue.ToString());

            // Return result
            return result;
        }

        private int CompareDescending(object xValue, object yValue)
        {
            // Return result adjusted for ascending or descending sort order ie
            // multiplied by 1 for ascending or -1 for descending
            return CompareAscending(xValue, yValue) * -1;
        }

        private object GetPropertyValue(T value, string property)
        {
            // Get property
            PropertyInfo propertyInfo = value.GetType().GetProperty(property);

            // Return value
            return propertyInfo.GetValue(value, null);
        }
    }
}
