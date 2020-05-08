using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LBSoft.IndustrialCtrls.Buttons;
using serverForm.HY_sys;

namespace serverForm.HY_SYS_main_status
{
    class HY_SYS_main_jb : Form
    {
        private List<Button> but_cmd_jb;
        private List<LBButton> but_fliker;
        private List<LBButton> but_show_jb;
        private List<Jb_data>[] list_jb_screen;
        private TabControl jb_tab_screen;
        private string[] jb_msg_strings = { 
        "",
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="but_cmd_jb"></param>
        /// <param name="but_fliker"></param>
        /// <param name="but_show_jb"></param>
        /// <param name="list_jb_screen">需要添加数据的链表</param>
        /// <param name="jb_tab_screen">tabpage</param>
        public HY_SYS_main_jb(List<Button> but_cmd_jb, List<LBButton> but_fliker, List<LBButton> but_show_jb, List<Jb_data>[] list_jb_screen,TabControl jb_tab_screen)
        {
            this.but_cmd_jb = but_cmd_jb;
            this.but_fliker = but_fliker;
            this.but_show_jb = but_show_jb;
            this.list_jb_screen = list_jb_screen;
            this.jb_tab_screen = jb_tab_screen;
        }

        internal void updateJB(HY_sys_JB jbMsg)
        {
            this.but_show_jb[24].ButtonColor = Color.Green;
            if (jbMsg.cmd_jb_cmd1 == 0x00)
            {
                switch (jbMsg.cmd_jb_cmd2)
                {
                    case 0x01:
                        jbStatusUpdate1(jbMsg);
                        break;
                    case 0x02:
                        jbStatusUpdate2(jbMsg);
                        break;
                    case 0x03:
                        jbStatusUpdate3(jbMsg);
                        break;
                }
            }
            else
            {
                //添加或者删除事件到链表中
                jb_add_msg_2_list(jbMsg);
            }
        }
        public void jb_add_jb_msg_1(HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(0,jbMsg) == 0)
            {
                string show = "探测器火警" + jbMsg.cmd_jb_cmd2 + "回路" + jbMsg.cmd_byte_cmd3 + "号";

                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[0].Add(data);
            }
            
        }
        public void jb_add_jb_msg_2(HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(1, jbMsg) == 0)
            {
                string show = "控制模块联动输出" + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[1].Add(data);
            }
            
        }
        public void jb_add_jb_msg_3(HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(1, jbMsg) == 0)
            {
                string show = "监视模块动作" + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[1].Add(data);
            }
        }
        public void jb_add_jb_msg_18(HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(1, jbMsg) == 0)
            {
                string show = "警报启动";// + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[1].Add(data);
            }
        }
        public void jb_add_jb_msg_21(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "应急电源220V故障";// + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }

        public void jb_add_jb_msg_23(int type,HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "屏蔽" + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }
        public void jb_add_jb_msg_4(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "探测器故障" + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }
        public void jb_add_jb_msg_6(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "主电故障";// + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }
        public void jb_add_jb_msg_8(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "备电故障";// + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }
        public void jb_add_jb_msg_d(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "回路断线故障";// + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }
        public void jb_add_jb_msg_e(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "回路短路故障";// + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }
        public void jb_add_jb_msg_f(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "回路24V电源故障";// + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }

        public void jb_add_jb_msg_11(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "模块故障" + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }
        public void jb_add_jb_msg_13(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "回路通讯故障" + jbMsg.cmd_jb_cmd2 + "号";// + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }
        public void jb_add_jb_msg_15(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "火灾显示盘通讯故障" + jbMsg.cmd_jb_cmd2 + "号";// + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }
        public void jb_add_jb_msg_1b(int type, HY_sys_JB jbMsg)
        {
            if (check_is_msg_in_list(type, jbMsg) == 0)
            {
                string show = "备用24V故障";// + jbMsg.cmd_jb_cmd2 + "号" + jbMsg.cmd_byte_cmd3 + "模块";
                Jb_data data = new Jb_data(jbMsg.cmd_jb_cmd1, show, jbMsg.cmd_jb_cmd2, jbMsg.cmd_byte_cmd3, 0);
                list_jb_screen[type].Add(data);
            }
        }
        public int check_is_msg_in_list(int type,HY_sys_JB jbMsg)
        {
            for (int i = 0; i < list_jb_screen[type].Count; i++)
            {
                if (list_jb_screen[type][i].cmd == jbMsg.cmd_jb_cmd1)//事件类型
                {
                    //判断数据
                    if (list_jb_screen[type][i].number1 == jbMsg.cmd_jb_cmd2 && list_jb_screen[type][i].number2 == jbMsg.cmd_byte_cmd3)
                    {
                        return 1;//该事件已存在，返回
                    }
                }
            }
            return 0;//不存在，可添加
        }
        public void remove_jb_msg_from_list(int type, HY_sys_JB jbMsg)
        {
            for (int i = 0; i < list_jb_screen[type].Count; i++)
            {
                if (list_jb_screen[type][i].cmd == jbMsg.cmd_jb_cmd1)//事件类型
                {
                    //判断数据
                    if (list_jb_screen[type][i].number1 == jbMsg.cmd_jb_cmd2 && list_jb_screen[type][i].number2 == jbMsg.cmd_byte_cmd3)
                    {
                        list_jb_screen[type].Remove(list_jb_screen[type][i]);
                        break;
                    }
                }
            }
        }
        private void jb_add_msg_2_list(HY_sys_JB jbMsg)
        {
            switch (jbMsg.cmd_jb_cmd1)
            {
//==========================火警
                case 0x01:
                    jb_add_jb_msg_1(jbMsg);
                    break;
//==========================联动
                case 0x02:
                    jb_add_jb_msg_2(jbMsg);
                    break;
                case 0x03:
                    jb_add_jb_msg_3(jbMsg);
                    break;
                case 0x17:
                    jbMsg.cmd_jb_cmd1 = 0x02;
                    remove_jb_msg_from_list(1, jbMsg);
                    break;
                case 0x18:
                    jb_add_jb_msg_18(jbMsg);
                    break;
                case 0x19:
                    jbMsg.cmd_jb_cmd1 = 0x18;
                    remove_jb_msg_from_list(1, jbMsg);
                    break;
                case 0x1A:
                    jbMsg.cmd_jb_cmd1 = 0x03;
                    remove_jb_msg_from_list(1, jbMsg);
                    break;

                //====================故障====================
                case 0x04:
                    jb_add_jb_msg_4(2,jbMsg);
                    break;
                case 0x05:
                    jbMsg.cmd_jb_cmd1 = 0x04;
                    remove_jb_msg_from_list(2, jbMsg);
                    break;
                case 0x06:
                    jb_add_jb_msg_6(2, jbMsg);
                    break;
                case 0x07:
                    jbMsg.cmd_jb_cmd1 = 0x06;
                    remove_jb_msg_from_list(2, jbMsg);
                    break;
                case 0x08:
                    jb_add_jb_msg_8(2, jbMsg);
                    break;
                case 0x09:
                    jbMsg.cmd_jb_cmd1 = 0x08;
                    remove_jb_msg_from_list(2, jbMsg);
                    break;
                case 0x0d:
                    jb_add_jb_msg_d(2, jbMsg);
                    break;
                case 0x0e:
                    jb_add_jb_msg_e(2, jbMsg);
                    break;
                case 0x0f:
                    jb_add_jb_msg_f(2, jbMsg);
                    break;
                case 0x10:
                    jbMsg.cmd_jb_cmd1 = 0x0f;
                    remove_jb_msg_from_list(2, jbMsg);
                    break;
                case 0x11:
                    jb_add_jb_msg_11(2, jbMsg);
                    break;
                case 0x12:
                    jbMsg.cmd_jb_cmd1 = 0x11;
                    remove_jb_msg_from_list(2, jbMsg);
                    break;
                case 0x13:
                    jb_add_jb_msg_13(2, jbMsg);
                    break;
                case 0x14:
                    jbMsg.cmd_jb_cmd1 = 0x13;
                    remove_jb_msg_from_list(2, jbMsg);
                    break;
                case 0x15:
                    jb_add_jb_msg_15(2, jbMsg);
                    break;
                case 0x16:
                    jbMsg.cmd_jb_cmd1 = 0x15;
                    remove_jb_msg_from_list(2, jbMsg);
                    break;
                case 0x1b:
                    jb_add_jb_msg_1b(2, jbMsg);
                    break;
                case 0x1c:
                    jbMsg.cmd_jb_cmd1 = 0x1b;
                    remove_jb_msg_from_list(2, jbMsg);
                    break;
                case 0x21:
                    jb_add_jb_msg_21(2,jbMsg);
                    break;
                case 0x22:
                    jbMsg.cmd_jb_cmd1 = 0x21;
                    remove_jb_msg_from_list(2, jbMsg);
                    break;
                //=================屏蔽==============
                case 0x23:
                    jb_add_jb_msg_23(3,jbMsg);
                    break;
                case 0x24:
                    jbMsg.cmd_jb_cmd1 = 0x23;
                    remove_jb_msg_from_list(3, jbMsg);
                    break;
                case 0x81:
                    clear_jb_list();
                    break;
                case 0x82:
                    //消音处理
                    break;
                default:
                    break;
            }
        }

        public void jbStatusUpdate1(HY_sys_JB jbMsg)
        {

            jb_tab_screen.TabPages[0].Text = "火警"+jbMsg.cmd_byte_cmd3.ToString();
            jb_tab_screen.TabPages[1].Text = "联动"+jbMsg.cmd_byte_cmd4.ToString();
            jb_tab_screen.TabPages[3].Text = "屏蔽" + jbMsg.cmd_byte_cmd5.ToString();
        }
        public void jbStatusUpdate2(HY_sys_JB jbMsg)
        {
            int num = 0;
            num = (int)(jbMsg.cmd_byte_cmd3 << 8 | jbMsg.cmd_byte_cmd4);
            
            jb_tab_screen.TabPages[2].Text = "故障" + num;
        }
        public void clear_jb_list()//可能存在问题
        {
            for (int i = 0; i < 4; i++)
            {
                list_jb_screen[i] = null;
            }
        }
        public void jbStatusUpdate3(HY_sys_JB jbMsg)
        {
            for (int i = 0; i < 8; i++)
            {
                if (jbMsg.cmd_jb_cmd3[i] == 0)
                {
                    but_show_jb[7-i].ButtonColor = SystemColors.ControlLight;
                }
                else
                {
                    but_show_jb[7-i].ButtonColor = Color.Red;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (jbMsg.cmd_jb_cmd4[i] == 0)
                {
                    but_show_jb[7 - i + 8].ButtonColor = SystemColors.ControlLight;
                }
                else
                {
                    but_show_jb[7 - i + 8].ButtonColor = Color.Red;
                }
                if(but_show_jb[13].ButtonColor == Color.Red)
                    but_show_jb[23].ButtonColor = Color.Green;//操作的颜色与状态保持一致
                else
                    but_show_jb[23].ButtonColor = Color.Orange;//操作的颜色与状态保持一致
            }


            for (int i = 1; i < 8; i++)
            {
                if (jbMsg.cmd_jb_cmd5[i] == 0)
                {
                    but_show_jb[7 - i + 16].ButtonColor = SystemColors.ControlLight;
                }
                else
                {
                    but_show_jb[7 - i + 16].ButtonColor = Color.Red;
                }
            }
        }
    }
}
