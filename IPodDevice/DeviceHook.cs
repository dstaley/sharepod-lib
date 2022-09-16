/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using SharePodLib.WinApi;
using System.Runtime.InteropServices;
using System.IO;

namespace SharePodLib
{
    internal class HookEventArgs : EventArgs
    {
        public int HookCode;
        public IntPtr wParam;
        public IntPtr lParam;
    }
        
    /// <summary>
    /// Summary description for Win32Hooks.
    /// </summary>
    internal class Win32Hook
    {

        protected IntPtr m_hhook = IntPtr.Zero;
        protected HookProc m_filterFunc = null;
        protected HookType m_hookType;

        public delegate void HookEventHandler(object sender, HookEventArgs  e);

        public event HookEventHandler HookInvoked;
        protected void OnHookInvoked(HookEventArgs e)
        {
            if (HookInvoked != null)
            {
                HookInvoked(this, e);
            }
        }

        public Win32Hook(HookType hook)
        {
            m_hookType = hook;
            m_filterFunc = new HookProc(this.CoreHookProc);
        }
        public Win32Hook(HookType hook, HookProc func)
        {
            m_hookType = hook;
            m_filterFunc = func;
        }

        internal int CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
            {
                return ApiFunctions.CallNextHookEx(m_hhook, code, wParam, lParam);
            }

            HookEventArgs e = new HookEventArgs();
            e.HookCode = code;
            e.wParam = wParam;
            e.lParam = lParam;
            OnHookInvoked(e);

            return ApiFunctions.CallNextHookEx(m_hhook, code, wParam, lParam);
        }

        public void Install()
        {

            m_hhook = ApiFunctions.SetWindowsHookEx(m_hookType, m_filterFunc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
        }

        public void Uninstall()
        {
            ApiFunctions.UnhookWindowsHookEx(m_hhook);
        }
    }

    internal class DeviceChangeEventArgs
    {
        public string Drive;
        public DriveInfo DriveInfo;
        public IntPtr EventHwnd;
    }

    internal class DeviceChangeHook : Win32Hook
    {
        const int WM_DEVICECHANGE = 0x0219;

        private static IntPtr _forHWnd;
        public static IntPtr ForHWnd
        {
            get { return DeviceChangeHook._forHWnd; }
            set { DeviceChangeHook._forHWnd = value; }
        }

        //http://msdn.microsoft.com/library/d...s/CWPSTRUCT.asp
        [StructLayout(LayoutKind.Sequential)]
        protected class Cwp
        {
            public IntPtr lParam;
            public int wParam;
            public ushort message;
            public IntPtr hwnd;
        }

        public delegate void DeviceChangeEventHandler(object sender, DeviceChangeEventArgs dce);
        public event DeviceChangeEventHandler DeviceArrived;
        public event DeviceChangeEventHandler DeviceRemoved;

      
        public DeviceChangeHook(IntPtr hWnd)
            : base(HookType.WH_CALLWNDPROC)
        {
            DeviceChangeHook._forHWnd = hWnd;
            this.HookInvoked += new HookEventHandler(GetMessageHookInvoked);
        }

        private void GetMessageHookInvoked(object sender, HookEventArgs he)
        {

            Cwp eventMessage = new Cwp();
            eventMessage = (Cwp)Marshal.PtrToStructure(he.lParam,
            eventMessage.GetType());
            if ((eventMessage.message == WM_DEVICECHANGE) && (eventMessage.hwnd == DeviceChangeHook._forHWnd))
            {
                DeviceChangeEvent action = (DeviceChangeEvent)eventMessage.wParam;
                DEV_BROADCAST_VOLUME eventInfo = new DEV_BROADCAST_VOLUME();
                
                DeviceChangeEventArgs changeArgs = new DeviceChangeEventArgs();

                switch (action)
                {
                    case DeviceChangeEvent.DBT_DEVICEARRIVAL:
                        eventInfo = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(eventMessage.lParam, eventInfo.GetType());
                        changeArgs.Drive = DecodeDriveLetter(eventInfo.dbcv_unitmask);
                        changeArgs.DriveInfo = new DriveInfo(changeArgs.Drive);
                        changeArgs.EventHwnd = eventMessage.hwnd;
                        DeviceArrived(this, changeArgs);
                        break;
                    case DeviceChangeEvent.DBT_DEVICEREMOVECOMPLETE:
                        eventInfo = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(eventMessage.lParam, eventInfo.GetType());
                        changeArgs.Drive = DecodeDriveLetter(eventInfo.dbcv_unitmask);
                        changeArgs.DriveInfo = null;
                        changeArgs.EventHwnd = eventMessage.hwnd;
                        DeviceRemoved(this, changeArgs);
                        break;
                    default:
                        break;
                }
            }
        }

        protected string DecodeDriveLetter(int unitMask)
        {
            char driveLetter;
            for (driveLetter = 'A'; driveLetter <= 'Z'; ++driveLetter)
            {
                if ((unitMask & 0x1) == 0x1)
                    break;
                unitMask = unitMask >> 1;
            }

            if (driveLetter > 'Z') // Catch any unitMask failure
                driveLetter = '0';

            return driveLetter.ToString() + ":";
        }
    }

    
}
