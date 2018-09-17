using System.IO;

namespace FFmpeg.NET
{
    public class MediaObject
    {
        public MediaObject(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; }
        internal MetaData MetaData { get; set; }
    }
}