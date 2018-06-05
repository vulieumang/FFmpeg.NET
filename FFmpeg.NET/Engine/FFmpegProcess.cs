using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET.Events;
using FFmpeg.NET.Exceptions;

namespace FFmpeg.NET.Engine
{
    internal sealed class FFmpegProcess
    {
        public async Task Execute(FFmpegParameters parameters, string ffmpegFilePath, CancellationToken cancellationToken = default(CancellationToken))
        {
            var startInfo = GenerateStartInfo(parameters, ffmpegFilePath);
            await ExecuteStreamAsync(startInfo, parameters, cancellationToken);
        }

        private async Task ExecuteStreamAsync(ProcessStartInfo startInfo, FFmpegParameters parameters, CancellationToken cancellationToken)
        {
           var messages = new List<string>();
            Exception caughtException = null;

            using (var ffmpegProcess = new Process {StartInfo = startInfo, EnableRaisingEvents = true})
            {
                ffmpegProcess.Exited += (sender, e) =>
                {
                    var process = (Process) sender;
                    if (process.ExitCode != 0 || caughtException != null)
                    {
                        var message = messages.Count > 0 ? string.Join("", messages) : null;
                        var exception = new FFmpegException(message, caughtException, process.ExitCode);
                        OnConversionError(new ConversionErrorEventArgs(exception, parameters.Input, parameters.Output));
                    }
                    else
                    {
                        OnConversionCompleted(new ConversionCompleteEventArgs(parameters.Input, parameters.Output));
                    }
                };
                ffmpegProcess.ErrorDataReceived += (sender, e) => OnData(new ConversionDataEventArgs(e.Data, parameters.Input, parameters.Output));
                ffmpegProcess.ErrorDataReceived += (sender, e) => FFmpegProcessOnErrorDataReceived(e, parameters, ref caughtException, messages);

                ffmpegProcess.Start();
                ffmpegProcess.BeginErrorReadLine();

                var inputTask = Task.Run(() =>
                {
                    parameters.Input.Stream.CopyTo(ffmpegProcess.StandardInput.BaseStream);
                    ffmpegProcess.StandardInput.Close();
                }, cancellationToken);

                var outputTask = Task.Run(() => { ffmpegProcess.StandardOutput.BaseStream.CopyTo(parameters.Output.Stream); }, cancellationToken);

                await Task.WhenAll(inputTask, outputTask);

                ffmpegProcess.WaitForExit();
            }
        }

        private void FFmpegProcessOnErrorDataReceived(DataReceivedEventArgs e, FFmpegParameters parameters, ref Exception exception, List<string> messages)
        {
            var totalMediaDuration = new TimeSpan();
            if (e.Data == null) return;

            try
            {
                messages.Insert(0, e.Data);
                if (parameters.Input != null)
                {
                    RegexEngine.TestVideo(e.Data, parameters);
                    RegexEngine.TestAudio(e.Data, parameters);

                    var matchDuration = RegexEngine.Index[RegexEngine.Find.Duration].Match(e.Data);
                    if (matchDuration.Success)
                    {
                        if (parameters.Input.MetaData == null)
                            parameters.Input.MetaData = new MetaData();

                        TimeSpan.TryParse(matchDuration.Groups[1].Value, out totalMediaDuration);
                        parameters.Input.MetaData.Duration = totalMediaDuration;
                    }
                }

                if (RegexEngine.IsProgressData(e.Data, out var progressData))
                {
                    progressData.TotalDuration = totalMediaDuration;
                    OnProgressChanged(new ConversionProgressEventArgs(progressData, parameters.Input, parameters.Output));
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        private ProcessStartInfo GenerateStartInfo(FFmpegParameters parameters, string ffmpegPath)
        {
            return new StreamToFileStartInfoBuilder().Build(parameters, ffmpegPath);
        }

        public event Action<ConversionProgressEventArgs> Progress;
        public event Action<ConversionCompleteEventArgs> Completed;
        public event Action<ConversionErrorEventArgs> Error;
        public event Action<ConversionDataEventArgs> Data;

        private void OnProgressChanged(ConversionProgressEventArgs eventArgs)
        {
            Progress?.Invoke(eventArgs);
        }

        private void OnConversionCompleted(ConversionCompleteEventArgs eventArgs)
        {
            Completed?.Invoke(eventArgs);
        }

        private void OnConversionError(ConversionErrorEventArgs eventArgs)
        {
            Error?.Invoke(eventArgs);
        }

        private void OnData(ConversionDataEventArgs eventArgs)
        {
            Data?.Invoke(eventArgs);
        }
    }

    internal class StreamToFileStartInfoBuilder
    {
        public ProcessStartInfo Build(FFmpegParameters parameters, string ffmpegPath)
        {
            var argumentBuilder = new FFmpegArgumentBuilder();
            var arguments = argumentBuilder.Build(parameters);
            return new ProcessStartInfo
            {
                Arguments = "-y -loglevel info " + arguments,
                FileName = ffmpegPath,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }
    }
}