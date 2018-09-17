using System;
using System.IO;

namespace FFmpeg.NET.Tests.Fixtures
{
    public class MediaFileFixture
    {
        public FileInfo VideoFile => new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MediaFiles\SampleVideo_1280x720_1mb.mp4"));
        public MediaObject Video => new MediaObject(VideoFile.OpenRead());

        public FileInfo AudioFile => new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MediaFiles\SampleAudio_0.4mb.mp3"));
        public MediaObject Audio => new MediaObject(AudioFile.OpenRead());
    }
}