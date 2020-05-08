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
    public partial class Login : Form
    {
        public const int ROOT = 1;  //管理员权限
        public const int NORMAL = 2;//普通用户
        public int login_resulut = 0;
        public int login_mode = 1;//1 local模式。2 remote模式
        public Login()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            cBox_login_mod.SelectedItem = cBox_login_mod.Items[0];
        }

        private void button1_login_Click(object sender, EventArgs e)
        {
            if (text_name.Text == "" || text_passwd.Text == "")
            {
                MessageBox.Show("请输入应户名或密码！");
                return;
            }
            if (cBox_login_mod.Text == "")
            {
                MessageBox.Show("请输入请设置正确的模式！");
                return;
            }
            if (text_name.Text == "admin" && text_passwd.Text == "123456")
            {
                login_resulut = ROOT;
                if (cBox_login_mod.Text == "本地模式")
                {
                    login_mode = 1;
                }
                else if (cBox_login_mod.Text == "远程模式")
                {
                    login_mode = 2;
                }
                this.Close();
            }
            else if (login_check(text_name.Text, text_passwd.Text))
            {
                login_resulut = NORMAL;
                this.Close();
            }
            else
            {
                MessageBox.Show("输入错误，请重新输入");
            }
        }

        public bool login_check(string userName,string passwd)
        {
            UserManege u1 = new UserManege(); ;
            if (u1.searchUser(userName, passwd))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void text_passwd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (text_name.Text == "admin" && text_passwd.Text == "123456")
                {
                    login_resulut = ROOT;
                    this.Close();
                }
                else if (login_check(text_name.Text, text_passwd.Text))
                {
                    login_resulut = NORMAL;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("输入错误，请重新输入");
                }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请联系管理员,增加权限");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请联系管理员，增加权限");
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            //System.Environment.Exit(0);
        }
    }
}
