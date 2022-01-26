using System.Runtime.InteropServices;

namespace NtfsAlternateDataStreams
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)] 
    public struct Win32StreamID { 
        public int dwStreamId; 
        public int dwStreamAttributes; 
        public long Size; 
        public int dwStreamNameSize; 
    }
}
