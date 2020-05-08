using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serverForm.HY_sys
{
    class HY_sys_HZB : HY_crc
    {
        public byte[] m_msg;
        public byte[] hzb_cmd1;
        public byte[] hzb_cmd2;
        public byte[] hzb_cmd3;
        public byte[] hzb_cmd4;
        public byte[] hzb_cmd5;
        public byte hzb_cmd4_byte;


        public HY_sys_HZB(byte[] msg)
        {
            //0    1       2       3   4       5     6     7    8      9    10       
            //0xAA  0x55  addr1  addr2  cmd1  cmd2  cmd3  cmd4  cmd5  CRCH  CRCL
            LogHelper.WriteLog(1, "[HY_sys_HZB]", msg);
            m_msg = msg;
            hzb_cmd1 = new byte[8];
            hzb_cmd2 = new byte[8];
            hzb_cmd3 = new byte[8];
            hzb_cmd4 = new byte[8];
            hzb_cmd5 = new byte[8];
        }
        //解析命令
        public void parseMsg()
        {
            if (!cmdCkeck(m_msg))// 命令解析错误直接退出
            {
                cmd_check = 0;
                return;
            }
            hzb_cmd4_byte = m_msg[7];
            for (int i = 0; i < 8; i++)//获取cmd的每一位的值
            {
                hzb_cmd1[i] = (byte)((m_msg[4] >> i) & 0x01);
                hzb_cmd2[i] = (byte)((m_msg[5] >> i) & 0x01);
                hzb_cmd3[i] = (byte)((m_msg[6] >> i) & 0x01);
                hzb_cmd4[i] = (byte)((m_msg[7] >> i) & 0x01);
                hzb_cmd5[i] = (byte)((m_msg[8] >> i) & 0x01);
                //Console.WriteLine(hzb_cmd1[i].ToString() + " " + hzb_cmd2[i].ToString() + " " + hzb_cmd3[i].ToString() + " " + hzb_cmd4[i].ToString() + " " + hzb_cmd5[i].ToString());
            }
        }
        //封装命令
        public void makeMsg()
        {
              
        }
    }
}
