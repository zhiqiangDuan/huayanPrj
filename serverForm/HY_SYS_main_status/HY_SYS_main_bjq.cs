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
    class HY_SYS_main_bjq : Form
    {
       static public int but_bjq_cy_status = 0;//船员状态
       static public int but_bjq_lk_status = 0;//旅客状态
        private List<Button> but_cmd_bjq;
        private List<LBButton> but_fliker;
        private List<LBButton> but_show_bjq;
        static int is_cmd_xy_send = 0;//是否发送过消息
        public static int bjq_status_cysd = 0;//船员手动
        public static int bjq_status_lksd = 0;//旅客手动
        public static int bjq_status_cyty = 0;//船员通用
        public static int bjq_status_lkty = 0;//旅客通用
        public static byte last_status_cmd2 = 0x00;//保存上一次收到的状态
        public static byte last_status_cmd3 = 0x00;//保存上一次收到的状态

        public HY_SYS_main_bjq(List<Button> but_cmd_bjq, List<LBButton> but_fliker, List<LBButton> but_show_bjq)
        {
            this.but_cmd_bjq = but_cmd_bjq;
            this.but_fliker = but_fliker;
            this.but_show_bjq = but_show_bjq;
        }
        /// <summary>
        /// 更新报警器状态
        /// </summary>
        /// <param name="bjqMsg"></param>
        internal void updateBJQ(HY_sys_BJQ bjqMsg)
        {
            but_show_bjq[6].ButtonColor = Color.Green;//通信状态
            showBJQ_status(bjqMsg);
            if (bjqMsg.cmd_bjq_cmd2 == 0x01)//无
            { 
            }
            if (bjqMsg.cmd_bjq_cmd2 == 0x02)//无
            { 
            
            }
            last_status_cmd2 = bjqMsg.cmd_bjq_cmd2;
            last_status_cmd3 = bjqMsg.cmd_bjq_cmd3;
            showBJQ_no_msg(bjqMsg);
            showTYBJ(bjqMsg);//显示通用报警
            showBJQ_hj(bjqMsg);
            if (bjqMsg.cmd_bjq_cmd2 == 0x04)//无
            {

            }
            showSDBJ(bjqMsg);//手动报警
            

        }
        void showBJQ_status(HY_sys_BJQ bjqMsg)
        {

            but_show_bjq[5].ButtonColor = SystemColors.ControlLight;//通信
            if (bjqMsg.cmd_bjq_cmd1[0] == 1)//主电源
            {
                but_show_bjq[0].ButtonColor = Color.Green;
            }
            else
            {
                but_show_bjq[0].ButtonColor = SystemColors.ControlLight;
            }
            //   bgqAddFliker(0);
            if (bjqMsg.cmd_bjq_cmd1[1] == 1)//应急电源
            {
                but_show_bjq[1].ButtonColor = Color.Green;

            }
            else
            {
                but_show_bjq[1].ButtonColor = SystemColors.ControlLight;
            }

            if (bjqMsg.cmd_bjq_cmd1[2] == 1)//应急电源
            {
                but_show_bjq[2].ButtonColor = Color.Green;
            }
            else
            {
                but_show_bjq[2].ButtonColor = SystemColors.ControlLight;
            }
            if (bjqMsg.cmd_bjq_cmd1[1] == 0 && bjqMsg.cmd_bjq_cmd1[2] == 0)//如果应急电源有一个为1 。则失电关闭
            {
                but_show_bjq[3].ButtonColor = SystemColors.ControlLight;
            }
            else
            {
                but_show_bjq[3].ButtonColor = Color.Red;
            }

            //bgqAddFliker(2);
            //if (bjqMsg.cmd_bjq_cmd1[3] == 1)
              //  bgqAddFliker(3);
            if (bjqMsg.cmd_bjq_cmd1[4] == 0)//如果没有消音则闪烁。消音后则停止闪烁
            {
                if (bjqMsg.cmd_bjq_cmd1[3] == 1)
                    bgqAddFliker(3);
            } //是否消音
            else
            {
                bgqRemoveFliker(3);
                but_show_bjq[3].ButtonColor = Color.Red;
            }
        }
        /// <summary>
        /// 若没有报警事件则将按钮颜色回复正常状态
        /// </summary>
        /// <param name="bjqMsg"></param>
        void showBJQ_no_msg(HY_sys_BJQ bjqMsg)
        {
            if (bjqMsg.cmd_bjq_cmd2 == 0)
            {

                but_cmd_bjq[0].Image = Properties.Resources.BJQ_cysd;
                but_cmd_bjq[1].Image = Properties.Resources.BJQ_cyty;
                but_cmd_bjq[2].Image = Properties.Resources.BJQ_lkty;
                but_cmd_bjq[3].Image = Properties.Resources.BJQ_cysd;
                //火警 常量
                but_show_bjq[4].ButtonColor = SystemColors.ControlLight;
            }
            //but_bjq_cy_status = 0;//船员状态
            //but_bjq_lk_status = 0;//旅客状态
        }
        void showBJQ_hj(HY_sys_BJQ bjqMsg) //火警
        {
            if (bjqMsg.cmd_bjq_cmd2 == 0x04)
            {
                but_show_bjq[4].ButtonColor = Color.Red;
            }

        }
        /// <summary>
        /// 通用报警
        /// </summary>
        /// <param name="bjqMsg"></param>
        void showTYBJ(HY_sys_BJQ bjqMsg)
        {
            if (bjqMsg.cmd_bjq_cmd2 == 0x03)
            {
                if (bjqMsg.cmd_bjq_cmd3 == 0x01)
                {
                    bjq_status_cyty = 1;
                    bjq_status_lkty = 0;
                    but_cmd_bjq[1].Image = Properties.Resources.BJQ_cyty_2;
                    but_cmd_bjq[2].Image = Properties.Resources.BJQ_lkty;
                    //but_cmd_bjq[1].BackColor = Color.FromArgb(150, 255, 0, 0);
                    //but_cmd_bjq[2].BackColor = Color.Transparent;
                }
                if (bjqMsg.cmd_bjq_cmd3 == 0x02)
                {
                    bjq_status_cyty = 0;
                    bjq_status_lkty = 1;
                    but_cmd_bjq[1].Image = Properties.Resources.BJQ_cyty;
                    but_cmd_bjq[2].Image = Properties.Resources.BJQ_lkty_2;
                    //but_cmd_bjq[1].BackColor = Color.Transparent;
                    //but_cmd_bjq[2].BackColor = Color.FromArgb(150, 255, 0, 0);
                }
                if (bjqMsg.cmd_bjq_cmd3 == 0x03)
                {
                    bjq_status_cyty = 1;
                    bjq_status_lkty = 1;
                    but_cmd_bjq[1].Image = Properties.Resources.BJQ_cyty_2;
                    but_cmd_bjq[2].Image = Properties.Resources.BJQ_lkty_2;
                    //but_cmd_bjq[1].BackColor = Color.FromArgb(150, 255, 0, 0);
                    //but_cmd_bjq[2].BackColor = Color.FromArgb(150, 255, 0, 0);
                }
            }
        }
        /// <summary>
        /// 手动报警状态更新
        /// </summary>
        /// <param name="bjqMsg"></param>
        void showSDBJ(HY_sys_BJQ bjqMsg)
        {

            if (bjqMsg.cmd_bjq_cmd2 == 0x05)
            {
                if (bjqMsg.cmd_bjq_cmd3 == 0x01)
                {
                    but_cmd_bjq[0].Image = Properties.Resources.BJQ_cysd_2;//Color.Yellow;
                    but_cmd_bjq[3].Image = Properties.Resources.BJQ_cysd;

                    //but_cmd_bjq[0].BackColor = Color.Yellow;
                    //but_cmd_bjq[3].BackColor = Color.Transparent;
                }
                if (bjqMsg.cmd_bjq_cmd3 == 0x02)
                {
                    but_cmd_bjq[0].Image = Properties.Resources.BJQ_cysd;//Color.Yellow;
                    but_cmd_bjq[3].Image = Properties.Resources.BJQ_cysd_2;
                    //but_cmd_bjq[0].BackColor = Color.Transparent;
                    //but_cmd_bjq[3].BackColor = Color.Yellow;
                }
                if (bjqMsg.cmd_bjq_cmd3 == 0x03)
                {
                    but_cmd_bjq[0].Image = Properties.Resources.BJQ_cysd_2;//Color.Yellow;
                    but_cmd_bjq[3].Image = Properties.Resources.BJQ_cysd_2;
                    //but_cmd_bjq[0].BackColor = Color.Yellow;
                    //but_cmd_bjq[3].BackColor = Color.Yellow;
                }
            }
        }
        /// <summary>
        /// 闪烁按钮添加到队列
        /// </summary>
        /// <param name="index"></param>
        void bgqAddFliker(int index)
        {
            if (!but_fliker.Contains(but_show_bjq[index]))
            {
                but_fliker.Add(but_show_bjq[index]);
            }
        }
        /// <summary>
        /// 将闪烁状态移除队列
        /// </summary>
        /// <param name="index"></param>
        void bgqRemoveFliker(int index)
        {
            if (but_fliker.Contains(but_show_bjq[index]))
            {
                but_fliker.Remove(but_show_bjq[index]);
            }
        }
    }
}
