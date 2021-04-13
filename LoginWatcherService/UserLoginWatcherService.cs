using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LoginWatcherService
{
    public partial class UserLoginWatcherService : ServiceBase
    {
        public UserLoginWatcherService()
        {
            //InitializeComponent();
             CanPauseAndContinue = true;
        CanHandleSessionChangeEvent = true;
        ServiceName = "SimpleService";
        }



        protected override void OnStart(string[] args)
        {

        }

        protected override void OnStop()
        {
        }


        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);
        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);

        private enum WtsInfoClass
        {
            WTSUserName = 5,
            WTSDomainName = 7,
        }

        private static string GetUsername(int sessionId, bool prependDomain = true)
        {
            IntPtr buffer;
            int strLen;
            string username = "SYSTEM";
            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) && strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory(buffer);
                if (prependDomain)
                {
                    if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer, out strLen) && strLen > 1)
                    {
                        username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
                        WTSFreeMemory(buffer);
                    }
                }
            }
            return username;
        }


        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
           Console.WriteLine("SimpleService.OnSessionChange", DateTime.Now.ToLongTimeString() +
                " - Session change notice received: " +
                changeDescription.Reason.ToString() + "  Session ID: " +
                GetUsername(changeDescription.SessionId));
           

            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                    Console.WriteLine($"SimpleService.OnSessionChange: Logon: {GetUsername(changeDescription.SessionId)}");
                    break;

                case SessionChangeReason.SessionLogoff:
                    Console.WriteLine($"SimpleService.OnSessionChange: LogOff: {GetUsername(changeDescription.SessionId)}");
                    break;
                case SessionChangeReason.SessionLock:
                    Console.WriteLine($"SimpleService.OnSessionChange: Lock: {GetUsername(changeDescription.SessionId)}");
                    break;
                case SessionChangeReason.SessionUnlock:
                     Console.WriteLine($"SimpleService.OnSessionChange: Lock: {GetUsername(changeDescription.SessionId)}");
                    break;

            }

        }
    }
}
