// Original vector artwork and ICO packer. MIT. Uses Windows libraries only.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

internal static class BuildIcons
{
    private static readonly int[] Sizes = { 12, 16, 20, 24, 32, 40, 48, 64, 128, 256 };

    private static Bitmap DrawCup(int size, bool active, bool dark = false, bool universal = false)
    {
        using (Bitmap large = new Bitmap(size * 4, size * 4, PixelFormat.Format32bppArgb))
        {
            using (Graphics g = Graphics.FromImage(large))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.ScaleTransform(size * 4f / 24f, size * 4f / 24f);
                using (GraphicsPath cup = new GraphicsPath())
                using (GraphicsPath handle = new GraphicsPath())
                using (GraphicsPath steam = new GraphicsPath())
                {
                    // Flat side view. Same body in both states; only steam changes.
                    cup.AddLine(2.5f, 11, 17.5f, 11);
                    cup.AddLine(17.5f, 11, 17.5f, 17);
                    cup.AddBezier(17.5f, 17, 17.5f, 21.5f, 2.5f, 21.5f, 2.5f, 17);
                    cup.CloseFigure();
                    handle.AddBezier(17.5f, 12, 24, 10.5f, 24, 18, 17.5f, 17.5f);
                    if (active)
                    {
                        steam.AddBezier(7, 8, 3.5f, 6, 10.5f, 4, 7, 1.5f);
                        steam.StartFigure();
                        steam.AddBezier(13, 8, 9.5f, 6, 16.5f, 4, 13, 1.5f);
                    }
                    if (universal)
                    {
                        using (Pen halo = new Pen(Color.White, 3.8f))
                        {
                            halo.StartCap = LineCap.Round;
                            halo.EndCap = LineCap.Round;
                            halo.LineJoin = LineJoin.Round;
                            g.DrawPath(halo, handle); g.DrawPath(halo, cup);
                            if (active) g.DrawPath(halo, steam);
                        }
                    }
                    using (Pen outline = new Pen(dark ? Color.White : Color.Black, 2.1f))
                    {
                        outline.StartCap = LineCap.Round;
                        outline.EndCap = LineCap.Round;
                        outline.LineJoin = LineJoin.Round;
                        g.DrawPath(outline, handle); g.DrawPath(outline, cup);
                        if (active) g.DrawPath(outline, steam);
                    }
                }
            }
            Bitmap result = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(large, new Rectangle(0, 0, size, size));
            }
            return result;
        }
    }

    private static void WriteIcon(string file, bool active, bool dark = false, bool universal = false)
    {
        List<byte[]> images = new List<byte[]>();
        foreach (int size in Sizes)
            using (Bitmap bitmap = DrawCup(size, active, dark, universal))
            using (MemoryStream stream = new MemoryStream())
            {
                // Legacy System.Drawing.Icon misreads small PNG-only frames.
                // Use classic BGRA DIB+AND-mask below 256, PNG only at 256.
                if (size == 256)
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    images.Add(stream.ToArray());
                }
                else images.Add(EncodeDib(bitmap));
            }
        using (BinaryWriter output = new BinaryWriter(File.Create(file)))
        {
            output.Write((ushort)0); output.Write((ushort)1); output.Write((ushort)Sizes.Length);
            int offset = 6 + 16 * Sizes.Length;
            for (int i = 0; i < Sizes.Length; i++)
            {
                output.Write((byte)(Sizes[i] == 256 ? 0 : Sizes[i]));
                output.Write((byte)(Sizes[i] == 256 ? 0 : Sizes[i]));
                output.Write((byte)0); output.Write((byte)0);
                output.Write((ushort)1); output.Write((ushort)32);
                output.Write(images[i].Length); output.Write(offset);
                offset += images[i].Length;
            }
            foreach (byte[] image in images) output.Write(image);
        }
    }

    private static byte[] EncodeDib(Bitmap bitmap)
    {
        int size = bitmap.Width;
        int maskStride = ((size + 31) / 32) * 4;
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(40); writer.Write(size); writer.Write(size * 2);
            writer.Write((ushort)1); writer.Write((ushort)32);
            writer.Write(0); writer.Write(size * size * 4 + maskStride * size);
            writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0);
            for (int y = size - 1; y >= 0; y--)
                for (int x = 0; x < size; x++)
                {
                    Color p = bitmap.GetPixel(x, y);
                    writer.Write(p.B); writer.Write(p.G); writer.Write(p.R); writer.Write(p.A);
                }
            for (int y = size - 1; y >= 0; y--)
            {
                byte[] mask = new byte[maskStride];
                for (int x = 0; x < size; x++)
                    if (bitmap.GetPixel(x, y).A == 0) mask[x / 8] |= (byte)(0x80 >> (x % 8));
                writer.Write(mask);
            }
            writer.Flush();
            return stream.ToArray();
        }
    }

    private static void Preview(string file)
    {
        using (Bitmap preview = new Bitmap(740, 260))
        using (Graphics g = Graphics.FromImage(preview))
        using (Font font = new Font("Segoe UI", 12))
        {
            g.Clear(Color.FromArgb(247, 245, 240));
            using (Brush background = new SolidBrush(Color.FromArgb(30, 30, 30)))
                g.FillRectangle(background, 370, 0, 370, 260);
            for (int theme = 0; theme < 2; theme++)
            {
                int left = 18 + 370 * theme;
                Brush text = theme == 0 ? Brushes.Black : Brushes.White;
                g.DrawString("Desactivat", font, text, left, 12);
                g.DrawString("Activat", font, text, left + 174, 12);
                for (int state = 0; state < 2; state++)
                {
                    using (Bitmap big = DrawCup(128, state == 1, theme == 1)) g.DrawImageUnscaled(big, left + state * 174, 42);
                    int x = left + state * 174;
                    foreach (int size in new int[] { 12, 16, 20, 24 })
                    {
                        using (Bitmap icon = DrawCup(size, state == 1, theme == 1)) g.DrawImageUnscaled(icon, x, 195);
                        x += size + 5;
                    }
                }
            }
            preview.Save(file, ImageFormat.Png);
        }
    }

    private static int Main(string[] args)
    {
        if (args.Length != 1) return 1;
        Directory.CreateDirectory(args[0]);
        WriteIcon(Path.Combine(args[0], "inactive-light.ico"), false);
        WriteIcon(Path.Combine(args[0], "active-light.ico"), true);
        WriteIcon(Path.Combine(args[0], "inactive-dark.ico"), false, true);
        WriteIcon(Path.Combine(args[0], "active-dark.ico"), true, true);
        WriteIcon(Path.Combine(args[0], "app.ico"), true, false, true);
        Preview(Path.Combine(args[0], "preview.png"));
        return 0;
    }
}
