using FFmpeg.AutoGen;
using FFmpeg.AutoGen.Example;
using Ot2Player.VideoDecoder.Models;
using OT2Player.VideoDecoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OT2Player.VideoDecoder
{
    public unsafe class VideoDecoder
    {
        /* FFMPEG related stuffs */
        private readonly AVCodecContext* _pCodecContext;
        private readonly AVFormatContext* _pFormatContext;
        private readonly int _streamIndex;
        private readonly AVFrame* frame;
        private readonly AVPacket* packet;


        public delegate void NewFrame(Frame frame);
        public event NewFrame newFrameEvent;

        //private Resolution outputResolution;

        private Queue<byte[]> rawData;

        private Thread decodingThread;

        private DecoderConfiguration configuration;

        public VideoDecoder(DecoderConfiguration configuration)
        {
            FFmpegBinariesHelper.RegisterFFmpegBinaries();
            //outputResolution = resolution;
            Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
            Console.WriteLine("Runnung in {0}-bit mode.", Environment.Is64BitProcess ? "64" : "32");


            Console.WriteLine($"FFmpeg version info: {ffmpeg.av_version_info()}");


            //FFMPEG initialization
            _pFormatContext = ffmpeg.avformat_alloc_context();

            var pFormatContext = _pFormatContext;
            //ffmpeg.avformat_open_input(&pFormatContext, url, null, null).ThrowExceptionIfError();

            ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();

            // find the first video stream
            //AVStream* pStream = null;
            //for (var i = 0; i < _pFormatContext->nb_streams; i++)
            //    if (_pFormatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            //    {
            //        pStream = _pFormatContext->streams[i];
            //        break;
            //}

            //if (pStream == null) throw new InvalidOperationException("Could not found video stream.");

            // GET DECODER FOR STREAM
            //_streamIndex = pStream->index;
            //_pCodecContext = pStream->codec;

            //var codecId = _pCodecContext->codec_id;

            var codecId = FormatHelper.OT2ToFFmpeg(configuration.codec);
            var pCodec = ffmpeg.avcodec_find_decoder(codecId);
            _pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);
            if (pCodec == null) throw new InvalidOperationException("Unsupported codec.");

            ffmpeg.avcodec_open2(_pCodecContext, pCodec, null).ThrowExceptionIfError();

            var codecName = ffmpeg.avcodec_get_name(codecId);
            var pixelFormat = _pCodecContext->pix_fmt;

            // ALLOC FRAME AND PACKET
            packet = ffmpeg.av_packet_alloc();
            frame = ffmpeg.av_frame_alloc();
            decodingThread = new Thread(DecodeFrames);
        }

        public void StartDecoding()
        {

        }

        private void DecodeFrames()
        {
            while (true)
            {
                if (rawData.Count > 0)
                    DecodeFrame(rawData.Dequeue());
            }

        }

        private bool DecodeFrame(byte[] data)
        {
            ffmpeg.av_frame_unref(frame);
            int error;
            do
            {
                try
                {
                    //do
                    //{
                    //    error = ffmpeg.av_read_frame(_pFormatContext, _pPacket);
                    //    if (error == ffmpeg.AVERROR_EOF)
                    //    {
                    //        frame = *_pFrame;
                    //        return false;
                    //    }

                    //    error.ThrowExceptionIfError();
                    //} while (_pPacket->stream_index != _streamIndex);

                    ffmpeg.av_new_packet(packet, data.Length);
                    Marshal.Copy(data, 0, (IntPtr)packet->data, data.Length);
                    ffmpeg.avcodec_send_packet(_pCodecContext, packet).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(packet);
                }

                error = ffmpeg.avcodec_receive_frame(_pCodecContext, frame);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

            error.ThrowExceptionIfError();
            var ot2Frame = new Frame
            {
                hasError = error == 0 ? false : true,
                pixelFormat = configuration.outputPixelFormat,
                resolution = configuration.outputResolution,
                rgbFrame = null // TODO !!!
            };
            newFrameEvent?.Invoke(ot2Frame);
            return true;
        }

        public void FeedDecoder(byte[] data)
        {
            rawData.Append(data);
        }
    }
}
