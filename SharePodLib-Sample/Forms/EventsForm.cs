using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharePodLib;

namespace SharePodLib_Sample.Forms
{
    public partial class EventsForm : Form
    {
        public EventsForm()
        {
            InitializeComponent();

            SharePodLib.Device.IPodConnected += new SharePodLib.Device.IPodConnectedHandler(Device_IPodConnected);
            SharePodLib.Device.IPodDisconnected += new SharePodLib.Device.IPodDisconnectedHandler(Device_IPodDisconnected);

            this.FormClosed += new FormClosedEventHandler(EventsForm_FormClosed);
        }

        void EventsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SharePodLib.Device.IPodConnected -= new SharePodLib.Device.IPodConnectedHandler(Device_IPodConnected);
            SharePodLib.Device.IPodDisconnected -= new SharePodLib.Device.IPodDisconnectedHandler(Device_IPodDisconnected);

            //We should make sure we stop listening 
            SharePodLib.Device.StopListeningForDeviceChanges();
        }

        private void ListenButton_Click(object sender, EventArgs e)
        {
            SharePodLib.Device.ListenForDeviceChanges(this.Handle);
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            SharePodLib.Device.StopListeningForDeviceChanges();
        }


        void Device_IPodDisconnected(EventArgs args)
        {
            if (args is IPhoneDisconnectEventArgs)
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    EventsListBox.Items.Add("iPhone Disconnected: " + ((IPhoneDisconnectEventArgs)args).Device.SerialNumber);
                }));

            }
            else if (args is StandardIPodDisconnectEventArgs)
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    EventsListBox.Items.Add("iPod disconnected: " + ((StandardIPodDisconnectEventArgs)args).DriveLetter);
                }));
            }
        }

        void Device_IPodConnected(SharePodLib.IPod iPod)
        {
            MethodInvoker mi = new MethodInvoker(delegate()
                {
                    EventsListBox.Items.Add("Connected: " + iPod.DeviceInfo.SerialNumber);
                });
            if (this.InvokeRequired)
                this.Invoke(mi);
            else
                mi.Invoke();
        }
    }
}
