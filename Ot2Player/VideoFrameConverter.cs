using Ot2Player.VideoDecoder.Models;
using OT2Player.VideoDecoder.Models;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FFmpeg.AutoGen.Example
{
    public sealed unsafe class VideoFrameConverter : IDisposable
    {
        private readonly IntPtr _convertedFrameBufferPtr;
        private DecoderConfiguration configuration;
        private readonly byte_ptrArray4 _dstData;
        private readonly int_array4 _dstLinesize;
        private readonly SwsContext* _pConvertContext;

        public VideoFrameConverter(DecoderConfiguration decoderConfiguration)
        {
            configuration = decoderConfiguration;
         
                _pConvertContext = ffmpeg.sws_getContext(configuration.inputResolution.width, configuration.inputResolution.height,
                   FormatHelper.OT2ToFFmpeg(configuration.inputPixelFormat),
                configuration.outputResolution.width,
                configuration.outputResolution.height, FormatHelper.OT2ToFFmpeg(configuration.outputPixelFormat),
                ffmpeg.SWS_FAST_BILINEAR, null, null, null);
            if (_pConvertContext == null) throw new ApplicationException("Could not initialize the conversion context.");

            var convertedFrameBufferSize = ffmpeg.av_image_get_buffer_size(FormatHelper.OT2ToFFmpeg(configuration.outputPixelFormat)
                , configuration.outputResolution.width, configuration.outputResolution.height, 1);
            _convertedFrameBufferPtr = Marshal.AllocHGlobal(convertedFrameBufferSize);
            _dstData = new byte_ptrArray4();
            _dstLinesize = new int_array4();

            ffmpeg.av_image_fill_arrays(ref _dstData, ref _dstLinesize, (byte*) _convertedFrameBufferPtr, FormatHelper.OT2ToFFmpeg(configuration.outputPixelFormat)
                , configuration.outputResolution.width, configuration.outputResolution.height, 1);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_convertedFrameBufferPtr);
            ffmpeg.sws_freeContext(_pConvertContext);
        }

        public AVFrame Convert(AVFrame sourceFrame)
        {
            int res = ffmpeg.sws_scale(_pConvertContext, sourceFrame.data, sourceFrame.linesize, 0, sourceFrame.height, _dstData, _dstLinesize);
            var v = *_pConvertContext;

            var data = new byte_ptrArray8();
            data.UpdateFrom(_dstData);
            var linesize = new int_array8();
            linesize.UpdateFrom(_dstLinesize);

            return new AVFrame
            {
                data = data,
                linesize = linesize,
                width = configuration.outputResolution.width,
                height = configuration.outputResolution.height
            };
        }
    }
}