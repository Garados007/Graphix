using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes
{
    public class Text : RenderingBase
    {
        public ValueWrapper<string> TextValue { get; private set; }

        public ValueWrapper<ScreenPos> FontSize { get; private set; }

        public ValueWrapper<Align> Align { get; private set; }

        public ValueWrapper<Valign> Valign { get; private set; }

        public Text()
        {
            RenderName = GetType().FullName;
            Parameter.Add("Text", TextValue = new ValueWrapper<string>());
            Parameter.Add("FontSize", FontSize = new ValueWrapper<ScreenPos>());
            Parameter.Add("Align", Align = new ValueWrapper<Graphix.Align>());
            Parameter.Add("Valign", Valign = new ValueWrapper<Graphix.Valign>());
        }
    }
}
