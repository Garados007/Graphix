using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes
{
    public class Image : DisplayBase
    {
        public ValueWrapper<string> Url { get; private set; }

        public Image()
        {
            RenderName = GetType().FullName;
            Parameter.Add("Url", Url = new ValueWrapper<string>());
        }
    }
}
