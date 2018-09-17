using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET.Compression;
using FFmpeg.NET.Events;

namespace FFmpeg.NET.Engine
{
    public sealed class FFmpeg
    {
        private static readonly object Lock = new object();

        public FFmpeg()
        {
            lock (Lock)
            {
                EnsureInitialized();
            }
        }

        private static string FFmpegFilePath
        {
            get
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyPath = Path.GetDirectoryName(assemblyLocation);
                return Path.Combine(assemblyPath, @"ffmpeg.exe");
            }
        }

        public event EventHandler<ConversionProgressEventArgs> Progress;
        public event EventHandler<ConversionErrorEventArgs> Error;
        public event EventHandler<ConversionCompleteEventArgs> Complete;
        public event EventHandler<ConversionDataEventArgs> Data;

        public async Task<MetaData> GetMetaData(MediaObject media, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameters = new FFmpegParameters(media, null, FFmpegTask.GetMetaData, null);
            await Execute(parameters, cancellationToken);
            return parameters.Input.MetaData;
        }

        public async Task<MediaObject> GetThumbnail(MediaObject input, MediaObject output, ConversionOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameters = new FFmpegParameters(input, output, FFmpegTask.GetThumbnail, options);
            await Execute(parameters, cancellationToken);
            return parameters.Output;
        }

        public async Task<MediaObject> Convert(MediaObject input, MediaObject output, ConversionOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameters = new FFmpegParameters(input, output, FFmpegTask.Convert, options);
            await Execute(parameters, cancellationToken);
            return parameters.Output;
        }


        private void EnsureFFmpegFileExists()
        {
            if (!File.Exists(FFmpegFilePath)) UnpackFFmpegExecutable(FFmpegFilePath);
        }

        private void EnsureDirectoryExists()
        {
            var directory = Path.GetDirectoryName(FFmpegFilePath) ?? Directory.GetCurrentDirectory();
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        private void UnpackFFmpegExecutable(string path)
        {
            using (var compressedFFmpegStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Resources.Resources.FFmpegManifestResourceName))
            {
                if (compressedFFmpegStream == null)
                    throw new Exception(Resources.Resources.Exceptions_Null_FFmpeg_Gzip_Stream);

                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                {
                    var compressorFactory = new CompressorFactory();
                    var compressor = compressorFactory.CreateCompressor(CompressorType.GZip);
                    compressor.Decompress(compressedFFmpegStream, fileStream);
                }
            }
        }

        private void EnsureInitialized()
        {
            EnsureDirectoryExists();
            EnsureFFmpegFileExists();
        }

        private async Task Execute(FFmpegParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ffmpegProcess = new FFmpegProcess();
            ffmpegProcess.Progress += OnProgress;
            ffmpegProcess.Completed += OnComplete;
            ffmpegProcess.Error += OnError;
            ffmpegProcess.Data += OnData;
            await ffmpegProcess.Execute(parameters, FFmpegFilePath, cancellationToken);
        }

        private void OnProgress(ConversionProgressEventArgs e)
        {
            Progress?.Invoke(this, e);
        }

        private void OnError(ConversionErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        private void OnComplete(ConversionCompleteEventArgs e)
        {
            Complete?.Invoke(this, e);
        }

        private void OnData(ConversionDataEventArgs e)
        {
            Data?.Invoke(this, e);
        }
    }
}