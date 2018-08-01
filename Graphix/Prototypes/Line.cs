using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes
{
    public class Line : RenderingBase
    {
        public ValueWrapper<ScreenPos> X2 { get; private set; }
        public ValueWrapper<ScreenPos> Y2 { get; private set; }
        

        public Line()
        {
            RenderName = GetType().FullName;
            Parameter.Add("X2", X2 = new ValueWrapper<ScreenPos>());
            Parameter.Add("Y2", Y2 = new ValueWrapper<ScreenPos>());
        }
    }
}
