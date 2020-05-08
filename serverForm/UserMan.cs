using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace serverForm
{
    public partial class 配置用户信息 : Form
    {
        UserManege u;
        public 配置用户信息()
        {
            InitializeComponent();
            u = new UserManege();
            DataTable table = new DataTable();
            table = u.getTable();
            dataGridView1.DataSource = table;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //保存所有修改的项
            //1，已有的项。修改
            //2.增加的项，增加
            List<string> userName = new List<string>();
            List<string> passwd = new List<string>();
            /*for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                userName.Add(dataGridView1[i, 0].Value as string);
                passwd.Add(dataGridView1[i, 1].Value as string);
            }*/
            
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                userName.Add(dataGridView1.Rows[i].Cells[0].Value as string);
                passwd.Add(dataGridView1.Rows[i].Cells[1].Value as string);

            }

            u.deleTable();
            u.CreateTable(1);
            for (int i = 0;i < userName.Count;i++)
            {
                if (userName[i] == "" || passwd[i] == "")
                {
                    continue;
                }
                u.addUser(userName[i], passwd[i]);
            }
            

            DataTable table = new DataTable();
            table = u.getTable();
            dataGridView1.DataSource = table;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UserManege u1 = new UserManege(); ;
            if (u1.searchUser("root", "123456"))
            {
                MessageBox.Show("OK!!");
            }
            else
            {
                MessageBox.Show("ERROR!!");
            }

        }
    }
}
