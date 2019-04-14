using FFmpeg.AutoGen;
using FFmpeg.AutoGen.Example;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ot2Player.VideoDecoders
{
    public unsafe class VideoDecoder
    {
        /* FFMPEG related stuffs */
        private readonly AVCodecContext* _pCodecContext;
        private readonly AVFormatContext* _pFormatContext;
        private readonly int _streamIndex;
        private readonly AVFrame* _pFrame;
        private readonly AVPacket* _pPacket;


        public delegate void NewFrame();
        public event NewFrame newFrameEvent;

        private Resolution outputResolution;

        private Queue<byte[]> rawFrames;

        private Thread decodingThread;

        public VideoDecoder(Codec codec, Resolution resolution)
        {
            outputResolution = resolution;
            decodingThread = new Thread(DecodeFrames);
            Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
            Console.WriteLine("Runnung in {0}-bit mode.", Environment.Is64BitProcess ? "64" : "32");

            FFmpegBinariesHelper.RegisterFFmpegBinaries();

            Console.WriteLine($"FFmpeg version info: {ffmpeg.av_version_info()}");


        //FFMPEG initialization
        _pFormatContext = ffmpeg.avformat_alloc_context();

            var pFormatContext = _pFormatContext;
            //ffmpeg.avformat_open_input(&pFormatContext, url, null, null).ThrowExceptionIfError();

            //ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();

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

            var codecId = AVCodecID.AV_CODEC_ID_H264;
            var pCodec = ffmpeg.avcodec_find_decoder(codecId);
            _pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);
            if (pCodec == null) throw new InvalidOperationException("Unsupported codec.");

            ffmpeg.avcodec_open2(_pCodecContext, pCodec, null).ThrowExceptionIfError();

            var codecName = ffmpeg.avcodec_get_name(codecId);
            var pixelFormat = _pCodecContext->pix_fmt;

            // ALLOC FRAME AND PACKET
            _pPacket = ffmpeg.av_packet_alloc();
            _pFrame = ffmpeg.av_frame_alloc();
        }

        private void DecodeFrames()
        {

        }

        private bool DecodeFrame(out AVFrame frame)
        {
            // loop on rawFrames
            ffmpeg.av_frame_unref(_pFrame);
            int error;
            do
            {
                try
                {
                    do
                    {
                        error = ffmpeg.av_read_frame(_pFormatContext, _pPacket);
                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            frame = *_pFrame;
                            return false;
                        }

                        error.ThrowExceptionIfError();
                    } while (_pPacket->stream_index != _streamIndex);

                    ffmpeg.avcodec_send_packet(_pCodecContext, _pPacket).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(_pPacket);
                }

                error = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

            error.ThrowExceptionIfError();
            frame = *_pFrame;
            return true;
        }

        public void FeedDecoder(byte[] data)
        {
            rawFrames.Append(data);
        }

        public void SetResolution(Resolution newResolution)
        {
            outputResolution = newResolution;
        }

        public Resolution GetResolution()
        {
            return outputResolution;
        }
    }
}
