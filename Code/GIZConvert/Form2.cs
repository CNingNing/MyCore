using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GIZConvert
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        public void Radio_Change(object sender,EventArgs e)
        {
            if (!((RadioButton)sender).Checked)
            {
                return;
            }
            var text = ((RadioButton)sender).Text.ToString();
            switch(text)
            {
                case "红色":
                    button1.Tag = Color.Red;
                    break;
                case "蓝色":
                    button1.Tag = Color.Blue;
                    break;
                case "黄色":
                    button1.Tag = Color.Yellow;
                    break;
                default:
                    button1.Tag = Color.Black;
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.ForeColor = (Color)button1.Tag;
        }
    }
}
