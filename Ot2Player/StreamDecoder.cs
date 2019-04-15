using FFmpeg.AutoGen;
using FFmpeg.AutoGen.Example;
using Ot2Player.VideoDecoder.Models;
using OT2Player.VideoDecoder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OT2Player.VideoDecoder
{
    public sealed unsafe class StreamDecoder : IDisposable
    {
        private readonly AVCodecContext* _pCodecContext;
        private readonly AVFormatContext* _pFormatContext;
        private readonly int _streamIndex;
        private readonly AVFrame* _pFrame;
        private readonly AVPacket* _pPacket;

        private Thread decodingThread;
        private DecoderConfiguration decoderConfiguration;

        public delegate void NewFrame(Frame frame);
        public event NewFrame newFrameEvent;


        public StreamDecoder(string url, DecoderConfiguration configuration)
        {
            FFmpegBinariesHelper.RegisterFFmpegBinaries(); //Should not be here

            this.decoderConfiguration = configuration;
            _pFormatContext = ffmpeg.avformat_alloc_context();

            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_open_input(&pFormatContext, url, null, null).ThrowExceptionIfError();

            ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();

            // find the first video stream
            AVStream* pStream = null;
            for (var i = 0; i < _pFormatContext->nb_streams; i++)
                if (_pFormatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    pStream = _pFormatContext->streams[i];
                    break;
                }

            if (pStream == null) throw new InvalidOperationException("Could not found video stream.");

            _streamIndex = pStream->index;
            _pCodecContext = pStream->codec;


            var codecId = _pCodecContext->codec_id;
            var pCodec = ffmpeg.avcodec_find_decoder(codecId);
            if (pCodec == null) throw new InvalidOperationException("Unsupported codec.");

            ffmpeg.avcodec_open2(_pCodecContext, pCodec, null).ThrowExceptionIfError();

            decoderConfiguration.codec = FormatHelper.FFmpegToOT2(codecId);
            decoderConfiguration.inputResolution = new Resolution(_pCodecContext->height, _pCodecContext->width);
            //CodecName = ffmpeg.avcodec_get_name(codecId);
            //PixelFormat = _pCodecContext->pix_fmt;
            decoderConfiguration.inputPixelFormat = FormatHelper.FFmpegToOT2(_pCodecContext->pix_fmt);

            _pPacket = ffmpeg.av_packet_alloc();
            _pFrame = ffmpeg.av_frame_alloc();
            decodingThread = new Thread(() => { while (DecodeFrame() == 0); });
        }

        public string CodecName { get; }
        public AVPixelFormat PixelFormat { get; }

        public void Dispose()
        {
            ffmpeg.av_frame_unref(_pFrame);
            ffmpeg.av_free(_pFrame);

            ffmpeg.av_packet_unref(_pPacket);
            ffmpeg.av_free(_pPacket);

            ffmpeg.avcodec_close(_pCodecContext);
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
        }

        public void Start()
        {
            decodingThread.Start();
        }

        public int DecodeFrame()
        {
            ffmpeg.av_frame_unref(_pFrame);
            var frame = new Frame();
            frame.hasError = false;
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
                            frame.hasError = true;
                            frame.errorMsg = "EOF";
                            //frame = *_pFrame;
                            return 1;
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

            var frameConverter = new VideoFrameConverter(decoderConfiguration); //TODO decoder should not be reinitialized
            var outputframe = frameConverter.Convert(*_pFrame);

            if (newFrameEvent != null)
                newFrameEvent.Invoke(new Frame
                {
                    hasError = false,
                    pixelFormat = Models.PixelFormat.RGB,
                    resolution = decoderConfiguration.outputResolution,
                    rgbFrame = GetByteArrayFromRGBFrame(&outputframe)
                });
            error.ThrowExceptionIfError();
            return 0;
        }

        // TODO Handle other type of Pixel Fmt 
        byte[] GetByteArrayFromRGBFrame(AVFrame* avFrame)
        {
            IntPtr av = (IntPtr)avFrame->data[0];
            byte[] arr = new byte[avFrame->width * avFrame->height * 3];
            Marshal.Copy(av, arr, 0, avFrame->width * avFrame->height * 3);
            return arr;
        }

        void SaveRGBFrame(AVFrame* avFrame)
        {
            var stream = new FileStream("testimg2.jpeg", FileMode.Create);
            IntPtr av = (IntPtr)avFrame->data[0];
            byte[] arr = new byte[avFrame->width * avFrame->height * 3];
            Marshal.Copy(av, arr, 0, avFrame->width * avFrame->height * 3);
            stream.Write(arr, 0, avFrame->width * avFrame->height * 3);
            //fwrite(avY, avFrame->width, 1, fDump);
            stream.Close();
        }

        void SaveAvFrame(AVFrame* avFrame)
        {
            //FILE* fDump = fopen("...", "ab");

            int pitchY = avFrame->linesize[0];
            int pitchU = avFrame->linesize[1];
            int pitchV = avFrame->linesize[2];

            IntPtr avY = (IntPtr)avFrame->data[0];
            IntPtr avU = (IntPtr)avFrame->data[1];
            IntPtr avV = (IntPtr)avFrame->data[2];

            var stream = new FileStream("testimg.jpeg", FileMode.Create);
            for (int i = 0; i < avFrame->height; i++)
            {
                byte[] arr = new byte[avFrame->width];
                Marshal.Copy(avY, arr, 0, avFrame->width);
                stream.Write(arr, 0, avFrame->width);
                //fwrite(avY, avFrame->width, 1, fDump);
                avY += pitchY;
            }

            for (Byte i = 0; i < avFrame->height / 2; i++)
            {
                byte[] arr = new byte[avFrame->width];
                Marshal.Copy(avU, arr, 0, avFrame->width);
                stream.Write(arr, 0, avFrame->width / 2);
                //fwrite(avU, avFrame->width / 2, 1, fDump);
                avU += pitchU;
            }

            for (Byte i = 0; i < avFrame->height / 2; i++)
            {
                byte[] arr = new byte[avFrame->width];
                Marshal.Copy(avV, arr, 0, avFrame->width);
                stream.Write(arr, 0, avFrame->width / 2);
                //fwrite(avV, avFrame->width / 2, 1, fDump);
                avV += pitchV;
            }
            stream.Close();
        }

        public IReadOnlyDictionary<string, string> GetContextInfo()
        {
            AVDictionaryEntry* tag = null;
            var result = new Dictionary<string, string>();
            while ((tag = ffmpeg.av_dict_get(_pFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);
                result.Add(key, value);
            }

            return result;
        }
    }
}
