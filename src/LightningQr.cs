// Original code, MIT. Draws the author's embedded QR with integer pixel modules.
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Despert
{
    public sealed class LightningQr : Control
    {
        private readonly string[] _rows = new string[45];

        public LightningQr()
        {
            Size = new Size(180, 180);
            BackColor = Color.White;
            TabStop = false;
            AccessibleRole = AccessibleRole.Graphic;
            AccessibleName = Strings.Get("QrCode");
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            using (Stream source = Assembly.GetExecutingAssembly().GetManifestResourceStream("Despert.Lightning.modules"))
            {
                if (source == null) throw new InvalidOperationException("Missing embedded Lightning QR.");
                using (StreamReader reader = new StreamReader(source))
                {
                    for (int y = 0; y < _rows.Length; y++)
                    {
                        _rows[y] = reader.ReadLine();
                        if (_rows[y] == null || _rows[y].Length != 45 || _rows[y].Trim('0', '1').Length != 0)
                            throw new InvalidOperationException("Invalid embedded Lightning QR.");
                    }
                    if (reader.ReadLine() != null) throw new InvalidOperationException("Unexpected Lightning QR data.");
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(Color.White);
            int module = Math.Min(ClientSize.Width, ClientSize.Height) / 45;
            if (module == 0) return;
            int left = (ClientSize.Width - module * 45) / 2;
            int top = (ClientSize.Height - module * 45) / 2;
            for (int y = 0; y < 45; y++)
                for (int x = 0; x < 45; x++)
                    if (_rows[y][x] == '1')
                        e.Graphics.FillRectangle(Brushes.Black, left + x * module, top + y * module, module, module);
        }
    }
}
