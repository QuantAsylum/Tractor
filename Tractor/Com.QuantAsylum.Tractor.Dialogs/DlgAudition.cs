using Com.QuantAsylum.Tractor.TestManagers;
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
    partial class DlgAudition : Form
    {
        IAudioAnalyzer Aa;

        float AuditionVolume;

        public DlgAudition(IAudioAnalyzer audioAnalyzer, float auditionVolume, string operatorInstructions)
        {
            InitializeComponent();
            label2.Text = operatorInstructions;
            AuditionVolume = auditionVolume;
            Aa = audioAnalyzer;
            label1.Text = AuditionVolume.ToString("0.00");
        }

        // Volume up
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                AuditionVolume += 0.05f;

                if (AuditionVolume > 1.0f)
                    AuditionVolume = 1.0f;

                Aa.AuditionSetVolume(AuditionVolume);
                label1.Text = AuditionVolume.ToString("0.00");
            }
            catch
            {

            }
        }

        // Volume down
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                AuditionVolume -= 0.05f;

                if (AuditionVolume < 0f)
                    AuditionVolume = 0f;

                Aa.AuditionSetVolume(AuditionVolume);
                label1.Text = AuditionVolume.ToString("0.00");
            }
            catch
            {

            }
        }
    }
}
