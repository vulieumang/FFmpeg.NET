using System;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.NET.Engine;
using FFmpeg.NET.Enums;
using FFmpeg.NET.Events;
using FFmpeg.NET.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace FFmpeg.NET.Tests
{
    public class ConversionTests : IClassFixture<MediaFileFixture>
    {
        public ConversionTests(MediaFileFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _outputHelper = outputHelper;
        }

        private readonly MediaFileFixture _fixture;
        private readonly ITestOutputHelper _outputHelper;

        [Fact]
        public async Task FFmpeg_Invokes_ConversionCompleteEvent()
        {
            var outputFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"MediaFiles\conversionTest.mp4"));

            var ffmpeg = new Engine.FFmpeg();
            ffmpeg.Error += (sender, args) =>
            {
                _outputHelper.WriteLine(args.Exception?.Message ?? string.Empty);
                _outputHelper.WriteLine(args.Exception?.ExitCode.ToString() ?? string.Empty);
            };
            ffmpeg.Data += (sender, args) => _outputHelper.WriteLine(args.Data ?? string.Empty);
            ffmpeg.Complete += (sender, args) =>
            {
                Assert.NotNull(args);
                Assert.NotNull(args.Output);
                _outputHelper.WriteLine("ConversionCompletedEvent: {0}", args);
            };
            ffmpeg.Progress += (sender, args) => _outputHelper.WriteLine(args.ToString());

            using (var outputStream = outputFile.OpenWrite())
            {
                var output = new MediaObject(outputStream);
                await ffmpeg.Convert(_fixture.Video, output, new ConversionOptions {Format = Format.Mp4, Codec = "h264"});
            }

            Assert.True(File.Exists(outputFile.FullName));
            outputFile.Delete();
            Assert.False(File.Exists(outputFile.FullName));

            
        }
    }
}