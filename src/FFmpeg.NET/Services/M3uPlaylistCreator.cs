using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FFmpeg.NET.Services
{
    public class M3uPlaylistCreator : IPlaylistCreator
    {
        public string Create(IDictionary<FileInfo,MetaData> files)
        {
            if (files == null)
                throw new ArgumentException(nameof(files));

            var sb = new StringBuilder();
            sb.AppendLine("#EXTM3U");
            foreach (var file in files)
            {
                sb.AppendLine($"#EXTINF:{(int)file.Value.Duration.TotalSeconds},{file.Key.Name}");
                sb.AppendLine($"file:///{file.Key.FullName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}");
            }

            return sb.ToString();
        }
    }
}