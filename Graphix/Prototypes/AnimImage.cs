using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes
{
    public class AnimImage : Image
    {
        public ValueWrapper<double> FrameTime { get; private set; }

        public AnimImage()
        {
            RenderName = GetType().FullName;
            Parameter.Add("FrameTime", FrameTime = new ValueWrapper<double>());
        }
    }
}
