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
    public partial class DlgInput : Form
    {
        string Msg;
        public string Result = "";

        public DlgInput(string msg)
        {
            InitializeComponent();
            Msg = msg;
        }

        private void DlgInput_Load(object sender, EventArgs e)
        {
            label1.Text = Msg;
        }

        private void DlgInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            Result = textBox1.Text;
        }
    }
}
