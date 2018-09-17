using System;
using FFmpeg.NET.Exceptions;

namespace FFmpeg.NET.Events
{
    public class ConversionErrorEventArgs : EventArgs
    {
        public ConversionErrorEventArgs(FFmpegException exception, MediaObject input, MediaObject output)
        {
            Exception = exception;
            Input = input;
            Output = output;
        }

        public FFmpegException Exception { get; }
        public MediaObject Input { get; }
        public MediaObject Output { get; }
    }
}