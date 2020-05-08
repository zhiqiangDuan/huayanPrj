using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serverForm
{
    class LogHelper
    {
        //public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");
        //public static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("logerror");
        public static void WriteLog(int level,string info)
        {
            /*string strLevel = parseLevel(level);
            if (loginfo.IsInfoEnabled)
            {

                loginfo.Info(strLevel + info);
            }*/
        }

        public static void WriteLog(int level, byte[] msg)
        {
            /*string strLevel = parseLevel(level);
            if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(strLevel + byteArrayToString(msg));
            }
            */


        }

        public static void WriteLog(int level,string tag, byte[] msg)
        {
            /*string strLevel = parseLevel(level);
            if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(strLevel + tag + byteArrayToString(msg));
            }
            */
        }
        public static void WriteLog( byte[] msg)
        {
            /* if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(byteArrayToString(msg));
            }
            */


        }

        public static void WriteLog(string info, Exception ex)
        {
            /*if (logerror.IsErrorEnabled)
            {
                logerror.Error(info, ex);
            }
            */
        }
        public static string parseLevel(int level)
        {
            string strLevel = "[info]";
           /* switch (level)
            {
                case 1:
                    strLevel = "[info]";
                    break;
                case 2:
                    strLevel = "[error]";
                    break;
                default:
                    strLevel = "[info]";
                    break;
            }
            */
            return strLevel;
        }

        public static string byteArrayToString(byte[] msg)
        {
            string charMsg = "[msg][";
           /* foreach (byte bitMsg in msg)
            {
                charMsg += Convert.ToString(bitMsg, 16) + " ";
            }
            charMsg += "]"; */
            return charMsg;
        }
    }
}
