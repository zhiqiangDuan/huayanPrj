using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serverForm
{
    /// <summary>
    /// 华雁通信协议
    /// </summary>
    class HY_cmd
    {

        public const byte HY_CMD_HEAD_1 = 0xAA; //帧头1
        public const byte HY_CMD_HEAD_2 = 0x55;//帧头2

        public const byte HY_CMD_SYS_CKY = 0X01;//1	CKY系列船用公共广播系统
        public const byte HY_CMD_SYS_BJQ = 0X02;//2	BJQ通用紧急报警系统
        public const byte HY_CMD_SYS_JB = 0X03;//3	JB系列火灾报警系统
        public const byte HY_CMD_SYS_CCL = 0X04;//4	CCL应急主机传令钟系统
        public const byte HY_CMD_SYS_HZB = 0X05;//5	HZB驾驶室航行值班报警系统
        public const byte HY_CMD_SYS_SG  = 0X06;//6	SG警报指示器系统
        public const byte HY_CMD_SYS_MCL = 0X07;//7	MCL船用主机传令钟系统
        public const byte HY_CMD_SYS_CHJ = 0X08;//8	CHJ轮机员安全报警系统
        public const byte HY_CMD_SYS_YJB = 0X09;//9	YJB病员呼叫系统
        public const byte HY_CMD_SYS_LJB = 0X0A;//10	LJB冷库呼叫系统
        public const byte HY_CMD_SYS_WDQ = 0X0B;//11	WDQ/YHC号笛/电子号笛控制系统
        public const byte HY_CMD_SYS_HSJ = 0X0C;//12	HSJ视觉信息显示系统
        public const byte HY_CMD_SYS_HSM = 0X0D;//13	HSM水密门指示系统
        public const byte HY_CMD_SYS_FHM = 0X0E;//14	FHM防火门指示系统
        public const byte HY_CMD_SYS_BJ  = 0X0F;//15	BJ机舱监测报警系统
        public const byte HY_CMD_SYS_CCTV = 0X10;//16	CCTV视频监控系统
        public const byte HY_CMD_SYS_HTA = 0X11;//17	HTA储藏室高温报警控制系统
        public const byte HY_CMD_SYS_HT  = 0X12;//18	HT防盗报警控制系统
        public const byte HY_CMD_SYS_IP  = 0X13;//19	IP 程控交换机系统

        public int cmd_check = 0; // 命令的正确性

















        public byte[] HY_cmd_buf;//一条命令


        //解析命令
        public void HY_parse_cmd()
        {


        }
//合成命令
    }
}
