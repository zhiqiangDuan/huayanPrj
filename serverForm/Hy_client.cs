using Coldairarrow.Util.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace serverForm
{
    class Hy_client : HY_crc
    {

        private SocketClient client;
        private int port = 0;//5004;
        int isServerCon = 0;// 服务器是否连接成功
        static public Queue<QueueMsg> qMsg;
        public Queue<QueueMsg> qMsgDebug;
        QueueMsg queue;
        QueueMsg queueDebug;
        int tempFlag = 0;
        byte[] msgBuf = new byte[110];
        public ReaderWriterLockSlim RWLock_msgQueue { get; } = new ReaderWriterLockSlim();
        byte[] lastTemp = new byte[11];//上一条消息。与当前对比
        public int write_to_debug_list = 0;
        public int sendCount = 0;//bjq是否正在发送。一次只能发送一次
        public byte[] send_data_buf = new byte[11];

        int bufLength = 0;//存放到buf中数据的位置
        public static int hzb_msg_recv = 0;// 是否收到hzb的消息
        public static int ccl_msg_recv = 0;// 是否收到ccl的消息

        public string deadLine = "2020-04-01";
        public Hy_client(int port)
        {
            this.port = port;   
        }
        public void clientInit()
        {
            //创建客户端对象，默认连接本机127.0.0.1,端口为12345
            client = new SocketClient(port);

            //绑定当收到服务器发送的消息后的处理事件
            client.HandleRecMsg = new Action<byte[], SocketClient>((bytes, theClient) =>
            {
                if (bufLength + bytes.Length > 110)//收到的数据超过buf，将数据清空
                {
                    bufLength = 0;
                    Array.Clear(msgBuf, 0, msgBuf.Length);
                    Console.WriteLine("recv 22 error data!!!");
                    return;
                }
                Array.Copy(bytes, 0, msgBuf, bufLength, bytes.Length);
                bufLength += bytes.Length;
                if (bufLength < 11)
                    return;
                checkMsg();
                printMsg(bytes);
                //recv msg
            });

            //绑定向服务器发送消息后的处理事件
            client.HandleSendMsg = new Action<byte[], SocketClient>((bytes, theClient) =>
            {

                //向服务器发送数据
            });
            client.HandleClientStarted = new Action<SocketClient>((theClient) =>
            {
                //服务器已连接。可以接收数据了。
            });
            //开始运行客户端
            client.StartClient();
        }
        int recv_count = 0;
        public void checkMsg()
        {
            while (bufLength >= 11)
            {
                if (bufLength < 11)//buf中的数据小于11.等待下一帧数据
                    return;
                int headLocation = findtheHead(msgBuf);
                if (headLocation != -1)//找到数据头
                {
                    if (bufLength - headLocation < 11)//找到头，单数数据不完整。将buf中的数据全部放到buf的头中 
                    {
                        bufMove(headLocation);
                        return;
                    }
                    else
                    {
                        byte[] msg = getOneMsg(headLocation);
                        HY_crc crc = new HY_crc();
                        if (crc.cmdCkeck(msg))
                        {
                            bufMove(headLocation + 11);
                            //printMsg("[OKMsg]", msg);
                            //recv_count++;


                            addMsgToQueue(msg);
                            Console.WriteLine("port = "+port);
                            if (port == 5010)//hzb
                            {

                                sendDataAfter20(msg);//收到20后发送数据
                            }
                            else if (port == 5009)//ccl
                                sendDataAfter0C(msg);
                            //recv_count++;

                        }
                        else
                        {
                            bufMove(headLocation + 2);
                        }
                    }
                }
                else
                {
                    bufMove(headLocation + bufLength);

                }
            }
            return;

        }
        /// <summary>
        /// 从buf中取出一条有用信息。
        /// </summary>
        public byte[] getOneMsg(int head)
        {
            byte[] currectMsg = new byte[11];
            Array.Copy(msgBuf, head, currectMsg, 0, 11);
            return currectMsg;
        }
        /// <summary>
        /// 左移，head end 数据的位置
        /// </summary>
        /// <param name="head"></param>
        /// <param name="end"></param>
        public void bufMove(int head)//left 
        {
            int tempLength = bufLength - head;
            if (tempLength < 0) return;//数据错误
            byte[] temp = new byte[tempLength];
            Array.Copy(msgBuf, head, temp, 0, tempLength);
            Array.Clear(msgBuf, 0, msgBuf.Length);
            Array.Copy(temp, 0, msgBuf, 0, tempLength);
            bufLength = tempLength;//设置buf的end位置
        }
        /// <summary>
        /// 查找msg的头AA55
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int findtheHead(byte[] msg)
        {
            int flag = -1;
            for (int i = 0; i < msg.Length - 1; i++)
            {
                if (msg[i] == 0xAA && msg[i + 1] == 0x55)
                {
                    flag = i;
                    break;
                }
            }
            return flag;
        }

        static int isSending = 0;
        static byte[] sendBuf = new byte[11];
        public void sendDataThread()
        {
            
        }
        //CCL 系统 收到0C后才能将数据发送出去。
        private void sendDataAfter0C(byte[] readBuf)
        {
            if (readBuf[2] == 0x04)//CCL 
            {
                if (readBuf[3] == 0x0C)//收到0C
                {
                    if (sendCount-- > 0)
                    {
                        sendData(send_data_buf);
                    }


                }
            }
        }
        private void sendDataAfter20(byte[] readBuf)
        {

            //HZB的发送要在收到20后去发送
            if (readBuf[2] == 0x05)
            {
                if (readBuf[3] == 0x20)
                {
                    if (sendCount-- > 0)
                    {
                        Console.WriteLine("???????????");
                        sendData(send_data_buf);
                    }
                }
            }
        }
        public void sendData(byte[] msg)
        {

            byte[] sendMsg = getCRC(msg);
            //LogHelper.WriteLog(1, "[send]", msg);
            //if (!(sendMsg[2] == 0x04 && sendMsg[3] == 0x0C && sendMsg[4] == 0x0))
            {
                printMsg("[send]", sendMsg);
                //serialPort.Write(msg, 0, 11);
                client.Send(msg);
            }

        }
        public void sendData(int type, byte[] msg)
        {
            sendCount = 3;
            Console.WriteLine("send = "+ sendCount);
            Array.Copy(msg, 0, send_data_buf, 0, 11);
        }
        public void initQueue()
        {
            //if (qMsg != null)
            {
                qMsg = new Queue<QueueMsg>();
                qMsgDebug = new Queue<QueueMsg>();
            }
        }
        public QueueMsg getQueueMsg()
        {
            if (qMsg.Count > 0)
            {

                RWLock_msgQueue.EnterWriteLock();
                queue = qMsg.Dequeue();
                RWLock_msgQueue.ExitWriteLock();

            }
            else
            {
                return null;
            }
            return queue;
        }
        public QueueMsg getQueueMsgDebug()
        {
            if (qMsgDebug.Count > 0)
            {
                RWLock_msgQueue.EnterWriteLock();
                queue = qMsgDebug.Dequeue();
                RWLock_msgQueue.ExitWriteLock();
            }
            else
            {
                return null;
            }
            return queue;
        }
        public void queueMsgClear()
        {
            if (qMsgDebug.Count > 0)
            {
                RWLock_msgQueue.EnterWriteLock();
                qMsgDebug.Clear();
                RWLock_msgQueue.ExitWriteLock();
            }
        }

        int samemsgCount = 0;
        public void addMsgToQueue(byte[] msg)
        {
            byte[] temp = new byte[11];
            QueueMsg qMsgClass = new QueueMsg();
            //解决粘包问题。
            //目前测试发现最多是多条命令一起传回来。不会存在不定长的命令
            Buffer.BlockCopy(msg, 0, temp, 0, 11);
            if (lastTemp.Equals(temp))
                return;
            //printMsg(temp);
            /*byte[] checkMsg = checkMsgOK(temp);
             if (checkMsg != null)
             {
                 qMsgClass.msg = checkMsg;
             }
             else
             {
                 return;
             }*/
            qMsgClass.msg = msg;
            if (!cmdCheckCRC(msg))//CRC验证失败。数据错误。丢掉
            {
                LogHelper.WriteLog(2, "wrong msg,error crc!!!");
                return;
            }

            if (checkHZBLastMsg(lastTemp, msg))//HZB的上一条与现在一样，跳过不处理
            {
                Form1.hzb_msg_recv = 1;
                return;

            }
            if (msg[2] == 0x04)
            {


                Form1.ccl_msg_recv = 1;
                if (msg[3] != 0x0C)
                {
                    //复位信息会连续发送。如果有复位信息，则将复位的命令放行。
                    //后面的信息置0就行
                    if ((byte)(msg[4] & 0x01) == 0)
                    {
                        return;
                    }
                    
                }
                printMsg("[OKMsg]", msg);
            }
            LogHelper.WriteLog(1, "[ok data]", temp);

            RWLock_msgQueue.EnterWriteLock();

            qMsg.Enqueue(qMsgClass);
            if (write_to_debug_list == 1)
            {

                qMsgDebug.Enqueue(qMsgClass);
            }
            RWLock_msgQueue.ExitWriteLock();
            Buffer.BlockCopy(temp, 0, lastTemp, 0, 11);

        }
        public bool checkHZBLastMsg(byte[] last, byte[] cur)
        {
            if (last[2] == 0x05 && cur[2] == 0x05)
            {
                //printMsg(cur);
                if (last[4] == cur[4] && last[5] == cur[5] && last[6] == cur[6] && last[7] == cur[7]
                   && last[8] == cur[8])
                {
                    return true;
                }
            }
            return false;
        }


        public byte[] checkMsgOK(byte[] msg)
        {
            if (msg == null)
            {
                return null;
            }
            byte[] msgHead = { 0xAA, 0x55 };
            //将收到的数据拷贝到buf
            /*if (tempFlag >= 11)
            {//已经收到一条不包含AA55的数据，前面的数据丢弃
                tempFlag = 0;
                Array.Clear(msgBuf, 0, msgBuf.Length);
            }*/
            Array.Copy(msg, 0, msgBuf, tempFlag, msg.Length);

            int headLocation = findtheHead(msgBuf);
            if (headLocation != -1)//找到数据头
            {
                if (headLocation != 0)// 如果数据不在buf的头。说明数据不完整，需要再收一条
                {
                    byte[] msgTemp = new byte[11 - headLocation];
                    Array.Copy(msgBuf, headLocation, msgTemp, 0, 11 - headLocation);
                    Array.Copy(msgTemp, 0, msgBuf, 0, 11 - headLocation);
                    tempFlag = 11 - headLocation;
                    return null;
                }
                else//
                {
                    //收到有效数据取出有效数
                    byte[] currectMsg = new byte[11];
                    Array.Copy(msgBuf, 0, currectMsg, 0, 11);
                    return currectMsg;

                }

            }

            return null;

        }
        public void clearAllMsg()
        {
            //serialPort.DiscardInBuffer();
            RWLock_msgQueue.EnterWriteLock();
            if (qMsg != null)
            {
                qMsg.Clear();
            }
            RWLock_msgQueue.ExitWriteLock();
            Array.Clear(msgBuf, 0, msgBuf.Length);
            Array.Clear(lastTemp, 0, lastTemp.Length);
            Array.Clear(send_data_buf, 0, send_data_buf.Length);
            sendCount = 0;
            bufLength = 0;//存放到buf中数据的位置
        }
        public void printMsg(string head, byte[] msg)
        {
            string charMsg = head;
            foreach (byte bitMsg in msg)
            {
                charMsg += Convert.ToString(bitMsg, 16) + " ";
            }
            WriteLog(charMsg);
            Console.WriteLine(charMsg);
            // if (serialPort.PortName == "COM4")
            /*  {
                  using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"CclLog.txt", true))
                  {
                      file.WriteLine(charMsg);
                  }
              }*/ 

        }

        public void printMsg(int length, byte[] msg)
        {
            string charMsg = "[" + length + "]";
            for (int i = 0; i < length; i++)
            {
                charMsg += Convert.ToString(msg[i], 16) + " ";
            }
            charMsg += "\r\n";
            WriteLog(charMsg);
            Console.WriteLine(charMsg);

        }
        string id = Thread.CurrentThread.ManagedThreadId.ToString("00");
        private void WriteLog(string info)
        {
            return;
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = System.IO.Path.Combine(path, "Data Log");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fileName = System.IO.Path.Combine(path, $"Data_{DateTime.Now:yyyyMMdd}_serial2.txt") + id;
            using (var stream = new StreamWriter(fileName, true))
            {
                //stream.WriteLine("{0:yyyy-MM-dd HH:mm:ss},          Info{1}", DateTime.Now, info);
                stream.WriteLine("{0}", info);
            }

        }


        int send_count = 0;
        public void serial_write()
        {
            byte[,] sendMsg = new byte[33, 11]
            {
                {0xaa,0x55,0x5,0x1,0xa3,0x6,0x78,0x3c,0x0,0xd,0x48},
                {0xaa,0x55,0x5,0x2,0xa3,0x6,0x78,0x3c,0x0,0x3e,0x48},
                {0xaa,0x55,0x5,0x3,0xa3,0x6,0x78,0x3c,0x0,0xef,0x49},
                {0xaa,0x55,0x5,0x4,0xa3,0x6,0x78,0x3c,0x0,0x58,0x48},
                {0xaa,0x55,0x5,0x5,0xa3,0x6,0x78,0x3c,0x0,0x89,0x49},
                {0xaa,0x55,0x5,0x6,0xa3,0x6,0x78,0x3c,0x0,0xba,0x49},
                {0xaa,0x55,0x5,0x7,0xa3,0x6,0x78,0x3c,0x0,0x6b,0x48},
                {0xaa,0x55,0x5,0x8,0xa3,0x6,0x78,0x3c,0x0,0x94,0x48},
                {0xaa,0x55,0x5,0x9,0xa3,0x6,0x78,0x3c,0x0,0x45,0x49},
                {0xaa,0x55,0x5,0xa,0xa3,0x6,0x78,0x3c,0x0,0x76,0x49},
                {0xaa,0x55,0x5,0xb,0xa3,0x6,0x78,0x3c,0x0,0xa7,0x48},
                {0xaa,0x55,0x5,0xc,0xa3,0x6,0x78,0x3c,0x0,0x10,0x49},
                {0xaa,0x55,0x5,0xd,0xa3,0x6,0x78,0x3c,0x0,0xc1,0x48},
                {0xaa,0x55,0x5,0xe,0xa3,0x6,0x78,0x3c,0x0,0xf2,0x48},
                {0xaa,0x55,0x5,0xf,0xa3,0x6,0x78,0x3c,0x0,0x23,0x49},
                {0xaa,0x55,0x5,0x10,0xa3,0x6,0x78,0x3c,0x0,0x4c,0x4b},
                {0xaa,0x55,0x5,0x11,0xa3,0x6,0x78,0x3c,0x0,0x9d,0x4a},
                {0xaa,0x55,0x5,0x12,0xa3,0x6,0x78,0x3c,0x0,0xae,0x4a},
                {0xaa,0x55,0x5,0x13,0xa3,0x6,0x78,0x3c,0x0,0x7f,0x4b},
                {0xaa,0x55,0x5,0x14,0xa3,0x6,0x78,0x3c,0x0,0xc8,0x4a},
                {0xaa,0x55,0x5,0x15,0xa3,0x6,0x78,0x3c,0x0,0x19,0x4b},
                {0xaa,0x55,0x5,0x16,0xa3,0x6,0x78,0x3c,0x0,0x2a,0x4b},
                {0xaa,0x55,0x5,0x17,0xa3,0x6,0x78,0x3c,0x0,0xfb,0x4a},
                {0xaa,0x55,0x5,0x18,0xa3,0x6,0x78,0x3c,0x0,0x4,0x4a},
                {0xaa,0x55,0x5,0x19,0xa3,0x6,0x78,0x3c,0x0,0xd5,0x4b},
                {0xaa,0x55,0x5,0x1a,0xa3,0x6,0x78,0x3c,0x0,0xe6,0x4b},
                {0xaa,0x55,0x5,0x1b,0xa3,0x6,0x78,0x3c,0x0,0x37,0x4a},
                {0xaa,0x55,0x5,0x1c,0xa3,0x6,0x78,0x3c,0x0,0x80,0x4b},
                {0xaa,0x55,0x5,0x1d,0xa3,0x6,0x78,0x3c,0x0,0x51,0x4a},
                {0xaa,0x55,0x5,0x1e,0xa3,0x6,0x78,0x3c,0x0,0x62,0x4a},
                {0xaa,0x55,0x5,0x1f,0xa3,0x6,0x78,0x3c,0x0,0xb3,0x4b},
                {0xaa,0x55,0x5,0x20,0xa3,0x6,0x78,0x3c,0x0,0xbc,0x4e},
                {0xaa,0x55,0x5,0x20,0xa3,0x6,0x78,0x3c,0x0,0xbc,0x4e}
            };

            byte[] msg = new byte[11];
            int count = 10000;
            while (count-- != 0)
            {
                for (int i = 0; i < 33; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        msg[j] = sendMsg[i, j];
                    }
                    //serialPort.Write(msg, 0, 11);
                    Console.WriteLine("[send]=====================" + send_count++);
                    if (i == 32)
                        Thread.Sleep(20);
                    else
                        Thread.Sleep(20);
                }
            }

        }
        public void writeTest()
        {
            Thread sendThread = new Thread(serial_write);
            sendThread.Start();
        }
        /// <summary>
        /// 防止程序崩溃
        /// </summary>
        /// <param name="dateStr1"></param>
        /// <param name="dateStr2"></param>
        /// <returns></returns>
        public int wanyi(string dateStr1, string dateStr2)
        {
            DateTime t1 = Convert.ToDateTime(dateStr1);
            DateTime t2 = Convert.ToDateTime(dateStr2);
            int compNum = DateTime.Compare(t1, t2);
            return compNum;
        }
        public void printMsg(byte[] msg)
        {
            string charMsg = "";
            for (int i = 0; i < msg.Length; i++)
            {
                charMsg += Convert.ToString(msg[i], 16) + " ";
            }
            charMsg += "\r\n";
            Console.WriteLine(charMsg);

        }

    }
}
