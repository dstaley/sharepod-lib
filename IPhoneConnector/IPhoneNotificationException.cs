using System;
using System.Collections.Generic;
using System.Text;

namespace IPhoneConnector
{
    public class IPhoneNotificationException : Exception
    {
        public IPhoneNotificationException() :
            base("Couldn't listen for iPhone connections. Please check that the Apple Mobile Device service is installed and running.") { }
    }
}
