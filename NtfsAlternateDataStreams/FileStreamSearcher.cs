using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace NtfsAlternateDataStreams
{
    public class FileStreamSearcher { 
        private const int ERROR_HANDLE_EOF = 38; 
        private enum StreamInfoLevels { FindStreamInfoStandard = 0 }
        
        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)] 
        private static extern SafeFindHandle FindFirstStreamW(string lpFileName, StreamInfoLevels InfoLevel, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_STREAM_DATA lpFindStreamData, uint dwFlags);
        
        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)] 
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindNextStreamW(SafeFindHandle hndFindFile, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_STREAM_DATA lpFindStreamData);
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] 
        private class WIN32_FIND_STREAM_DATA { 
            public long StreamSize;
            
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 296)] 
            public string cStreamName; 
        } 
        
        public static IEnumerable<string> GetStream(FileInfo file) { 
            if (file == null) throw new ArgumentNullException("file"); 
            WIN32_FIND_STREAM_DATA findStreamData = new WIN32_FIND_STREAM_DATA(); 
            SafeFindHandle handle = FindFirstStreamW(file.FullName, StreamInfoLevels.FindStreamInfoStandard, findStreamData, 0); 
            if (handle.IsInvalid) 
                throw new Win32Exception(); 
            try { 
                do { 
                    yield return findStreamData.cStreamName; 
                } while (FindNextStreamW(handle, findStreamData)); 
                
                int lastError = Marshal.GetLastWin32Error(); 
                if (lastError != ERROR_HANDLE_EOF) throw new Win32Exception(lastError); 
            } 
            finally { 
                handle.Dispose(); 
            } 
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BackupRead(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, [MarshalAs(UnmanagedType.Bool)] bool bAbort, [MarshalAs(UnmanagedType.Bool)] bool bProcessSecurity, ref IntPtr lpContext);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BackupSeek(SafeFileHandle hFile, uint dwLowBytesToSeek, uint dwHighBytesToSeek, out uint lpdwLowByteSeeked, out uint lpdwHighByteSeeked, ref IntPtr lpContext);

        public static IEnumerable<StreamInfo> GetStreamInfo(FileInfo file)
        {
            const int bufferSize = 4096;
            using (FileStream fs = file.OpenRead())
            {
                IntPtr context = IntPtr.Zero;
                IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

                try
                {
                    while (true)
                    {
                        uint numRead;

                        if (!BackupRead(fs.SafeFileHandle, buffer, (uint)Marshal.SizeOf(typeof(Win32StreamID)), out numRead, false, true, ref context))
                            throw new Win32Exception();

                        if (numRead > 0)
                        {
                            Win32StreamID streamID = (Win32StreamID)Marshal.PtrToStructure(buffer, typeof(Win32StreamID));
                            string name = null;
                            if (streamID.dwStreamNameSize > 0)
                            {
                                if (!BackupRead(fs.SafeFileHandle, buffer, (uint)Math.Min(bufferSize, streamID.dwStreamNameSize), out numRead, false, true, ref context))
                                    throw new Win32Exception(); name = Marshal.PtrToStringUni(buffer, (int)numRead / 2);
                            }
                            yield return new StreamInfo(name, (StreamType)streamID.dwStreamId, streamID.Size);

                            if (streamID.Size > 0)
                            {
                                uint lo, hi;
                                BackupSeek(fs.SafeFileHandle, uint.MaxValue, int.MaxValue, out lo, out hi, ref context);
                            }
                        }
                        else
                            break;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer); uint numRead;
                    if (!BackupRead(fs.SafeFileHandle, IntPtr.Zero, 0, out numRead, true, false, ref context))
                        throw new Win32Exception();
                }
            }
        }
    }
}
