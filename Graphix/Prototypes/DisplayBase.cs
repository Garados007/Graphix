using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes
{
    public class DisplayBase : PrototypeBase
    {
        public ValueWrapper<ScreenPos> X { get; private set; }

        public ValueWrapper<ScreenPos> Y { get; private set; }

        public ValueWrapper<ScreenPos> Width { get; private set; }

        public ValueWrapper<ScreenPos> Height { get; private set; }

        public ValueWrapper<bool> Visible { get; private set; }

        public ValueWrapper<bool> MouseSolid { get; private set; }

        public DisplayBase()
        {
            RenderName = GetType().FullName;
            Parameter.Add("X", X = new ValueWrapper<ScreenPos>());
            Parameter.Add("Y", Y = new ValueWrapper<ScreenPos>());
            Parameter.Add("Width", Width = new ValueWrapper<ScreenPos>());
            Parameter.Add("Height", Height = new ValueWrapper<ScreenPos>());
            Parameter.Add("Visible", Visible = new ValueWrapper<bool>());
            Parameter.Add("MouseSolid", MouseSolid = new ValueWrapper<bool>());
            Visible.Value = true;
        }
    }


}
