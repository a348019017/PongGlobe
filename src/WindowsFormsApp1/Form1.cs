using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {         
            InitializeComponent();
            this.Shown += Form1_Shown;
            //this.WindowState=
        }
        UserControl1 control;

        private void Form1_Shown(object sender, EventArgs e)
        {
            //control.Run();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            control = new UserControl1();
            control.Dock = DockStyle.Fill;
            //control.Margin = new Padding(10);
            this.Controls.Add(control);      
            
        }
    }
}
