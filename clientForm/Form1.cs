using Coldairarrow.Util.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace clientForm
{
    public partial class Form1 : Form
    {
        SocketClient client;
        public delegate void RefreshUI(object o);
        public void refreshUI(Object o)
        {
            this.label1.Text += o.ToString() + "\n";
        }
        public Form1()
        {
            InitializeComponent();
            clientInit();
        }
        public void clientInit()
        {
            //创建客户端对象，默认连接本机127.0.0.1,端口为12345
            client = new SocketClient(12345);

            //绑定当收到服务器发送的消息后的处理事件
            client.HandleRecMsg = new Action<byte[], SocketClient>((bytes, theClient) =>
            {
                string msg = Encoding.UTF8.GetString(bytes);
                this.Invoke(new RefreshUI(refreshUI), new object[] { msg });
                Console.WriteLine($"收到消息:{msg}");
            });

            //绑定向服务器发送消息后的处理事件
            client.HandleSendMsg = new Action<byte[], SocketClient>((bytes, theClient) =>
            {
                string msg = Encoding.UTF8.GetString(bytes);
                Console.WriteLine($"向服务器发送消息:{msg}");
            });

            //开始运行客户端
            client.StartClient();
        }
        void sendTest(object msg)
        {
            int count = 1000;
            byte[] testMsg = { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x11, 0x22, 0x33, 0x44, 0x55 };

            while (count-- != 0)
            {
                client.Send(testMsg);
                //client.Send(msg.ToString());
                 Thread.Sleep(1);
                //textBox1.Text = count.ToString();
            }
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(sendTest);
            //if ("" != textBox1.Text)
            {
                thread.Start(textBox1.Text);
            }
            
            //client.Send(textBox1.Text);
        }
    }
}
