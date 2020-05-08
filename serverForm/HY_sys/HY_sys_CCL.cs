using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serverForm.HY_sys
{
    class HY_sys_CCL : HY_crc
    {
        public byte[] m_msg;
        public byte ccl_addr1 = 0;
        public byte ccl_cmd1 = 0;
        public byte[] byte_ccl_cmd1 = new byte[8];
        public byte ccl_cmd2 = 0;
        public byte ccl_cmd3 = 0;
        public byte ccl_cmd4 = 0;
        public byte ccl_cmd5 = 0;

        public HY_sys_CCL(byte[] msg)
        {
            //0    1       2       3   4       5     6     7    8      9    10       
            //0xAA  0x55  addr1  addr2  cmd1  cmd2  cmd3  cmd4  cmd5  CRCH  CRCL
            LogHelper.WriteLog(1, "[HY_sys_CCL]", msg);
            if (!cmdCkeck(msg))// 命令解析错误直接退出
            {
                cmd_check = 0;
                return;
            }
            m_msg = msg;

        }
        //解析命令
        public void parseMsg()
        {
            if (!cmdCkeck(m_msg))// 命令解析错误直接退出
            {
                cmd_check = 0;
                return;
            }
            ccl_addr1 = m_msg[3];
            ccl_cmd1 = m_msg[4];
            for (int i = 0; i < 8; i++)//获取cmd的每一位的值
            {
                byte_ccl_cmd1[i] = (byte)((m_msg[4] >> i) & 0x01);
            }
            ccl_cmd2 = m_msg[5];
            ccl_cmd3 = m_msg[6];
            ccl_cmd4 = m_msg[7];
            ccl_cmd5 = m_msg[8];
        }
        //封装命令
        public void makeMsg()
        {

        }
    }
}
