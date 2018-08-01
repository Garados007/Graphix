using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes
{
    public class Rect : RenderingBase
    {
        public ValueWrapper<double> LineWidth { get; private set; }

        public Rect()
        {
            RenderName = GetType().FullName;
            Parameter.Add("LineWidth", LineWidth = new ValueWrapper<double>()
            {
                Value = 1,
                Exists = true
            });
        }
    }
}
