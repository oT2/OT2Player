using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OT2Player.VideoDecoder.Models
{
    public enum Codec
    {
        UNKNOWN,
        H264,
        H265 //UNSUPPORTED BY FFMPEG.AUTOGEN FOR NOW
    }

    public enum PixelFormat
    {
        UNKNOWN,
        YUV420,
        RGB
    }

    public static class FormatHelper
    {
        public static Codec FFmpegToOT2(FFmpeg.AutoGen.AVCodecID ffmpegId)
        {
            if (ffmpegId == FFmpeg.AutoGen.AVCodecID.AV_CODEC_ID_H264)
                return Codec.H264;
            return Codec.UNKNOWN;
        }

        public static PixelFormat FFmpegToOT2(FFmpeg.AutoGen.AVPixelFormat ffmpegId)
        {
            return PixelFormat.YUV420;
        }

        public static FFmpeg.AutoGen.AVCodecID OT2ToFFmpeg(Codec ot2Id)
        {
            if (ot2Id == Codec.H264)
                return FFmpeg.AutoGen.AVCodecID.AV_CODEC_ID_H264;
            return FFmpeg.AutoGen.AVCodecID.AV_CODEC_ID_H264;
        }

        public static FFmpeg.AutoGen.AVPixelFormat OT2ToFFmpeg(PixelFormat ot2Id)
        {
            if (ot2Id == PixelFormat.YUV420)
                return FFmpeg.AutoGen.AVPixelFormat.AV_PIX_FMT_YUV420P;
            return FFmpeg.AutoGen.AVPixelFormat.AV_PIX_FMT_RGB24;
        }
    }

}
