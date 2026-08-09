// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.Demo.WinForms
{
    public partial class GenerateImageForm : Form
    {
        private readonly string _html;
        private readonly Bitmap _background;

        public GenerateImageForm(string html)
        {
            _html = html;
            InitializeComponent();

            Icon = DemoForm.GetIcon();

            _background = HtmlRenderingHelper.CreateImageForTransparentBackground();

            foreach (var color in GetColors())
            {
                if (color != Color.Transparent)
                    _backgroundColorTSB.Items.Add(color.Name);
            }
            _backgroundColorTSB.SelectedItem = Color.White.Name;

            foreach (var hint in Enum.GetNames(typeof(TextRenderingHint)))
            {
                _textRenderingHintTSCB.Items.Add(hint);
            }
            _textRenderingHintTSCB.SelectedItem = TextRenderingHint.AntiAlias.ToString();

            _useGdiPlusTSB.Enabled = true;
            _backgroundColorTSB.Enabled = true;
        }

        private void OnSaveToFile_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Images|*.png;*.bmp;*.jpg";
                saveDialog.FileName = "image";
                saveDialog.DefaultExt = ".png";

                var dialogResult = saveDialog.ShowDialog(this);
                if (dialogResult == DialogResult.OK)
                {
                    _pictureBox.Image.Save(saveDialog.FileName);
                }
            }
        }

        private async void OnUseGdiPlus_Click(object sender, EventArgs e)
        {
            _useGdiPlusTSB.Checked = !_useGdiPlusTSB.Checked;
            _textRenderingHintTSCB.Visible = _useGdiPlusTSB.Checked;
            _backgroundColorTSB.Visible = !_useGdiPlusTSB.Checked;
            _toolStripLabel.Text = _useGdiPlusTSB.Checked ? "Text Rendering Hint:" : "Background:";
            await GenerateImage();
        }

        private async void OnBackgroundColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            await GenerateImage();
        }

        private async void _textRenderingHintTSCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            await GenerateImage();
        }

        private async void OnGenerateImage_Click(object sender, EventArgs e)
        {
            await GenerateImage();
        }

        private async Task GenerateImage()
        {
            if (_backgroundColorTSB.SelectedItem != null && _textRenderingHintTSCB.SelectedItem != null)
            {
                var backgroundColor = Color.FromName(_backgroundColorTSB.SelectedItem.ToString());
                TextRenderingHint textRenderingHint = (TextRenderingHint)Enum.Parse(typeof(TextRenderingHint), _textRenderingHintTSCB.SelectedItem.ToString());

                Image img;
                if (_useGdiPlusTSB.Checked)
                {
                    img = await HtmlRender.RenderToImageGdiPlusAsync(_html, _pictureBox.ClientSize, textRenderingHint, null, DemoUtils.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoad);
                }
                else
                {
                    img = await HtmlRender.RenderToImageAsync(_html, _pictureBox.ClientSize, backgroundColor, null, DemoUtils.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoad);
                }
                _pictureBox.Image = img;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            using (var b = new TextureBrush(_background, WrapMode.Tile))
            {
                e.Graphics.FillRectangle(b, ClientRectangle);
            }
        }

        private static List<Color> GetColors()
        {
            const MethodAttributes attributes = MethodAttributes.Static | MethodAttributes.Public;
            PropertyInfo[] properties = typeof(Color).GetProperties();
            List<Color> list = new List<Color>();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo info = properties[i];
                if (info.PropertyType == typeof(Color))
                {
                    MethodInfo getMethod = info.GetGetMethod();
                    if ((getMethod != null) && ((getMethod.Attributes & attributes) == attributes))
                    {
                        list.Add((Color)info.GetValue(null, null));
                    }
                }
            }
            return list;
        }
    }
}