[<img src="lib/ffmpeg/v4/icon.png" alt="drawing" width="24" height="24" /> FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET)
============

Original is here

[FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) provides a straightforward interface for handling media data, making tasks such as converting, slicing and editing both audio and video completely effortless.


I had adding audio channel support

` var conversionOptions = new ConversionOptions
            {
                AudioSampleRate = AudioSampleRate.Hz48000,
                AudioChanel = 1,
            };`
