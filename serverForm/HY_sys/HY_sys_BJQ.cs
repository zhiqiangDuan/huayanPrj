using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serverForm.HY_sys
{   
    class HY_sys_BJQ: HY_crc
    {


        public byte[] m_msg;

        public byte[] cmd_bjq_cmd1;
        public byte cmd_bjq_cmd2;
        public byte cmd_bjq_cmd3;
        public byte cmd_bjq_cmd4;
        public byte cmd_bjq_cmd5;
        public HY_sys_BJQ(byte[] msg)
        {    //0    1       2       3   4       5     6     7    8      9    10       
            //0xAA  0x55  addr1  addr2  cmd1  cmd2  cmd3  cmd4  cmd5  CRCH  CRCL
            LogHelper.WriteLog(1, "[HY_sys_BJQ]", msg);
            m_msg = msg;
            if (!cmdCkeck(m_msg))// 命令解析错误直接退出
            {
                cmd_check = 0;
                return;
            }
        cmd_bjq_cmd1 = new byte[8];
        parseMsg();

    }
        public void parseMsg()
        {
            for (int i = 0; i < 8; i++)//获取cmd的每一位的值
            {
                cmd_bjq_cmd1[i] = (byte)((m_msg[4] >> i) & 0x01);

            }
            cmd_bjq_cmd2 = m_msg[5];
            cmd_bjq_cmd3 = m_msg[6];
    }
    }
}
/*
            public int CMD_BJQ_status_D0 = 0; //主电供电正常
            public int CMD_BJQ_status_D1 = 0; //备电供电正常
            public int CMD_BJQ_status_D2 = 0; //临时电供电正常
            public int CMD_BJQ_status_D3 = 0; //失电
            public int CMD_BJQ_status_D4 = 0; //消音
            public int CMD_BJQ_status_D5 = 0; //复位命令
            public int CMD_BJQ_status_D6 = 0; //通讯故障
            public int CMD_BJQ_status_D7 = 0; //备用
     */
