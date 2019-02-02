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
    public partial class DlgSplash : Form
    {
        public DlgSplash()
        {
            InitializeComponent();
            this.TopMost = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private void DlgSplash_Load(object sender, EventArgs e)
        {

        }
    }
}
