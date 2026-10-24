// Original code, MIT. Links and clipboard are used only after a user click.
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Despert
{
    public static class ProjectInfo
    {
        public const string Repository = "https://github.com/aaronFortuno/despert";
        public const string Portfolio = "https://aaronfortuno.github.io/portfolio/";
        public const string License = Repository + "/blob/main/LICENSE";
        public const string LightningAddress = "aptswim903@walletofsatoshi.com";
    }

    public sealed class AboutDialog : Form
    {
        private readonly Font _headingFont = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);
        private Icon _dialogIcon;

        public AboutDialog()
        {
            Text = "Despert · " + Strings.Get("About").Replace("&", "");
            Font = SystemFonts.MessageBoxFont;
            AutoScaleMode = AutoScaleMode.Font;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(22);
            using (Icon applicationIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath))
                if (applicationIcon != null) { _dialogIcon = (Icon)applicationIcon.Clone(); Icon = _dialogIcon; }

            TableLayoutPanel content = new TableLayoutPanel();
            content.AutoSize = true;
            content.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            content.ColumnCount = 1;
            content.Dock = DockStyle.Fill;
            Controls.Add(content);

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            content.Controls.Add(LabelFor("Despert " + version, true));
            content.Controls.Add(LabelFor(Strings.Get("Description"), false));
            content.Controls.Add(LabelFor(Strings.Get("Credits"), false));
            content.Controls.Add(LinkFor("Repository", ProjectInfo.Repository));
            content.Controls.Add(LinkFor("Portfolio", ProjectInfo.Portfolio));
            content.Controls.Add(LinkFor("License", ProjectInfo.License));
            Label heading = LabelFor(Strings.Get("V4VTitle"), true);
            heading.Margin = new Padding(0, 18, 0, 8);
            content.Controls.Add(heading);
            content.Controls.Add(LabelFor(Strings.Get("V4VBody"), false));

            FlowLayoutPanel contribution = new FlowLayoutPanel();
            contribution.AutoSize = true;
            contribution.WrapContents = true;
            contribution.MaximumSize = new Size(440, 0);
            contribution.Margin = new Padding(0, 6, 0, 0);
            TextBox address = new TextBox();
            address.Text = ProjectInfo.LightningAddress;
            address.ReadOnly = true;
            address.Width = 290;
            address.AccessibleName = "Lightning Network";
            address.Margin = new Padding(0, 4, 10, 0);
            contribution.Controls.Add(address);
            Button copy = new Button();
            copy.Text = Strings.Get("Copy");
            copy.AutoSize = true;
            copy.Click += delegate
            {
                try { Clipboard.SetText(ProjectInfo.LightningAddress); copy.Text = Strings.Get("Copied"); }
                catch (Exception ex) { ShowError("CopyError", ex); }
            };
            contribution.Controls.Add(copy);
            content.Controls.Add(contribution);

            Button close = new Button();
            close.Text = Strings.Get("Close");
            close.AutoSize = true;
            close.DialogResult = DialogResult.Cancel;
            close.Anchor = AnchorStyles.Right;
            close.Margin = new Padding(0, 20, 0, 0);
            content.Controls.Add(close);
            AcceptButton = close;
            CancelButton = close;
        }

        private Label LabelFor(string text, bool bold)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.MaximumSize = new Size(440, 0);
            label.Margin = new Padding(0, 0, 0, 10);
            if (bold) label.Font = _headingFont;
            return label;
        }

        private LinkLabel LinkFor(string key, string url)
        {
            LinkLabel link = new LinkLabel();
            link.Text = Strings.Get(key);
            link.AutoSize = true;
            link.MaximumSize = new Size(440, 0);
            link.Margin = new Padding(0, 0, 0, 8);
            link.LinkClicked += delegate
            {
                try
                {
                    ProcessStartInfo info = new ProcessStartInfo(url);
                    info.UseShellExecute = true;
                    using (Process process = Process.Start(info)) { }
                }
                catch (Exception ex) { ShowError("LinkError", ex); }
            };
            return link;
        }

        private void ShowError(string key, Exception ex)
        {
            MessageBox.Show(this, Strings.Format(key, ex.Message), "Despert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _headingFont.Dispose();
                if (_dialogIcon != null) { _dialogIcon.Dispose(); _dialogIcon = null; }
            }
        }
    }
}
