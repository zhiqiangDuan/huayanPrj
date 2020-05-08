using LBSoft.IndustrialCtrls.Buttons;
using serverForm.HY_sys;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace serverForm
{
    class HY_SYS_main_ccl : Form
    {
        List<Button> m_buttons;
        List<LBButton> m_flikers;//保存闪烁的按钮
        List<LBButton> m_but_show;//保存闪烁的指示灯
        static public int ccl_status_init = 0;
        public int but_count = 11;
        static int cur_flik_but_L = 0;
        static int cur_flik_but_R = 0;
        static int ccl_on_status_L = 0;   //表示哪个灯目前是常量状态  如果两天都可以常亮，则需要设置两个状态。
        static int ccl_on_status_R = 0;   //表示哪个灯目前是常量状态  如果两天都可以常亮，则需要设置两个状态。
        public static int ccl_cmd = 0;//保存当前的cmd状态。
        public static int is_ccl_reset = 0;  //是否收到reset命令。
        //按钮在列表中的索引
        private const int index_ccl_1_1 = 0;
        private const int index_ccl_1_2 = 1;
        private const int index_ccl_1_3 = 2;
        private const int index_ccl_1_4 = 3;
        private const int index_ccl_1_5 = 4;
        private const int index_ccl_1_6 = 5;
        private const int index_ccl_1_7 = 6;
        private const int index_ccl_1_8 = 7;
        private const int index_ccl_1_9 = 8;
        private const int index_ccl_1_10 = 9;
        private const int index_ccl_1_11 = 10;
        




        private const int index_ccl_2_1 = 11;
        private const int index_ccl_2_2 = 12;
        private const int index_ccl_2_3 = 13;
        private const int index_ccl_2_4 = 14;
        private const int index_ccl_2_5 = 15;
        private const int index_ccl_2_6 = 16;
        private const int index_ccl_2_7 = 17;
        private const int index_ccl_2_8 = 18;
        private const int index_ccl_2_9 = 19;
        private const int index_ccl_2_10 = 20;
        private const int index_ccl_2_11 = 21;

        //左收令钟
        private const int index_ccl_3_1 = 22;
        private const int index_ccl_3_2 = 23;
        private const int index_ccl_3_3 = 24;
        private const int index_ccl_3_4 = 25;
        private const int index_ccl_3_5 = 26;
        private const int index_ccl_3_6 = 27;
        private const int index_ccl_3_7 = 28;
        private const int index_ccl_3_8 = 29;
        private const int index_ccl_3_9 = 30;
        private const int index_ccl_3_10 = 31;
        private const int index_ccl_3_11 = 32;
       
        private System.ComponentModel.IContainer components;
        
        public HY_SYS_main_ccl(List<Button> buttons,List<LBButton> flikers, List<LBButton> butShow)
        {
            this.m_buttons = buttons;
            this.m_flikers = flikers;
            this.m_but_show = butShow;
        }
        //type 更新的类型，0 收到数据更新状态，1 操作按钮更新状态

        public void updateCCL(HY_sys_CCL cclMsg,int type)
        {
            int statusLR = 0;
            int is_reset = 0;

            //获取按钮的状态
            //左边状态栏
            if (type == 0)//收到数据才更新状态
            {
                m_but_show[26].ButtonColor = Color.Green;//连接状态
                m_but_show[27].ButtonColor = Color.Green;
            }
            //ShowRunStatus(cclMsg.ccl_addr1, cclMsg.ccl_cmd1);
            ccl_cmd = cclMsg.ccl_cmd1;
            is_reset = showCclCmd1(cclMsg.byte_ccl_cmd1);
            if (is_reset == 1)//有复位。不过需要注意消音与复位一起的情况到底处理哪一个
            {
                
                return;
                showResetStatus(cclMsg.ccl_cmd2);
                showResetStatus(cclMsg.ccl_cmd3);
                showResetStatus(cclMsg.ccl_cmd4);
                showResetStatus(cclMsg.ccl_cmd5);
                return;
            }
            else if (is_reset == 2)
            {
                //return;
            }
            //左右的提示互斥，只有一边会亮
            //cmd2与Cmd3肯定有一个为0.或者都为0
            if (cclMsg.ccl_cmd2 != 0)
            {
                setColor(0, cur_flik_but_R, SystemColors.ControlLight);
                showFuncStatus(cclMsg.ccl_cmd2);
                lastStatusLeft = cclMsg.ccl_cmd2;//保存获取到的cmd
                
                Console.WriteLine("[last cmd]="+ String.Format("{0:X}", lastStatusLeft));
                if (cclMsg.ccl_cmd2 >> 4 == 0x0D)
                {
                    Form1.ccl_but_status_L = 0;
                }
                
            }
            if (cclMsg.ccl_cmd3 != 0)
            {
                setColor(0, cur_flik_but_L, SystemColors.ControlLight);
                showFuncStatus(cclMsg.ccl_cmd3);
                lastStatusRight = cclMsg.ccl_cmd3;//保存获取到的cmd
            }
            showFuncStatus(cclMsg.ccl_cmd4);
            showFuncStatus(cclMsg.ccl_cmd5);
            /*if (cclMsg.ccl_cmd1 <= 0x0f)
            {
                if (cclMsg.ccl_cmd2 == 0x00 && cclMsg.ccl_cmd3 == 0x00 && cclMsg.ccl_cmd4 == 0x00)
                {
                    switch (cclMsg.ccl_cmd1)
                    {
                        case 0x05:
                            set_but_flik(24);
                            break;
                        case 0x06:
                            stop_but_flik(24);
                            m_but_show[24].ButtonColor = SystemColors.ControlLight;
                            break;
                        case 0x07:
                            set_but_flik(25);
                            break;
                        case 0x08:
                            stop_but_flik(25);
                            m_but_show[25].ButtonColor = SystemColors.ControlLight;
                            break;
                    }
                }
                 else
                 {
                        if (is_init == 0)
                        {
                            ShowRunStatus(cclMsg.ccl_addr1, cclMsg.ccl_cmd1);
                            showFuncStatus(cclMsg.ccl_cmd2);
                            showFuncStatus(cclMsg.ccl_cmd3);
                            showFuncStatus(cclMsg.ccl_cmd4);
                            showFuncStatus(cclMsg.ccl_cmd5);
                            is_init = 1;
                        }
                    }
               
                return;
            }
            else//显示运行状态
            {
                //ccl_msgType = cclMsg.ccl_cmd1 >> 4;
                showFuncStatus(cclMsg.ccl_cmd1);
            }
            */
            //获取索引

        }

        /// <summary>
        /// return 是否复位
        /// </summary>
        /// <param name="ccl_cmd1"></param>
        /// <returns></returns>
        private int showCclCmd1(byte[] ccl_cmd1)
        {
            if (ccl_cmd1[0] == 1)//复位有效
            {
                removeAllFliker();
                removeAllStatus();
                if (ccl_cmd1[2] == 1 || ccl_cmd1[3] == 1)//失电1有效
                {
                    add_but_fliker(22);
                    add_but_fliker(23);
                }
                if (ccl_cmd1[5] == 1)//误车1
                    add_but_fliker(24);
                if (ccl_cmd1[4] == 1)//误车2
                    add_but_fliker(25);
                is_ccl_reset = 1;
                return 1;
            }
            if (ccl_cmd1[1] == 1)//消音有效
            {
                if (ccl_cmd1[2] == 1 || ccl_cmd1[3] == 1)//失电1有效
                {
                    stop_but_flik(22);
                    stop_but_flik(23);
                    m_but_show[22].ButtonColor = Color.Red;
                    m_but_show[23].ButtonColor = Color.Red;
                }
                if (ccl_cmd1[5] == 1)//误车1
                {
                    stop_but_flik(24);
                    m_but_show[24].ButtonColor = Color.Red;
                }
                else
                {
                    stop_but_flik(24);
                    m_but_show[24].ButtonColor = SystemColors.ControlLight;
                }

                if (ccl_cmd1[4] == 1)//误车1
                {
                    stop_but_flik(25);
                    m_but_show[25].ButtonColor = Color.Red;
                }
                else
                {
                    stop_but_flik(25);
                    m_but_show[25].ButtonColor = SystemColors.ControlLight;
                }
                stopAllFliker();
                is_ccl_reset = 0;
                return 2;
                //todo
            }
            if (ccl_cmd1[2] == 1|| ccl_cmd1[3] == 1)//失电1有效
            {
                m_but_show[22].ButtonColor = Color.Red;
                m_but_show[23].ButtonColor = Color.Red;
            }
            else
            {
                stop_but_flik(22);
                stop_but_flik(23);
                m_but_show[22].ButtonColor = SystemColors.ControlLight;
                m_but_show[23].ButtonColor = SystemColors.ControlLight;
            }
            if (ccl_cmd1[5] == 1)//误车1
            {
                //m_but_show[24].ButtonColor = Color.Red;
                add_but_fliker(24);
            }
            else
            {
                stop_but_flik(24);
                m_but_show[24].ButtonColor = SystemColors.ControlLight;
            }
            if (ccl_cmd1[4] == 1)//误车1
            {
                add_but_fliker(25);
                //m_but_show[25].ButtonColor = Color.Red;
            }
            else
            {
                stop_but_flik(25);
                m_but_show[25].ButtonColor = SystemColors.ControlLight;
            }
            return 0;
        }

        public static int lastStatusLeft = 0;
        public static int lastStatusRight = 0;
        /// <summary>
        /// cmd 传进来cmd的值。
        /// cmdType 第几个cmd，新协议里面如果后面的命令有数据会清除前面的状态
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="cmdType"></param>
        void showFuncStatus(byte cmd)
        {
            int type = 0;
            int ccl_msgType = 0;
            int index = cmd & 0x0f;
            if (cmd == 0)//0 不显示状态
                return;
            
            ccl_msgType = cmd >> 4;

            if ((ccl_msgType == 0x02) || (ccl_msgType == 0x06) || (ccl_msgType == 0x04) || (ccl_msgType == 0x0E))
            {
                //判断左右
                index = index + 11;
                ccl_msgType = ccl_msgType - 1;
                //lastStatusRight = cmd;
                type = 2;//右边
            }
            else
            {
                type = 1;//左边
                //lastStatusLeft = cmd;
                //lastStatusLeft = cmd;
            }
            switch (ccl_msgType)
            {
                //左======================
                case 0x01://闪烁
                case 0x05:
                    set_but_flik(type,index);
                    //闪烁
                    break;

                case 0x03://熄灭
                    stop_but_flik(index);
                    setColor(0, index, SystemColors.ControlLight);
                    break;
                case 0x0D://常量
                    if (index < 11)
                    {
                       
                        ccl_on_status_L = index;
                    }

                    if (index >= 11)
                    {
                        Form1.ccl_but_status_R = 0;
                        ccl_on_status_R = index;
                    }
                        
                    stop_but_flik(index);
                    setColor(0, index, Color.Red);
                    break;
                default:
                    break;
            }
        }

        void showResetStatus(byte cmd)
        {
            if (cmd == 0)
                return;
            if ((cmd >= 0x50 && cmd <= 0x5A)|| (cmd >= 0xD0 && cmd <= 0xDA))
            {
                add_but_fliker(cmd & 0x0f);
            }
            else if ((cmd >= 0x60 && cmd <= 0x6A) || (cmd >= 0xE0 && cmd <= 0xEA))
            {
                add_but_fliker((cmd & 0x0f )+ 11);
            }
        }

        /// <summary>
        /// 
        /* 	上位机上电读取传令钟状态操作：
		0xAA 0x55 0x04 0x09 0x00 0x00 0x00 0x00 0x00 CRCH CRCL

            传令钟回令：
		0xAA 0x55 0x04 0x01(或0x02) cmd1 cmd2 cmd3 cmd4 cmd5 CRCH CRCL

        cmd1：	bit0		0——正常，1——主电失电
                 bit1		0——正常，1——备电失电
                bit2		0——正常，1——进误车
                bit3		0——正常，1——退误车
                bit4		0——前架，1——后架*/
        /// </summary>
        /// <param name="cmd1"></param>
        /// 
        void ShowRunStatus(byte addr,byte cmd1)
        {
            if (((byte)(cmd1 & 0x01) != 0) || ((byte)(cmd1 & (0x01 << 1)) != 0))
            {
                m_but_show[22].ButtonColor = Color.Red;
                m_but_show[23].ButtonColor = Color.Red;
            }
            else
            {
                m_but_show[22].ButtonColor = SystemColors.ControlLight;
                m_but_show[23].ButtonColor = SystemColors.ControlLight;
            }
            if (((byte)(cmd1 & (0x01 << 2)) != 0) || ((byte)(cmd1 & (0x01 << 3)) != 0))
            {
                if(addr == 0x01)
                {
                    m_but_show[24].ButtonColor = Color.Red;
                }
                else
                {
                    m_but_show[25].ButtonColor = Color.Red;
                }
            }
            else
            {
                if (addr == 0x01)
                {
                    m_but_show[24].ButtonColor = SystemColors.ControlLight;
                }
                else
                {
                    m_but_show[25].ButtonColor = SystemColors.ControlLight;
                }
            }
            ccl_status_init = 1;//告诉主线程。收到init命令
        }
        /// <summary>
        /// 设置按钮的颜色
        /// //type 是左还是右   先不用 后续需要区分左右
        ///index 按钮的索引
        ///status 需要显示的状态
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="color"></param>

        public void setColor(int type,int index,Color color)
        {
            //关闭其他状态
            //0
            if (index < 11)
            {
                for (int i = 0; i < 11; i++)
                {
                    if (i != index)
                    {
                        if ((i == ccl_on_status_L) && (!color.Equals(Color.Red))) //常量状态
                        {
                            continue;
                        }
                        m_but_show[i].ButtonColor = SystemColors.ControlLight;
                    }
                }
            }
            else
            {
                for (int i = 11; i < 22; i++)
                {
                    if (i != index)
                    {
                        if ((i == ccl_on_status_R) && (!color.Equals(Color.Red))) //常量状态
                        {
                            continue;
                        }
                        m_but_show[i].ButtonColor = SystemColors.ControlLight;
                    }
                }
            }
            
            
            m_but_show[index].ButtonColor = color;
        }
        public void set_but_flik(int type,int but_index)
        {
            //Console.WriteLine("R = "+cur_flik_but_R+",index = "+ but_index);
            //左右只能有一个在闪烁
            if (but_index < 11)
            {
                if (m_flikers.Contains(m_but_show[cur_flik_but_L]))//先关掉其他正咋闪烁的but
                {
                    m_flikers.Remove(m_but_show[cur_flik_but_L]);
                    m_but_show[cur_flik_but_L].ButtonColor = SystemColors.ControlLight;
                }
                m_but_show[cur_flik_but_L].ButtonColor = SystemColors.ControlLight;
            }
            if (but_index >= 11 && but_index < 22)
            {
                //Console.WriteLine("R = " + cur_flik_but_R + ",index = " + but_index);
                if (m_flikers.Contains(m_but_show[cur_flik_but_R]))//先关掉其他正咋闪烁的but
                 {
                     m_flikers.Remove(m_but_show[cur_flik_but_R]);
                    m_but_show[cur_flik_but_R].ButtonColor = SystemColors.ControlLight;
                 }
                m_but_show[cur_flik_but_R].ButtonColor = SystemColors.ControlLight;

            }
            if (!m_flikers.Contains(m_but_show[but_index]))
            {
                m_flikers.Add(m_but_show[but_index]);
            }
            
            if (but_index < 11)
            {
                cur_flik_but_L = but_index;
            }
            else if (but_index >= 11 && but_index < 22)
            {
                cur_flik_but_R = but_index;
            }
            
        }

        public void removeAllFliker()
        {
            for (int i = 0; i < 11; i++)
            {
                if (m_flikers.Contains(m_but_show[i]))//先关掉其他正咋闪烁的but
                {
                    m_flikers.Remove(m_but_show[i]);
                    m_but_show[i].ButtonColor = SystemColors.ControlLight;
                }
                if (m_flikers.Contains(m_but_show[i+11]))//先关掉其他正咋闪烁的but
                {
                    m_flikers.Remove(m_but_show[i+11]);
                    m_but_show[i+11].ButtonColor = SystemColors.ControlLight;
                }
            }
            
        }

        public void removeAllStatus()
        {
            for (int i = 0; i < 11; i++)
            {
                    m_but_show[i].ButtonColor = SystemColors.ControlLight;
                    m_but_show[i + 11].ButtonColor = SystemColors.ControlLight;
            }

        }


        public void stopAllFliker()
        {
            for (int i = 0; i < 11; i++)
            {
                if (m_flikers.Contains(m_but_show[i]))//先关掉其他正咋闪烁的but
                {
                    m_flikers.Remove(m_but_show[i]);
                    m_but_show[i].ButtonColor = Color.Red;
                }
                if (m_flikers.Contains(m_but_show[i + 11]))//先关掉其他正咋闪烁的but
                {
                    m_flikers.Remove(m_but_show[i + 11]);
                    m_but_show[i + 11].ButtonColor = Color.Red;
                }
            }
        }
        /// <summary>
        /// 停止闪烁
        /// index 指示灯的索引
        /// </summary>
        /// <param name="but_index"></param>
        public void stop_but_flik(int but_index)
        {
            //but_index = index_ccl_1_4;

            if (m_flikers.Contains(m_but_show[but_index]))
            {
                m_flikers.Remove(m_but_show[but_index]);
            }
        }
        public void add_but_fliker(int but_index)
        {
            if (!m_flikers.Contains(m_but_show[but_index]))
            {
                m_flikers.Add(m_but_show[but_index]);
            }
        }
    }
}
