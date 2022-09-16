/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;

namespace SharePodLib.Forms
{
    internal partial class LoadingForm : Form
    {
        DateTime _loadStartTime;

        public LoadingForm()
        {
			InitializeComponent();
			_loadStartTime = DateTime.Now;

            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            versionLabel.Text = "v" + v.ToString();
        }

        public void OnLoadComplete()
        {
            //hack of a way to show the license warning
            if (!SharePodLib.IsLicenced())
            {
                Application.DoEvents();
            }            

			while (!CanClose())
			{
				Thread.Sleep(200);
			}
			this.Close();
        }

        public bool CanClose()
        {
            int timeout = 2;
            if (new TimeSpan(DateTime.Now.Ticks - _loadStartTime.Ticks).TotalSeconds < timeout)
            {
                return false;
            }
            return true;
        }
    }
}