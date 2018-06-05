using System;
using System.Globalization;
using System.Text;
using FFmpeg.NET.Enums;

namespace FFmpeg.NET.Engine
{
    internal class FFmpegArgumentBuilder
    {
        public string Build(FFmpegParameters parameters)
        {
            switch (parameters.Task)
            {
                case FFmpegTask.Convert:
                    return Convert(parameters);

                case FFmpegTask.GetMetaData:
                    return GetMetadata();

                case FFmpegTask.GetThumbnail:
                    return GetThumbnail(parameters);

                default:
                    throw new NotSupportedException();
            }
        }

        private static string GetMetadata()
        {
            var commandBuilder = new StringBuilder();
            AppendInput(commandBuilder);
            return commandBuilder.ToString();
        }

        private static string GetThumbnail(FFmpegParameters parameters)
        {
            var commandBuilder = new StringBuilder();

            commandBuilder.AppendFormat(CultureInfo.InvariantCulture, " -ss {0} ", parameters.ConversionOptions.Seek.GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalSeconds);

            AppendInput(commandBuilder);

            commandBuilder.AppendFormat(" -vframes {0} ", 1);

            return commandBuilder.AppendFormat(" - ").ToString();
        }

        private static string Convert(FFmpegParameters parameters)
        {
            var commandBuilder = new StringBuilder();

            // Media seek position
            if (parameters.ConversionOptions.Seek != null)
                commandBuilder.AppendFormat(CultureInfo.InvariantCulture, " -ss {0} ", parameters.ConversionOptions.Seek.Value.TotalSeconds);

            AppendInput(commandBuilder);

            // Physical media conversion (DVD etc)
            if (parameters.ConversionOptions.Target != Target.Default)
            {
                commandBuilder.Append(" -target ");
                if (parameters.ConversionOptions.TargetStandard != TargetStandard.Default)
                {
                    commandBuilder.AppendFormat(" {0}-{1} - ", parameters.ConversionOptions.TargetStandard.ToString().ToLowerInvariant(), parameters.ConversionOptions.Target.ToString().ToLowerInvariant());

                    return commandBuilder.ToString();
                }

                commandBuilder.AppendFormat("{0} - ", parameters.ConversionOptions.Target.ToString().ToLowerInvariant());

                return commandBuilder.ToString();
            }

            commandBuilder.AppendFormat(" -f {0} ", parameters.ConversionOptions.Format.ToString().ToLowerInvariant());


            if (parameters.ConversionOptions.Codec != null)
                commandBuilder.AppendFormat(" -codec:{0} ", parameters.ConversionOptions.Codec);

            // Audio bit rate
            if (parameters.ConversionOptions.AudioBitRate != null)
                commandBuilder.AppendFormat(" -ab {0}k", parameters.ConversionOptions.AudioBitRate);

            // Audio sample rate
            if (parameters.ConversionOptions.AudioSampleRate != AudioSampleRate.Default)
                commandBuilder.AppendFormat(" -ar {0} ", parameters.ConversionOptions.AudioSampleRate.ToString().Replace("Hz", ""));

            // Maximum video duration
            if (parameters.ConversionOptions.MaxVideoDuration != null)
                commandBuilder.AppendFormat(" -t {0} ", parameters.ConversionOptions.MaxVideoDuration);

            // Video bit rate
            if (parameters.ConversionOptions.VideoBitRate != null)
                commandBuilder.AppendFormat(" -b {0}k ", parameters.ConversionOptions.VideoBitRate);

            // Video frame rate
            if (parameters.ConversionOptions.VideoFps != null)
                commandBuilder.AppendFormat(" -r {0} ", parameters.ConversionOptions.VideoFps);

            // Video size / resolution
            if (parameters.ConversionOptions.VideoSize == VideoSize.Custom)
            {
                commandBuilder.AppendFormat(" -vf \"scale={0}:{1}\" ", parameters.ConversionOptions.CustomWidth ?? -2, parameters.ConversionOptions.CustomHeight ?? -2);
            }
            else if (parameters.ConversionOptions.VideoSize != VideoSize.Default)
            {
                var size = parameters.ConversionOptions.VideoSize.ToString().ToLowerInvariant();
                if (size.StartsWith("_")) size = size.Replace("_", "");
                if (size.Contains("_")) size = size.Replace("_", "-");

                commandBuilder.AppendFormat(" -s {0} ", size);
            }

            // Video aspect ratio
            if (parameters.ConversionOptions.VideoAspectRatio != VideoAspectRatio.Default)
            {
                var ratio = parameters.ConversionOptions.VideoAspectRatio.ToString();
                ratio = ratio.Substring(1);
                ratio = ratio.Replace("_", ":");

                commandBuilder.AppendFormat(" -aspect {0} ", ratio);
            }

            // Video cropping
            if (parameters.ConversionOptions.SourceCrop != null)
            {
                var crop = parameters.ConversionOptions.SourceCrop;
                commandBuilder.AppendFormat(" -filter:v \"crop={0}:{1}:{2}:{3}\" ", crop.Width, crop.Height, crop.X, crop.Y);
            }

            if (parameters.ConversionOptions.BaselineProfile) commandBuilder.Append(" -profile:v baseline ");

            return commandBuilder.AppendFormat(" - ").ToString();
        }

        private static void AppendInput(StringBuilder stringbuilder)
        {
            stringbuilder.Append(" -i - ");
        }
    }
}