using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OT2Player.VideoDecoder.Models
{
    public class Frame
    {
        public byte[] rgbFrame;
        public Resolution resolution;
        public PixelFormat pixelFormat;
        public bool hasError;
        public string errorMsg;
    }
}
