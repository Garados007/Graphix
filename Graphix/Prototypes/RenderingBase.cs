using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Graphix.Prototypes
{
    public class RenderingBase : DisplayBase
    {
        public ValueWrapper<Color> ForeColor { get; private set; }

        public ValueWrapper<Color> BackColor { get; private set; }

        public RenderingBase()
        {
            RenderName = GetType().FullName;
            Parameter.Add("ForeColor", ForeColor = new ValueWrapper<Color>());
            Parameter.Add("BackColor", BackColor = new ValueWrapper<Color>());
        }
    }
}
