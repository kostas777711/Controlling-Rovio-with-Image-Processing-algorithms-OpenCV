using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace rovio_1
{
    public partial class Form1 : Form
    {
        private int speed;
        RovioLib.RovioController rovio = new RovioLib.RovioController("username", "password", "http://192.168.1.105");

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //RovioLib.RovioController rovio = new RovioLib.RovioController("username", "password", "http://192.168.1.105");
            MessageBox.Show(rovio.GetReport());
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Maximum = 10;
            numericUpDown1.Minimum = 1;
            speed = (int) numericUpDown1.Value;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(0, speed);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(1, speed);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(2, speed);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(4, speed);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(3, speed);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(8, speed);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(7, speed);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(10, speed);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(9, speed);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(11, speed);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(12, speed);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(13, speed);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(18, speed);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            rovio.ManualDrive(17, speed);
        }

    }
}
