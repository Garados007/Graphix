using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;

namespace Graphix.Rendering
{
    public class RenderArgs
    {
        public FlatPrototype Prototype { get; private set; }

        public DeviceContext Context { get; private set; }

        public RectangleF Bounds { get; private set; }

        public Size2 Screen { get; private set; }

        public RenderArgs(FlatPrototype prot, DeviceContext context, RectangleF bounds, Size2 screen)
        {
            Prototype = prot;
            Context = context;
            Bounds = bounds;
            Screen = screen;
        }

        public float Transform(ScreenPos pos, bool vertAxis)
        {
            switch (pos.PosType)
            {
                case PosType.Relative:
                    return (vertAxis ? Bounds.Height : Bounds.Width) * (float)pos.Value * 0.01f;
                case PosType.RelativeWidth:
                    return Bounds.Width * (float)pos.Value * 0.01f;
                case PosType.RelativeHeight:
                    return Bounds.Height * (float)pos.Value * 0.01f;
                case PosType.Screen:
                    return (vertAxis ? Screen.Height : Screen.Width) * (float)pos.Value * 0.01f;
                case PosType.ScreenWidth:
                    return Screen.Width * (float)pos.Value * 0.01f;
                case PosType.ScreenHeight:
                    return Screen.Height * (float)pos.Value * 0.01f;
                case PosType.Absolute:
                default:
                    return (float)pos.Value;
            }
        }

    }
}
