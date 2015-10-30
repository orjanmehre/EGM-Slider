using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using egmtest;

namespace EgmSmallTest
{
    public partial class ControlPanel : Form
    {
        Sensor sensor;

        public ControlPanel(Sensor sensor)
        {
            this.sensor = sensor;
            sensor.Start();
            InitializeComponent();
        }

        private void trackBarHeight_ValueChanged(object sender, EventArgs e)
        {
            var trackBar = (sender as TrackBar);
            var value = trackBar.Value;
            sensor.Height = value;
            lblHeight.Text = value.ToString();
            System.Diagnostics.Debug.WriteLine(value);
        }
    }
}
