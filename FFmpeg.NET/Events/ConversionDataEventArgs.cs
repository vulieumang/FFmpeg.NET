using System;

namespace FFmpeg.NET.Events
{
    public class ConversionDataEventArgs : EventArgs
    {
        public ConversionDataEventArgs(string data, MediaObject input, MediaObject output)
        {
            Data = data;
            Input = input;
            Output = output;
        }

        public string Data { get; }
        public MediaObject Input { get; }
        public MediaObject Output { get; }
    }
}