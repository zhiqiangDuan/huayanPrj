using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serverForm.HY_sys
{
    class HY_sys_JB : HY_crc
    {
        public byte[] m_msg;
        public byte cmd_jb_cmd1;
        public byte cmd_jb_cmd2;
        public byte cmd_byte_cmd3;
        public byte cmd_byte_cmd4;
        public byte cmd_byte_cmd5;
        public byte[] cmd_jb_cmd3;
        public byte[] cmd_jb_cmd4;
        public byte[] cmd_jb_cmd5;
        public HY_sys_JB(byte[] msg)
        {
            //0    1       2       3   4       5     6     7    8      9    10       
            //0xAA  0x55  addr1  addr2  cmd1  cmd2  cmd3  cmd4  cmd5  CRCH  CRCL
            LogHelper.WriteLog(1, "[HY_sys_JB]", msg);
            m_msg = msg;
            cmd_jb_cmd3 = new byte[11];
            cmd_jb_cmd5 = new byte[11];
            cmd_jb_cmd4 = new byte[11];

    }
        public void parseMsg()
        {
            cmd_jb_cmd1 = m_msg[4];
            cmd_jb_cmd2 = m_msg[5];
            cmd_byte_cmd3 = m_msg[6];
            cmd_byte_cmd4 = m_msg[7];
            cmd_byte_cmd5 = m_msg[8];
            for (int i = 0; i < 8; i++)//获取cmd的每一位的值
            {
                cmd_jb_cmd3[i] = (byte)((m_msg[6] >> i) & 0x01);
                cmd_jb_cmd4[i] = (byte)((m_msg[7] >> i) & 0x01);
                cmd_jb_cmd5[i] = (byte)((m_msg[8] >> i) & 0x01);
            }
        }
    }
}
