using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serverForm.HY_sys
{
    class HY_sys_CKY :HY_crc
    {
        public byte[] m_msg;//保存该条消息
        public byte cky_addr2 = 0;
        public byte[] cmd_cky_cmd1;
        public byte cmd_cky_cmd2 = 0;
        public byte[] cmd_cky_cmd3;
        public byte[] cmd_cky_cmd4;
        public byte[] cmd_cky_cmd5;

        public byte cmd_cky_byte_cmd1;
        public byte cmd_cky_byte_cmd3;
        public byte cmd_cky_byte_cmd4;
        public HY_sys_CKY(byte[] msg)
        {
            //0    1       2       3   4       5     6     7    8      9    10       
            //0xAA  0x55  addr1  addr2  cmd1  cmd2  cmd3  cmd4  cmd5  CRCH  CRCL
            LogHelper.WriteLog(1, "[HY_sys_CKY]", msg);
            m_msg = msg;
            cmd_cky_cmd1 = new byte[11];
            cmd_cky_cmd3 = new byte[11];
            cmd_cky_cmd4 = new byte[11];
            cmd_cky_cmd5 = new byte[11];
        }


        public void parseMsg()
        {
            if (!cmdCkeck(m_msg))// 命令解析错误直接退出
            {
                cmd_check = 0;
                return;
            }
            cky_addr2 = m_msg[3];
            cmd_cky_byte_cmd1 = m_msg[4];
            cmd_cky_cmd2 = m_msg[5];
            cmd_cky_byte_cmd3 = m_msg[6];
            cmd_cky_byte_cmd4 = m_msg[7];
            for (int i = 0; i < 8; i++)//获取cmd的每一位的值
            {
                cmd_cky_cmd1[i] = (byte)((m_msg[4] >> i) & 0x01);
                cmd_cky_cmd3[i] = (byte)((m_msg[6] >> i) & 0x01);
                cmd_cky_cmd4[i] = (byte)((m_msg[7] >> i) & 0x01);
                cmd_cky_cmd5[i] = (byte)((m_msg[8] >> i) & 0x01);
            }
        }
    }
}
