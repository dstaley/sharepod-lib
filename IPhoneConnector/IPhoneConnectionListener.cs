using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace IPhoneConnector
{
	public static class IPhoneConnectionListener
	{
		public static event EventHandler Connected;
		public static event EventHandler Disconnected;

		public static List<IPhone> Connections { get; private set; }

		private static DeviceNotificationCallback _deviceNotificationDelegate = null;
        private static FileSystemAccess _access = FileSystemAccess.Standard;

        public static bool IsListening
        {
            get { return _deviceNotificationDelegate != null; }
        }

        public static void ListenForEvents(FileSystemAccess access)
        {
            _access = access;

            if (!IsListening)
            {
                Connections = new List<IPhone>();
                _deviceNotificationDelegate = new DeviceNotificationCallback(DeviceNotificationCallback);
                AMDeviceNotification notification = new AMDeviceNotification();

                int ret = AppleMobileDeviceAPI.AMDeviceNotificationSubscribe(_deviceNotificationDelegate, 0, 0, 0, ref notification);

                Trace.WriteLine("AMDeviceNotificationSubscribe: " + ret, IPhone.TraceCategory);
                if (ret != 0)
                {
                    _deviceNotificationDelegate = null;
                    throw new IPhoneNotificationException();
                }
            }
        }

		private static void DeviceNotificationCallback(ref AMDeviceNotificationCallbackInfo callback)
		{
			if (callback.msg == NotificationMessage.Connected)
			{
				IntPtr device = callback.dev_ptr;
				IPhone iPhone = new IPhone(device, _access);
				if (iPhone.IsConnected)
				{
					Connections.Add(iPhone);
				}
				if (Connected != null) Connected(iPhone, null);
			}
			else if (callback.msg == NotificationMessage.Disconnected)
			{
				IntPtr devPtr = callback.dev_ptr;
				IPhone iPhone = Connections.Find(a => a.DeviceHandle == devPtr);
				if (iPhone != null)
				{
					iPhone.OnDisconnect();
					Connections.Remove(iPhone);
					if (Disconnected != null) Disconnected(iPhone, null);
				}
			}
		}

		public static void StopListeningForEvents()
		{
			int ret = AppleMobileDeviceAPI.AMDeviceNotificationUnsubscribe();
            _deviceNotificationDelegate = null;
			Trace.WriteLine("AMDeviceNotificationUnsubscribe: " + ret, IPhone.TraceCategory);
		}

		public static IPhone WaitForSingleConnection(int timeoutSeconds)
		{
			IPhone phone = null;
			int time = 0;
			while (time < timeoutSeconds * 1000)
			{
				if (Connections.Count > 0)
				{
					phone = Connections[0];

					if (phone.IsConnecting)
						break;

					if (phone.IsConnected)
						return phone;
				}

				Thread.Sleep(100);
				time += 100;
			}

			if (phone == null) return null;

			while (phone.IsConnecting)
			{
				Thread.Sleep(100);
			}
			return phone;
		}
	}
}
