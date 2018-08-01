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
    class WaitingRenderer : IDisposable
    {
        TextFormat textFormat;
        Brush foregroundBrush, barBrush;
        System.Diagnostics.Stopwatch watch;

        public WaitingRenderer(SharpRender r)
        {
            foregroundBrush = new SolidColorBrush(r.d2dContext, Color.White);
            barBrush = new SolidColorBrush(r.d2dContext, Color.Green);
            textFormat = new TextFormat(r.wrFactory, "Calibri", 18)
            {
                TextAlignment = TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center
            };
            watch = new System.Diagnostics.Stopwatch();
            watch.Start();
        }

        public void Dispose()
        {
            watch.Stop();
            textFormat.Dispose();
            foregroundBrush.Dispose();
            barBrush.Dispose();
        }

        public void Render(DeviceContext context, Size2 screen)
        {
            var r1 = new RectangleF(screen.Width / 2 - 200, screen.Height / 2 - 75, 400, 150);
            var r2 = new RectangleF(screen.Width / 2 - 200, screen.Height / 2 - 75, 400, 75);
            context.DrawRectangle(r1, foregroundBrush);
            context.DrawText("Loading Assets", textFormat, r2, foregroundBrush);
            var t = watch.ElapsedMilliseconds % 1000;
            var p = t * 0.5f - 140;
            var l = Math.Max(0, p);
            var r = Math.Min(360, p + 140);
            var r3 = new RectangleF(screen.Width / 2 - 180 + l, screen.Height / 2 + 20, r - l, 20);
            context.FillRectangle(r3, barBrush);
        }
    }
}
