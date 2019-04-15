using OT2Player.VideoDecoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OT2Player.VideoDecoder.Example
{
    class Program
    {
        static void Main(string[] args)
        {
        

        
            var url = "http://www.quirksmode.org/html5/videos/big_buck_bunny.mp4";
            //var decoder = new OT2Player.VideoDecoder.VideoDecoder(OT2Player.VideoDecoder.Models.Codec.H264, new OT2Player.VideoDecoder.Models.Resolution());
            var decoder = new StreamDecoder(url, new Ot2Player.VideoDecoder.Models.DecoderConfiguration { outputPixelFormat = Models.PixelFormat.RGB, outputResolution = new Models.Resolution(1080, 1920)});
            decoder.newFrameEvent += NewFrame;
            decoder.Start();
            while (true)
                ;
        }

        static void NewFrame(Frame frame)
        {
            Console.WriteLine("new frame: " + frame.rgbFrame.Length);
        }
    }
}
