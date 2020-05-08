using LBSoft.IndustrialCtrls.Buttons;
using serverForm.HY_sys;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace serverForm.HY_SYS_main_status
{
    class HY_SYS_main_hzb : Form
    {
        List<Button> m_buttons;
        List<LBButton> m_flikers;//保存闪烁的按钮
        List<LBButton> m_but_show;//保存闪烁的指示灯
        public static int hzb_sys_gz = 0;//系统故障的状态
        public static int hzb_sys_is_xy = 0; //是否消音
        public HY_SYS_main_hzb(List<Button> buttons, List<LBButton> flikers, List<LBButton> butShow)
        {
            this.m_buttons = buttons;
            this.m_flikers = flikers;
            this.m_but_show = butShow;
        }

        public void updateHZB(HY_sys_HZB hzbMsg)
        {
            m_but_show[3].ButtonColor = SystemColors.ControlLight;//收到信息说明通讯正常
            m_but_show[10].ButtonColor = Color.Green;//左侧通信等收到信息说明通讯正常
            if (hzbMsg.hzb_cmd1[0] == 1)//失电
            {
                m_but_show[0].ButtonColor = Color.Green;
                m_but_show[4].ButtonColor = Color.Gold;
            }
            else
            {
                m_but_show[0].ButtonColor = SystemColors.ControlLight;
                m_but_show[4].ButtonColor = SystemColors.ControlLight;
            }
            if (hzbMsg.hzb_cmd1[1] == 1)
            {
                m_but_show[1].ButtonColor = Color.Green;
            }
            else
            {
                m_but_show[1].ButtonColor = SystemColors.ControlLight;
            }
            if (hzbMsg.hzb_cmd1[2] == 1)
            {
                m_but_show[2].ButtonColor = Color.Green;
            }
            else
            {
                m_but_show[2].ButtonColor = SystemColors.ControlLight;
            }
            if (hzbMsg.hzb_cmd1[7] == 1)//系统故障
            {
                if (hzb_sys_is_xy == 0)
                {
                    hzb_sys_gz = 1;
                    hzbAddFliker(5);
                    //m_but_show[5].ButtonColor = Color.Yellow;
                }

            }
            else
            {
                hzb_sys_gz = 0;
                hzbRemoveFliker(5);
                m_but_show[5].ButtonColor = SystemColors.ControlLight;
                hzb_sys_is_xy = 0;
            }
            if (hzbMsg.hzb_cmd1[6] == 1)//消音
            {
                hzbRemoveFliker(5);
                m_but_show[5].ButtonColor = SystemColors.ControlLight;
                if (hzb_sys_gz == 1)
                {
                    ;
                    m_but_show[5].ButtonColor = Color.Gold;
                }
                hzb_sys_is_xy = 1;
            }
            //todo加上闪烁逻辑。
            if (hzbMsg.hzb_cmd4[2] == 1)//启动光报警
            {
                Form1.hy_System_alarm[Form1.SYS_HZB] = 1;
                hzbAddFliker(6);
            }
            if (hzbMsg.hzb_cmd4[3] == 1)
            {
                hzbRemoveFliker(6);
                m_but_show[6].ButtonColor = Color.Red;
                hzbAddFliker(7);
            }
            if (hzbMsg.hzb_cmd4[4] == 1)
            {
                hzbRemoveFliker(6);
                hzbRemoveFliker(7);
                m_but_show[6].ButtonColor = Color.Red;
                m_but_show[7].ButtonColor = Color.Red;
                hzbAddFliker(8);
            }
            if (hzbMsg.hzb_cmd4[5] == 1)
            {
                hzbRemoveFliker(6);
                hzbRemoveFliker(7);
                hzbRemoveFliker(8);
                m_but_show[6].ButtonColor = Color.Red;
                m_but_show[7].ButtonColor = Color.Red;
                m_but_show[8].ButtonColor = Color.Red;
                hzbAddFliker(9);
            }
            if (hzbMsg.hzb_cmd4_byte == 0x00)//全0.要清空
            {
                Form1.hy_System_alarm[Form1.SYS_HZB] = 0;
                hzbRemoveFliker(6);
                hzbRemoveFliker(7);
                hzbRemoveFliker(8);
                hzbRemoveFliker(9);
                m_but_show[6].ButtonColor = SystemColors.ControlLight;
                m_but_show[7].ButtonColor = SystemColors.ControlLight;
                m_but_show[8].ButtonColor = SystemColors.ControlLight;
                m_but_show[9].ButtonColor = SystemColors.ControlLight;
            }

        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // HY_SYS_main_hzb
            // 
            this.ClientSize = new System.Drawing.Size(236, 273);
            this.Name = "HY_SYS_main_hzb";
            this.ResumeLayout(false);

        }

        void hzbAddFliker(int index)
        {
            if (!m_flikers.Contains(m_but_show[index]))
            {
                m_flikers.Add(m_but_show[index]);
            }
        }
        /// <summary>
        /// 将闪烁状态移除队列
        /// </summary>
        /// <param name="index"></param>
        void hzbRemoveFliker(int index)
        {
            if (m_flikers.Contains(m_but_show[index]))
            {
                m_flikers.Remove(m_but_show[index]);
            }
        }
    }
}
