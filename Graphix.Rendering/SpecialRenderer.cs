using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;
using Color = System.Drawing.Color;

namespace Graphix.Rendering
{
    public abstract class SpecialRenderer
    {
        public RessourceManager Ressources { get; internal set; }

        public abstract void Render(RenderArgs args);
    }

    public class RectRenderer : SpecialRenderer
    {
        public override void Render(RenderArgs args)
        {
            var prot = args.Prototype;
            var context = args.Context;
            var bounds = args.Bounds;
            var fg = Ressources.GetBrush((Color)prot.Parameter["ForeColor"].Value);
            var bg = Ressources.GetBrush((Color)prot.Parameter["BackColor"].Value);
            var bw = (double)prot.Parameter["LineWidth"].Value;
            if (bg != null) context.FillRectangle(bounds, bg);
            if (fg != null) context.DrawRectangle(bounds, fg, (float)bw);
        }
    }

    public class EllipseRenderer : SpecialRenderer
    {
        public override void Render(RenderArgs args)
        {
            var prot = args.Prototype;
            var context = args.Context;
            var bounds = args.Bounds;
            var fg = Ressources.GetBrush((Color)prot.Parameter["ForeColor"].Value);
            var bg = Ressources.GetBrush((Color)prot.Parameter["BackColor"].Value);
            if (bg != null) context.FillEllipse(
                new Ellipse(
                    new Vector2(bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f), 
                    bounds.Width * 0.5f, bounds.Height * 0.5f), 
                bg);
            if (fg != null) context.DrawEllipse(
                new Ellipse(
                    new Vector2(bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f), 
                    bounds.Width * 0.5f, bounds.Height * 0.5f), 
                fg);
        }
    }

    public class LineRenderer : SpecialRenderer
    {
        public override void Render(RenderArgs args)
        {
            var prot = args.Prototype;
            var context = args.Context;
            var bounds = args.Bounds;
            var x2 = prot.Parameter.ContainsKey("X2") ? (ScreenPos)prot.Parameter["X2"].Value : new ScreenPos();
            var y2 = prot.Parameter.ContainsKey("Y2") ? (ScreenPos)prot.Parameter["Y2"].Value : new ScreenPos();
            var fg = Ressources.GetBrush((Color)prot.Parameter["ForeColor"].Value);
            if (fg != null) context.DrawLine(bounds.TopLeft, new Vector2(args.Transform(x2, false), args.Transform(y2, true)), fg);
        }
    }

    public class TextRenderer : SpecialRenderer
    {
        public override void Render(RenderArgs args)
        {
            var prot = args.Prototype;
            var context = args.Context;
            var bounds = args.Bounds;
            var text = prot.Parameter["Text"].Value as String;
            var align = prot.Parameter.ContainsKey("Align") ? (Align)prot.Parameter["Align"].Value : Align.Left;
            var valign = prot.Parameter.ContainsKey("Valign") ? (Valign)prot.Parameter["Valign"].Value : Valign.Top;
            var size = prot.Parameter.ContainsKey("FontSize") ? (ScreenPos)prot.Parameter["FontSize"].Value : new ScreenPos();
            var fg = Ressources.GetBrush((Color)prot.Parameter["ForeColor"].Value);
            //var bg = Ressources.GetBrush((string)prot.Parameter["BackColor"].Value);
            var format = Ressources.GetFormat(args.Transform(size, true), align, valign);
            if (fg != null && text != null) context.DrawText(text, format, bounds, fg);
        }
    }

    public class ImgRenderer : SpecialRenderer
    {
        public override void Render(RenderArgs args)
        {
            var url = args.Prototype.Parameter.ContainsKey("Url") ? args.Prototype.Parameter["Url"].Value as string : null;
            var bitmap = url == null ? null : Ressources.GetBitmap(url);
            if (bitmap != null)
                args.Context.DrawBitmap(bitmap, args.Bounds, 1.0f, BitmapInterpolationMode.Linear);
        }
    }

    public class AnimImgRenderer : SpecialRenderer
    {
        public override void Render(RenderArgs args)
        {
            var url = args.Prototype.Parameter.ContainsKey("Url") ? args.Prototype.Parameter["Url"].Value as string : null;
            var frametime = args.Prototype.Parameter.ContainsKey("FrameTime") ? (double)args.Prototype.Parameter["FrameTime"].Value : 40;
            var files = Ressources.GetAnimBitmapFiles(url);
            if (files.Length == 0) return;
            var frame = (int)((Environment.TickCount / frametime) % files.Length);
            var bitmap = Ressources.GetBitmap(files[frame]);
            if (bitmap != null)
                args.Context.DrawBitmap(bitmap, args.Bounds, 1.0f, BitmapInterpolationMode.Linear);
        }
    }
}
