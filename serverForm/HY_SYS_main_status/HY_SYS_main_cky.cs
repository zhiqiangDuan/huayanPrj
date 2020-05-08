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
    class HY_SYS_main_cky : Form
    {
        List<Button> m_buttons;
        List<LBButton> m_flikers;//保存闪烁的按钮
        List<LBButton> m_but_show;//保存闪烁的指示灯
        private List<Button> but_cmd_cky;
        private List<LBButton> but_fliker;
        private List<LBButton> but_show_cky;
        public static byte cky_cmd1 = 0x00;
        public static byte cky_cmd3 = 0x00;
        public HY_SYS_main_cky()
        { 
        
        }
        public HY_SYS_main_cky(List<Button> but_cmd_cky, List<LBButton> but_fliker, List<LBButton> but_show_cky)
        {
            this.but_cmd_cky = but_cmd_cky;
            this.but_fliker = but_fliker;
            this.but_show_cky = but_show_cky;
        }

        internal void updateCKY(HY_sys_CKY ckyMsg)
        {
            but_show_cky[25].ButtonColor = Color.Green//左侧按钮
;
            clearAllbut();//先全部清空
            showCmd1Status(ckyMsg);
            showCmd2Status(ckyMsg);
        }
        /// <summary>
        /// 先清空其他的所有状态
        /// </summary>
        private void clearAllbut()
        {
            for (int i = 0; i < 25; i++)
            {
                but_show_cky[i].ButtonColor = SystemColors.ControlLight;
            }
        }

        public void showCmd2Status(HY_sys_CKY ckyMsg)
        {
            switch (ckyMsg.cmd_cky_cmd2)
            {
                case 0x01://应急
                    showYJstatus(ckyMsg);
                    break;
                case 0x02://通用报警
                case 0x04://通用报警
                case 0x06:
                    showBJstatus(ckyMsg);
                    break;
                case 0x03://紧急讲话
                    showJJJHstatus(ckyMsg);
                    break;
                case 0x07://对讲
                    showDJstatus(ckyMsg);
                    break;
                case 0x08://全船
                    showQCstatus(ckyMsg);
                    break;
                    //电量 遥控台1 仓面-6个全点亮
                case 0x09:            
                    showHJYKTstatus(ckyMsg);//呼叫遥控台
                    break;
                case 0x0A:
                    showYKTstatus(ckyMsg);//全船
                    showBHJstatus(ckyMsg);//显示被呼叫
                    //处理遥控台与仓面6个
                    break;
                case 0x0B://电话控制
                    showDHstatus(ckyMsg);
                    break;
                case 0x0D://线路
                    cky_cmd3 = ckyMsg.cmd_cky_byte_cmd3;
                    showXLstatus(ckyMsg);
                    break;

                default:
                    break;
            }
        }

        private void showXLstatus(HY_sys_CKY ckyMsg)
        {
            if (ckyMsg.cmd_cky_cmd3[0] == 1)//喊话
            {
                but_show_cky[18].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd3[1] == 1)
            {
                but_show_cky[19].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd3[2] == 1)
            {
                but_show_cky[20].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd3[3] == 1)
            {
                but_show_cky[21].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd3[4] == 1)
            {
                but_show_cky[22].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd3[5] == 1)
            {
                but_show_cky[23].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd3[6] == 1)
            {
                but_show_cky[24].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd3[7] == 1)
            {
                but_show_cky[17].ButtonColor = Color.Red;
            }

        }

        private void showBHJstatus(HY_sys_CKY ckyMsg)
        {
            if (ckyMsg.cmd_cky_cmd4[0] == 1)//喊话
            {
                but_show_cky[18].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd4[1] == 1)
            {
                but_show_cky[19].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd4[2] == 1)
            {
                but_show_cky[20].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd4[3] == 1)
            {
                but_show_cky[21].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd4[4] == 1)
            {
                but_show_cky[22].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd4[5] == 1)
            {
                but_show_cky[23].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd4[6] == 1)
            {
                but_show_cky[24].ButtonColor = Color.Red;
            }
            if (ckyMsg.cmd_cky_cmd4[7] == 1)
            {
                but_show_cky[17].ButtonColor = Color.Red;
            }

        }

        private void showDHstatus(HY_sys_CKY  ckyMsg)
        {
            but_show_cky[15].ButtonColor = Color.Red;
        }

        private void showDJstatus(HY_sys_CKY ckyMsg)
        {
            but_show_cky[4].ButtonColor = Color.Red;
            for (int i = 0; i < 8; i++)
            {
                if (ckyMsg.cmd_cky_cmd3[i] == 1)
                {
                    but_show_cky[5 + i].ButtonColor = Color.Red;
                }
            }
        }
        private void showYKTstatus(HY_sys_CKY ckyMsg)
        {
            for (int i = 0; i < 8; i++)
            {
                if (ckyMsg.cmd_cky_cmd3[i] == 1)
                {
                    but_show_cky[5 + i].ButtonColor = Color.Red;
                }
            }
        }
        private void showBJstatus(HY_sys_CKY ckyMsg)
        {
            but_show_cky[3].ButtonColor = Color.Red;

            but_show_cky[19].ButtonColor = Color.Red;
            but_show_cky[20].ButtonColor = Color.Red;
            but_show_cky[21].ButtonColor = Color.Red;
            but_show_cky[22].ButtonColor = Color.Red;
            but_show_cky[23].ButtonColor = Color.Red;
            but_show_cky[24].ButtonColor = Color.Red;
        }
        private void showJJJHstatus(HY_sys_CKY ckyMsg)
        {

            but_show_cky[3].ButtonColor = Color.Red;
            but_show_cky[5].ButtonColor = Color.Red;
            but_show_cky[19].ButtonColor = Color.Red;
            but_show_cky[20].ButtonColor = Color.Red;
            but_show_cky[21].ButtonColor = Color.Red;
            but_show_cky[22].ButtonColor = Color.Red;
            but_show_cky[23].ButtonColor = Color.Red;
            but_show_cky[24].ButtonColor = Color.Red;

        }
        private void showYJstatus(HY_sys_CKY  ckyMsg)
        {
            but_show_cky[2].ButtonColor = Color.Red;
            for (int i = 0; i < 8; i++)
            {
                if (ckyMsg.cmd_cky_cmd3[i] == 1)
                {
                    but_show_cky[5 + i].ButtonColor = Color.Red;
                }
            }

            but_show_cky[19].ButtonColor = Color.Red;
            but_show_cky[20].ButtonColor = Color.Red;
            but_show_cky[21].ButtonColor = Color.Red;
            but_show_cky[22].ButtonColor = Color.Red;
            but_show_cky[23].ButtonColor = Color.Red;
            but_show_cky[24].ButtonColor = Color.Red;
        }
        //呼叫遥控台
        private void showHJYKTstatus(HY_sys_CKY ckyMsg)
        {
            for (int i = 0; i < 8; i++)
            {
                if (ckyMsg.cmd_cky_cmd3[i] == 1)
                {
                    but_show_cky[5+i].ButtonColor = Color.Red;
                }
            }

            for (int i = 0; i < 8; i++)
            {
                if (ckyMsg.cmd_cky_cmd4[i] == 1)
                {
                    but_show_cky[5 + i].ButtonColor = Color.Red;
                }
            }
        }
        private void showQCstatus(HY_sys_CKY ckyMsg)
        {
            but_show_cky[5].ButtonColor = Color.Red;//报警
            but_show_cky[19].ButtonColor = Color.Red;//其他区域全亮
            but_show_cky[20].ButtonColor = Color.Red;
            but_show_cky[21].ButtonColor = Color.Red;
            but_show_cky[22].ButtonColor = Color.Red;
            but_show_cky[23].ButtonColor = Color.Red;
            but_show_cky[24].ButtonColor = Color.Red;

        }
        /// <summary>
        /// 显示主状态。
        /// 根据收到的cmd1来解析
        /// </summary>
        /// <param name="ckyMsg"></param>
        public void showCmd1Status(HY_sys_CKY ckyMsg)
        {
            cky_cmd1 = ckyMsg.cmd_cky_byte_cmd1;
            //收到消息则电源正常
            but_show_cky[1].ButtonColor = Color.Green;//运行
            if (ckyMsg.cmd_cky_cmd1[4] == 1)//加密状态
            {
                but_show_cky[0].ButtonColor = Color.Red;
            }
            else
            {
                but_show_cky[0].ButtonColor = SystemColors.ControlLight;
            }
            if (ckyMsg.cmd_cky_cmd1[0] == 0 )
            {//如果主电否则是灭的
                but_show_cky[13].ButtonColor = SystemColors.ControlLight;
            }
            else
            {
                but_show_cky[13].ButtonColor = Color.Red;
            }
            /*if (ckyMsg.cmd_cky_cmd1[5] == 1)//通讯故障
            {
                but_show_cky[14].ButtonColor = SystemColors.ControlLight;
            }
            else
            {
                but_show_cky[14].ButtonColor = Color.Red;
            }
            */

        }
    }
}
