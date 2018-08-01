using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Physic
{
    public class AnimationRuntimeData
    {
        public double CurrentTime { get; set; }

        public double LastTileTime { get; set; }

        public double StartTime { get; set; }

        public AnimationRuntime Runtime { get; set; }

        public bool CanAnimate { get; set; }
    }
}
