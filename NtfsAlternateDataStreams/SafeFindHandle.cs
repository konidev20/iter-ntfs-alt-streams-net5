using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace NtfsAlternateDataStreams
{
    public sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid { 
        private SafeFindHandle() : base(true) 
        { 
        } 

        protected override bool ReleaseHandle() { 
            return FindClose(this.handle); 
        }

        [DllImport("kernel32.dll")] 
        [return: MarshalAs(UnmanagedType.Bool)] 
        private static extern bool FindClose(IntPtr handle); 
    }
}
