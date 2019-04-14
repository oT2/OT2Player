using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoDecoder.Example
{
    class Program
    {
        static void Main(string[] args)
        {


            var decoder = new Ot2Player.VideoDecoders.VideoDecoder(Ot2Player.VideoDecoders.Codec.H264, new Ot2Player.VideoDecoders.Resolution());
        }
    }
}
