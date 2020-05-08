using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serverForm.HY_sys
{
    /// <summary>
    /// 命令处理类型。
    /// 1.处理从设备获取到的命令。解析成相应的类类型，并从UI刷新
    /// 2.合成数据。根据界面的操作。合成协议命令。并发送给设备系统
    /// </summary>
    class HY_cmd_handle :HY_crc
    {
        public delegate void updateUIcallBack(int sysType,Object obj);
        /// <summary>
        ///来自系统的命令的解析 ，将协议命令解析成命令
        ///arg 来自设备的协议命令，数组
        /// </summary>
        public void hy_cmd_parse(byte[] msg, updateUIcallBack updateUI)
        {
            if (msg == null)
            {
                LogHelper.WriteLog(2,"hy_cmd_parse error!!!");
                return;
            }

            Object objSys = new Object();

            switch (msg[2])
            {
                case HY_CMD_SYS_CKY:
                    HY_sys_CKY sysCKY = new HY_sys_CKY(msg);
                    sysCKY.parseMsg();
                    objSys = sysCKY;
                    break;
                case HY_CMD_SYS_BJQ:
                    HY_sys_BJQ sysBJQ = new HY_sys_BJQ(msg);
                    objSys = sysBJQ;
                    break;
                case HY_CMD_SYS_JB:
                    HY_sys_JB sysJB = new HY_sys_JB(msg);
                    sysJB.parseMsg();
                    objSys = sysJB;
                    break;
                case HY_CMD_SYS_CCL:
                    HY_sys_CCL sysCCL = new HY_sys_CCL(msg);
                    sysCCL.parseMsg();
                    objSys = sysCCL;
                    break;
                case HY_CMD_SYS_HZB:
                    HY_sys_HZB sysHZB = new HY_sys_HZB(msg);
                    sysHZB.parseMsg();
                    objSys = sysHZB;
                    break;
                default:
                    //log wrong data
                    break;
            }
            if (updateUI == null)
            {
                LogHelper.WriteLog(2, "updateUI error!!!!!!!");
                return;
            }
            updateUI(msg[2], objSys);
            //跳转界面！！！！
        }

        /// <summary>
        /// 将16进制的字符串数组转为byte数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// 根据上层的控制指令，合成协议命令
        /// </summary>
        public void hy_cmd_dump(byte a)
        {

        }
        public void hy_cmd_dump(int a)
        {

        }
    }
}
