using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OT2Player.VideoDecoder.Models
{
    public class Resolution
    {
        public int height;
        public int width;

        public Resolution(int height, int width)
        {
            this.height = height;
            this.width = width;
        }
    }
}
