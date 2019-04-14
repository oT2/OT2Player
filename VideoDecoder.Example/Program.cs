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


            var decoder = new OT2Player.VideoDecoder.VideoDecoder(OT2Player.VideoDecoder.Models.Codec.H264, new OT2Player.VideoDecoder.Models.Resolution());
        }
    }
}
