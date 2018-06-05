using System;

namespace FFmpeg.NET.Engine
{
    internal class FFmpegParameters
    {
        internal FFmpegParameters(MediaObject input, MediaObject output, FFmpegTask task, ConversionOptions options)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Output = output;
            Task = task;
            ConversionOptions = options ?? throw new ArgumentNullException(nameof(options));
        }

        internal ConversionOptions ConversionOptions { get; }
        internal FFmpegTask Task { get; }
        internal MediaObject Output { get; }
        internal MediaObject Input { get; }
    }
}