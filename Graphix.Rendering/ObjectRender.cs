using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;

namespace Graphix.Rendering
{
    class ObjectRender : IDisposable
    {
        public Dictionary<string, SpecialRenderer> Renderer = new Dictionary<string, SpecialRenderer>();

        public ObjectRender(SharpRender r)
        {

        }

        public void Dispose()
        {
        }

        public void Click(Renderer source, Vector2 point, Size2 screen, Physic.ClickButton button)
        {

            foreach (var obj in source.Objects)
                if (ClickObject(source, point, obj, new RectangleF(0, 0, screen.Width, screen.Height), screen, button))
                    return;
        }

        bool ClickObject(Renderer source, Vector2 point, FlatPrototype prot, RectangleF parent, Size2 screen, Physic.ClickButton button)
        {
            var bounds = GetBounds(prot, parent, screen);
            var contains = bounds.Contains(point);
            var found = false;
            var force = prot.Parameter.ContainsKey("MouseSolid") && (bool)prot.Parameter["MouseSolid"].Value;
            if (!force || contains)
                foreach (var sub in prot.Container)
                    if (ClickObject(source, point, sub, bounds, screen, button))
                        return true;
            if (contains)
                foreach (var anim in prot.Animations)
                    foreach (var act in anim.Activations)
                        if ((act is Physic.ClickAnimation) && act.Enabled)
                        {
                            var click = (Physic.ClickAnimation)act;
                            if (click.Button.Exists && click.Button.Value != button)
                                continue;
                            source.Animation.ExecuteAnimation(anim);
                            found = true;
                        }
            return force && found;
        }

        public void Render(Renderer source, Size2 screen)
        {
            foreach (var obj in source.Objects)
                RenderObject(source, obj, new RectangleF(0, 0, screen.Width, screen.Height), screen);
        }

        void RenderObject(Renderer source, FlatPrototype prot, RectangleF parent, Size2 screen)
        {
            var bounds = GetBounds(prot, parent, screen);
            if (prot.Parameter.ContainsKey("Visible") && !(bool)prot.Parameter["Visible"].Value)
                return;
            foreach (var sub in prot.Container)
                RenderObject(source, sub, bounds, screen);
            if (Renderer.ContainsKey(prot.RenderName))
                Renderer[prot.RenderName].Render(new RenderArgs(prot, source.SharpRender.d2dContext, bounds, screen));
        }

        float Transform(ScreenPos pos, Size2F parent, Size2 screen, bool vertAxis)
        {
            switch (pos.PosType)
            {
                case PosType.Relative:
                    return (vertAxis ? parent.Height : parent.Width) * (float)pos.Value * 0.01f;
                case PosType.RelativeWidth:
                    return parent.Width * (float)pos.Value * 0.01f;
                case PosType.RelativeHeight:
                    return parent.Height * (float)pos.Value * 0.01f;
                case PosType.Screen:
                    return (vertAxis ? screen.Height : screen.Width) * (float)pos.Value * 0.01f;
                case PosType.ScreenWidth:
                    return screen.Width * (float)pos.Value * 0.01f;
                case PosType.ScreenHeight:
                    return screen.Height * (float)pos.Value * 0.01f;
                case PosType.Absolute:
                default:
                    return (float)pos.Value;
            }
        }

        RectangleF GetBounds(FlatPrototype prot, RectangleF parent, Size2 screen)
        {
            var x = prot.Parameter.ContainsKey("X") ? Transform((ScreenPos)prot.Parameter["X"].Value, parent.Size, screen, false) : 0;
            var y = prot.Parameter.ContainsKey("Y") ? Transform((ScreenPos)prot.Parameter["Y"].Value, parent.Size, screen, true) : 0;
            x += parent.X;
            y += parent.Y;
            var w = prot.Parameter.ContainsKey("Width") ? Transform((ScreenPos)prot.Parameter["Width"].Value, parent.Size, screen, false) : parent.Right - x;
            var h = prot.Parameter.ContainsKey("Height") ? Transform((ScreenPos)prot.Parameter["Height"].Value, parent.Size, screen, true) : parent.Bottom - y;
            return new RectangleF(x, y, w, h);
        }
    }
}
