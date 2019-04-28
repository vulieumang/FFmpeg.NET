using System;
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
            using (var input = File.OpenRead(_fixture.AudioFile.FullName))
            using (var output = await ProcessStream(input))
            using (var fs = File.OpenWrite(Path.ChangeExtension(_fixture.AudioFile.FullName, ".mp3")))
            {
                await output.CopyToAsync(fs);
            }
        }

        [Fact]
        public async Task FFmpeg_Process_Can_Handle_Input_Streams_By_Reference()
        {
            using (var input = File.OpenRead(_fixture.AudioFile.FullName))
            using (var output = File.OpenWrite(Path.ChangeExtension(_fixture.AudioFile.FullName, ".mp3")))
            {
                await ProcessStream(input, output);
            }
        }

        private async Task<Stream> ProcessStream(Stream input)
        {
            var output = new MemoryStream();
            await ProcessStream(input, output);
            output.Position = 0;
            return output;
        }

        private async Task ProcessStream(Stream input, Stream output)
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

                process.ErrorDataReceived += (sender, eventArgs) => { _output.WriteLine(eventArgs?.Data ?? string.Empty); };

                process.Start();
                process.BeginErrorReadLine();

                // copytoasync will flush the whole stream at once.
                // this is not effectively piping, buit just waiting till everything is done
                // TODO refactor with ReadAsync and WriteAsync
                // starting a new task is also a bit nasty but pretty much unavoidable
                // starting multiple conversions at once could potentially end up in a deadlock by the threadpool :x
                var inputTask = Task.Run(async () =>
                {
                    await input.CopyToAsync(process.StandardInput.BaseStream);
                    process.StandardInput.Close();
                });

                // copytoasync will flush the whole stream at once.
                // this is not effectively piping, buit just waiting till everything is done
                // TODO refactor with ReadAsync and WriteAsync
                // starting a new task is also a bit nasty but pretty much unavoidable
                // starting multiple conversions at once could potentially end up in a deadlock by the threadpool :x
                var outputTask = Task.Run(async () =>
                {
                    await process.StandardOutput.BaseStream.CopyToAsync(output);
                });

                await Task.WhenAll(inputTask, outputTask);
                process.WaitForExit();
            }
        }
    }
}
