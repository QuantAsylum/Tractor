using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Com.QuantAsylum.Tractor.Dialogs
{
    public partial class DlgPrompt : Form
    {
        public DlgPrompt(string instruction, bool showFailButton, Bitmap image)
        {
            InitializeComponent();

            // Not sure why this isn't sticking. This size is specified in designer, but
            // it's not the size that we get at runtime unless the size is specifically
            // set.
            Size = new Size(552, 689);

            label1.Text = instruction;

            button3.Visible = showFailButton;

            if (image != null)
            {
                pictureBox1.Image = image;
            }
            else
            {
                Height = Height - pictureBox1.Height;
            }
        }
    }
}
