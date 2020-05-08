using Coldairarrow.Util.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace serverForm
{
    class HY_server : HY_crc
    {
        public int port = 5005;
        public int count = 0;
        public int clientId = 0;
        Coldairarrow.Util.Sockets.SocketServer server;

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
        public static int ccl_msg_recv = 0;// 是否收到hzb的消息

        public string deadLine = "2020-06-01";
        //存放消息的队列
        //LinkedList<SocketConnection> list;
        SocketConnection theDevice = null;
        public int conCount = 0;
        public delegate void updateConStatus(int sysType, object o);

        public HY_server(int port)
        {
            this.port = port;
        }

        //arg 回调获取到的客户端信息
        //还未考虑多个客户端。
        public void serverStart(updateConStatus conStatus)
        {
            //创建服务器对象，默认监听本机0.0.0.0，端口5004
            server = new SocketServer(port);
            //处理从客户端收到的消息
            server.HandleRecMsg = new Action<byte[], SocketConnection, SocketServer>((bytes, client, theServer) =>
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
                //printMsg(bytes);
            });
            server.HandleSendMsg = new Action<byte[], SocketConnection, SocketServer>((msg, client, theServer) =>
            {
                // TODO
            });


            //处理服务器启动后事件
            server.HandleServerStarted = new Action<SocketServer>(theServer =>
            {

                Console.WriteLine("服务已启动************");
                //this.Invoke(new RefreshUI(refreshUI), new object[] { "服务已启动************" });

            });

            //处理新的客户端连接后的事件
            server.HandleNewClientConnected = new Action<SocketServer, SocketConnection>((theServer, theCon) =>
            {
                string clientIP = ((IPEndPoint)theCon._socket.RemoteEndPoint).Address.ToString();
                theCon.Property = clientIP;
                theServer.AddConnection(theCon);
                theDevice = theCon;
                conCount++;
                conStatus(conCount, theCon);
                Console.WriteLine($@"一个新的客户端[{clientIP}]接入，当前连接数：{theServer.GetConnectionCount()}");
            });

            //处理客户端连接关闭后的事件
            server.HandleClientClose = new Action<SocketConnection, SocketServer>((theCon, theServer) =>
            {
                theServer.RemoveConnection(theCon);
                conCount--;
                Console.WriteLine($@"一个客户端关闭，当前连接数为：{theServer.GetConnectionCount()}");
                // this.Invoke(new RefreshUI(refreshUI), new object[] { $@"一个新的客户端接入，当前连接数：{theServer.GetConnectionCount()}" });
            });

            //处理异常
            server.HandleException = new Action<Exception>(ex =>
            {
                // this.Invoke(new RefreshUI(refreshUI), new object[] { $@"ohoops,an error !!!!" });
            });

            //服务器启动
            server.StartServer();

        }

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
                            //Console.WriteLine("port = " + sendCount);
                            if (port == 5005)
                                sendDataAfter20(msg);//收到20后发送数据
                            else if (port == 5004)
                                sendDataAfter0C(msg);

                            addMsgToQueue(msg);
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
                printMsg("send = " ,msg);
                if (theDevice != null)
                {
                    theDevice.Send(sendMsg);
                }
                //serialPort.Write(msg, 0, 11);
            }

        }
        public void sendData(int type, byte[] msg)
        {
            sendCount = 3;
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
                //printMsg("[OKMsg]", msg);
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


