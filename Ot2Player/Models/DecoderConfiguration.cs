using OT2Player.VideoDecoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ot2Player.VideoDecoder.Models
{
    public class DecoderConfiguration
    {
        public Resolution outputResolution;
        public Resolution inputResolution;
        public PixelFormat inputPixelFormat;
        public PixelFormat outputPixelFormat;
        public Codec codec;
    }
}
