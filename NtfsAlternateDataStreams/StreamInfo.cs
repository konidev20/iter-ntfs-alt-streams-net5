namespace NtfsAlternateDataStreams
{
    public struct StreamInfo { 
        public StreamInfo(string name, StreamType type, long size) 
        { 
            Name = name; 
            Type = type; 
            Size = size; 
        } 
        
        public readonly string Name; 
        public readonly StreamType Type; 
        public readonly long Size; 
    }
}
