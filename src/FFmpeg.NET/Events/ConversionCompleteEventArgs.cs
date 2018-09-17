using System;

namespace FFmpeg.NET.Events
{
    public class ConversionCompleteEventArgs : EventArgs
    {
        public ConversionCompleteEventArgs(MediaObject input, MediaObject output)
        {
            Input = input;
            Output = output;
        }

        public MediaObject Input { get; }
        public MediaObject Output { get; }
    }
}