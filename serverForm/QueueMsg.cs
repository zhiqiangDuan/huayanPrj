using Coldairarrow.Util.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serverForm
{
    class QueueMsg
    {
        public SocketConnection client;
        public byte[] msg;
        public int queueLength;
    }
    class SysPicInfo
    {

        public int sys;//具体代表那个系统
        public System.Windows.Forms.PictureBox pb;//代表系统的图片
        public string path;//图片的path
    }
}
