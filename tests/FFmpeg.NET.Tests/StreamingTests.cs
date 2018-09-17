using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.NET.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace FFmpeg.NET.Tests
{
    public class StreamingTests : IClassFixture<MediaFileFixture>
    {
        public StreamingTests(MediaFileFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _output = outputHelper;
        }

        private readonly MediaFileFixture _fixture;
        private readonly ITestOutputHelper _output;


        [Fact]
        public async Task FFmpeg_Process_Can_Handle_Input_Streams()
        {
            using (var process = new Process())
            {
                var startInfo = new ProcessStartInfo
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = "-y -i - -f mp3 -",
                    FileName = "ffmpeg.exe"
                };
                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;

                process.ErrorDataReceived += (sender, eventArgs) => { _output.WriteLine(eventArgs.Data); };

                process.Start();
                process.BeginErrorReadLine();

                var inputTask = Task.Run(() =>
                {
                    using (var input = _fixture.VideoFile.OpenRead())
                    {
                        input.CopyTo(process.StandardInput.BaseStream);
                        process.StandardInput.Close();
                    }
                });

                var outputTask = Task.Run(() =>
                {
                    using (var output = new MemoryStream())
                    {
                        process.StandardOutput.BaseStream.CopyTo(output);
                        Assert.NotEmpty(output.ToArray());
                    }
                });

                await Task.WhenAll(inputTask, outputTask);

                process.WaitForExit();
            }
        }
    }
}