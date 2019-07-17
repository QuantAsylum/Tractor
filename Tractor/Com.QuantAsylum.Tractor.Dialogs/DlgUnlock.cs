using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tractor.Com.QuantAsylum.Tractor.Dialogs
{
    internal partial class DlgUnlock : Form
    {
        public string Password = "";

        public DlgUnlock()
        {
            InitializeComponent();
        }

        // OK button
        private void Button1_Click(object sender, EventArgs e)
        {
            Password = textBox1.Text.Trim();
            Close();
        }

        // Cancel button
        private void Button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
