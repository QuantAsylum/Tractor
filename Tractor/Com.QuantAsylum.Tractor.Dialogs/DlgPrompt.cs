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

            label1.Text = instruction;

            button3.Visible = showFailButton;

            if (image != null)
            {
                this.Height = 650;
                pictureBox1.Image = image;
            }
            else
                this.Height = 300;
        }
    }
}
