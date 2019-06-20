﻿using System;
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
    public partial class DlgPassFail : Form
    {
        public DlgPassFail(string text, bool pass, string operatorMessage = "")
        {
            InitializeComponent();
            label1.Text = text;

            label2.Text = operatorMessage;

            if (pass)
            {
                this.BackColor = Color.Green;
            }
            else
            {
                this.BackColor = Color.Red;
            }
        }
    }
}
