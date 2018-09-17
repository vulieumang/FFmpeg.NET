using System.Collections.Generic;
using System.IO;

namespace FFmpeg.NET.Services
{
    public interface IPlaylistCreator
    {
        string Create(IDictionary<FileInfo, MetaData> metaData);
    }
}