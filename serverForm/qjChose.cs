using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace serverForm
{
    public partial class qjChose : Form
    {

        string fileName;

        //声明一个委托
        public delegate void DisplayUpdateDelegate(int sys, int subSys, string str);
        //声明事件
        public event DisplayUpdateDelegate ShowUpdate;

        public qjChose()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
        }
        //选择图片
        private void button1_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;
            }
            
        }
        //保存
        private void button2_Click(object sender, EventArgs e)
        {
            int index = 0;
            if (ShowUpdate != null)
            {
                index = comboBox1.SelectedIndex;
                if ((index == -1)|| fileName == null)
                {
                    MessageBox.Show("请选择指定的系统！");
                    return;
                }
                if (fileName != null)
                    ShowUpdate(index, 1, fileName);
            }
            Form1.picClick = 0;
            this.Close();
        }
        //取消
        private void button3_Click(object sender, EventArgs e)
        {
            Form1.picClick = 0;
            this.Close();
        }
    }
}
