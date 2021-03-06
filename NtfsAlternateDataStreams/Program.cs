using System;
using System.IO;

namespace NtfsAlternateDataStreams
{
    class Program
    {
        static void Main(string[] args) { 
            foreach (string path in args) 
            { 
                Console.WriteLine(path + ":"); 
                foreach (StreamInfo stream in FileStreamSearcher.GetStreamInfo(new FileInfo(path))) 
                { 
                    Console.WriteLine("\t{0}\t{1}\t{2}", stream.Name != null ? stream.Name : "(unnamed)", stream.Type, stream.Size); 
                } 
            } 
        }
    }
}
