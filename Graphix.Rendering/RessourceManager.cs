using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.WIC;
using SharpDX.Windows;
using SharpDX.Win32;
using SharpDX.IO;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;
using Bitmap = SharpDX.Direct2D1.Bitmap;

namespace Graphix.Rendering
{
    public class RessourceManager : IDisposable
    {
        RenderTarget target;

        internal void SetRenderTarget(RenderTarget target)
        {
            this.target = target;
        }

        Dictionary<System.Drawing.Color, SolidColorBrush> Brushes = new Dictionary<System.Drawing.Color, SolidColorBrush>();

        public SolidColorBrush GetBrush(System.Drawing.Color color)
        {
            if (Brushes.ContainsKey(color)) return Brushes[color];
            var b = new SolidColorBrush(target, SharpRender.c(color));
            Brushes.Add(color, b);
            return b;
        }

        Dictionary<string, TextFormat> Formats = new Dictionary<string, TextFormat>();

        public TextFormat GetFormat(double fontSize, Align a, Valign v)
        {
            var name = fontSize.ToString() + "|" + a.ToString() + "|" + v.ToString();
            if (Formats.ContainsKey(name)) return Formats[name];
            if (fontSize <= 0) fontSize = 16;
            var format = new TextFormat(new SharpDX.DirectWrite.Factory(),
                "Calibri", (float)fontSize)
            {
                TextAlignment =
                    a == Align.Left ? TextAlignment.Leading :
                    a == Align.Right ? TextAlignment.Trailing :
                    TextAlignment.Center,
                ParagraphAlignment =
                    v == Valign.Top ? ParagraphAlignment.Near :
                    v == Valign.Bottom ? ParagraphAlignment.Far :
                    ParagraphAlignment.Center
            };
            Formats[name] = format;
            return format;
        }

        Dictionary<string, Bitmap> Bitmaps = new Dictionary<string, Bitmap>();
        Dictionary<string, string> BitmapFile = new Dictionary<string, string>();

        public Bitmap GetBitmap(string file)
        {
            string full;
            System.IO.FileInfo fi = null;
            if (BitmapFile.ContainsKey(file))
                full = BitmapFile[file];
            else
            {
                fi = new System.IO.FileInfo(file);
                full = fi.FullName;
                BitmapFile.Add(file, full);
            }
            if (Bitmaps.ContainsKey(full)) return Bitmaps[full];
            if (fi == null) fi = new System.IO.FileInfo(full);
            if (fi.Exists)
            {
                using (var fac = new ImagingFactory())
                using (var fs = new NativeFileStream(full, NativeFileMode.Open, NativeFileAccess.Read))
                using (var dec = new BitmapDecoder(fac, fs, DecodeOptions.CacheOnDemand))
                using (var frame = dec.GetFrame(0))
                using (var conv = new FormatConverter(fac))
                {
                    conv.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPBGRA);
                    var bit = Bitmap.FromWicBitmap(target, conv);
                    Bitmaps[full] = bit;
                    return bit;
                }
            }
            else
            {
                Bitmaps.Add(full, null);
                return null;
            }
        }

        Dictionary<string, string[]> AnimBitmaps = new Dictionary<string, string[]>();
        string[] imgExtensions = new[] { ".jpg", ".png", ".jpeg", ".pneg", ".bmp" };

        public string[] GetAnimBitmapFiles(string folder)
        {
            if (AnimBitmaps.ContainsKey(folder)) return AnimBitmaps[folder];
            var d = new System.IO.DirectoryInfo(folder);
            if (!d.Exists) return new string[0];
            var l = new List<string>();
            foreach (var file in d.GetFiles())
                if (imgExtensions.Contains(file.Extension.ToLower()))
                    l.Add(file.FullName);
            return AnimBitmaps[folder] = l.ToArray();
        }

        public void Dispose()
        {
            foreach (var p in Brushes) p.Value.Dispose();
            foreach (var p in Formats) p.Value.Dispose();
        }
    }
}
