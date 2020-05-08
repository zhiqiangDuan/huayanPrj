using Coldairarrow.Util.Sockets;
using LBSoft.IndustrialCtrls.Buttons;
using serverForm.HY_sys;
using serverForm.HY_SYS_main_status;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using static serverForm.HY_server;
using static serverForm.HY_sys.HY_cmd_handle;

namespace serverForm
{


    public partial class Form1 : Form
    {

        public static int SYS_JB = 2;
        public static int SYS_BJQ = 1;
        public static int SYS_HZB = 4;
        public static int SYS_CCL = 3;
        public static int SYS_CKY = 0;
        HY_server hy_server_hzb;//server类。包含读写功能
        HY_server hy_server_ccl;//server类。包含读写功能
        HY_server hy_server_cky;//server类。包含读写功能
        HY_server hy_server_jb;//server类。包含读写功能
        HY_server hy_server_bjq;//server类。包含读写功能


        Hy_client hy_client_hzb;//client类。包含读写功能
        Hy_client hy_client_ccl;//client类。包含读写功能
        Hy_client hy_client_cky;//client类。包含读写功能
        Hy_client hy_client_jb;//client类。包含读写功能
        Hy_client hy_client_bjq;//client类。包含读写功能

        Hy_client hy_PC;
        Serial s_hzb;
        Serial s_ccl;
        Serial s_bjq;
        Serial s_jb;
        Serial s_cky;
        Thread readMsg;
        Thread readCCL_init_info;
        Thread clientInit;
        Thread checkRecvMsg;
        string clientIp = "";
        private int isClientConnect = 0;
        public static int hzb_msg_recv = 0;
        public static int ccl_msg_recv = 0;
        int but_press_delay_ccl = 1;//CCL的命令相应慢。按钮按下2S内不响应其他操作  0不响应 1响应
        int but_dealy_counter_ccl = 0;
        int but_press_delay_bjq = 1;//bqq的命令相应慢。按钮按下2S内不响应其他操作  0不响应 1响应
        int but_dealy_counter_bjq = 0;
        int connect_mode = 0;
        List<PictureBox> listPic = new List<PictureBox>();//保存全景图系统的链表
        //List<PictureBox> listPicFliker = new List<PictureBox>();//保存全景图系统闪烁的链表
        List<SysPicInfo> listSysPic = new List<SysPicInfo>();//保存系统图片系统闪烁的链表信息
        //发送命令的button
        List<Button> but_cmd_ccl; //保存ccl发送命令的button
        List<Button> but_cmd_hzb; //保存hzb发送命令的button
        List<Button> but_cmd_cky; //保存cky发送命令的button
        List<Button> but_cmd_bjq; //保存bjq发送命令的button
        List<Button> but_cmd_jb; //保存jb发送命令的button

        //显示按钮 LBBUTTON
        List<LBButton> but_show_ccl; //保存ccl的button
        List<LBButton> but_show_cky; //保存ccl的button
        List<LBButton> but_show_bjq; //保存ccl的button
        List<LBButton> but_show_jb; //保存ccl的button
        List<LBButton> but_show_hzb; //保存ccl的button
        List<LBButton> but_fliker;//闪烁的链表

        List<Jb_data>[] list_jb_screen;//警报在左边显示的数据链表
        Label[] jbLabelShow;//警报显示状态的label
        int[] jbsShowIndex;//每个label当前显示的信息
        //发送的数据需要按位发送。
        //全景图保存的配置文件
        const string fullPicCfgName = "fullPiccfg.txt";
        //系统图标保存的位置
        const string sysPicCfgName = "sysPiccfg.txt";
        string fullPicPath = "";
        public int[] hy_sys_recv_msg = new int[5];
        //保存当前系统是否有事件触发
        public static int[] hy_System_alarm = new int[5];
        public Form1()
        {
            //临时方案，不检查控件的合法性。存在多线程的风险
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            //更新连接状态
            
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            InitializeComponent();
            loadCfg();
            setButtonBackColor();
            Login login = new Login();
            login.StartPosition = FormStartPosition.CenterScreen;
            login.ShowDialog();
            root_mod(login);


            this.WindowState = FormWindowState.Maximized;    //最大化窗体 

            initUITable();
            initJbTabInfo();
            timer_time.Start();
            timer_fliker.Start();
            timer_but_delay.Start();
            connectServer();
            //openSerial();
            //clientInit = new Thread(initClientThread);
            //clientInit.Start();
            readMsg = new Thread(getMsg);
            readMsg.Start();
            checkRecvMsg = new Thread(checkMsgRecvThread);
            checkRecvMsg.Start();
        }


        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteLog(e.ExceptionObject.ToString());
            MessageBox.Show(e.ExceptionObject.ToString());
        }

        void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            WriteLog(e.Exception.ToString());
            MessageBox.Show(e.Exception.ToString());
        }

        private void WriteLog(string info)
        {

            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = System.IO.Path.Combine(path, "Data Log");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (var stream = new StreamWriter(System.IO.Path.Combine(path, $"Data_{DateTime.Now:yyyyMMdd}.txt"), true))
            {
                stream.WriteLine("{0:yyyy-MM-dd HH:mm:ss},          Info{1}", DateTime.Now, info);
            }

        }
        /// <summary>
        /// root模式显示的设置。root模式下打开debug按钮。其他模式下隐藏
        /// </summary>
        /// <param name="login"></param>
        /// 
        public void openSerial()
        {
            s_hzb = new Serial("COM5");//hzb
            s_hzb.initQueue();
            s_ccl = new Serial("COM4");
            s_bjq = new Serial("COM2");
            s_jb = new Serial("COM3");
            s_cky = new Serial("COM1");
        }

        public void connectServer()
        {
            updateConStatus updatecon = updateConnectStatusCallBack;
            if (connect_mode == 1)
            {
                hy_server_cky = new HY_server(5001);
                hy_server_bjq = new HY_server(5002);
                hy_server_jb = new HY_server(5003);
                hy_server_ccl = new HY_server(5004);
                hy_server_hzb = new HY_server(5005);
                hy_server_jb.initQueue();
                hy_server_jb.serverStart(updatecon);
                hy_server_bjq.serverStart(updatecon);
                hy_server_hzb.serverStart(updatecon);
                hy_server_ccl.serverStart(updatecon);
                hy_server_cky.serverStart(updatecon);
            }
            else if(connect_mode == 2)
            {
                hy_client_jb = new Hy_client(5006);
                hy_client_bjq = new Hy_client(5007);
                hy_client_cky = new Hy_client(5008);
                hy_client_ccl = new Hy_client(5009);
                hy_client_hzb = new Hy_client(5010);

                hy_client_jb.initQueue();

                hy_client_jb.clientInit();
                hy_client_bjq.clientInit();
                hy_client_hzb.clientInit();
                hy_client_ccl.clientInit();
                hy_client_cky.clientInit();
            }
            
        }
        /// <summary>
        /// 管理员模式
        /// </summary>
        /// <param name="login"></param>
        public void root_mod(Login login)
        {
            if (login.login_resulut != Login.ROOT)
            {
                button18.Visible = false;

            }
            connect_mode = login.login_mode;
        }

        public void SetTextSafePost(object text)
        {

        }

        
        public delegate void RefreshUI(int sysType, object o);
        public void refreshUI(int sysType, Object o)
        {
            
            switch (sysType)
            {
                case HY_cmd.HY_CMD_SYS_CKY:
                    HY_SYS_main_cky main_cky = new HY_SYS_main_cky(but_cmd_cky, but_fliker, but_show_cky);
                    main_cky.updateCKY((HY_sys_CKY)o);
                    hy_sys_recv_msg[0] = 1;
                    break;
                case HY_cmd.HY_CMD_SYS_BJQ:
                    HY_SYS_main_bjq main_bjq = new HY_SYS_main_bjq(but_cmd_bjq, but_fliker, but_show_bjq);
                    main_bjq.updateBJQ((HY_sys_BJQ)o);
                    hy_sys_recv_msg[1] = 1;
                    break;
                case HY_cmd.HY_CMD_SYS_JB:
                    //List<Jb_data>[] list_jb_screen;//警报在左边显示的数据链表
                    //Label[] jbLabelShow;//警报显示状态的label
                    //int[] jbsShowIndex;//每个label当前显示的信息
                    //jb_tab_screen 左侧显示信息的tabpage
                    HY_SYS_main_jb main_jb = new HY_SYS_main_jb(but_cmd_jb, but_fliker, but_show_jb, list_jb_screen, jb_tab_screen);
                    main_jb.updateJB((HY_sys_JB)o);
                    //更新左侧的显示屏内容
                    jb_update_screen(list_jb_screen);
                    hy_sys_recv_msg[2] = 1;
                    break;
                case HY_cmd.HY_CMD_SYS_CCL:
                    HY_SYS_main_ccl main_ccl = new HY_SYS_main_ccl(but_cmd_ccl, but_fliker, but_show_ccl);
                    main_ccl.updateCCL((HY_sys_CCL)o, 0);
                    //hy_sys_recv_msg[3] = 1;
                    break;
                case HY_cmd.HY_CMD_SYS_HZB:
                    HY_SYS_main_hzb main_hzb = new HY_SYS_main_hzb(but_cmd_hzb, but_fliker, but_show_hzb);
                    main_hzb.updateHZB((HY_sys_HZB)o);
                    //hy_sys_recv_msg[4] = 1;
                    break;
                default:
                    //报错。错误系统。未识别的指令
                    break;
            }
            
            // this.label1.Text = (string)o;
        }

        public void showPageMassage(int pageIndex, int labelIndex)
        {
            if (list_jb_screen[pageIndex].Count != 0)
            {
                Jb_data data = list_jb_screen[pageIndex][labelIndex];
                jbLabelShow[pageIndex].Text = data.strType;
            }
            else
            {
                jbLabelShow[pageIndex].Text = "";
            }
        }
        public int[] lastShowIndex = new int[4];
        private void jb_update_screen(List<Jb_data>[] list_jb_screen)
        {
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    if (lastShowIndex[i] < list_jb_screen[i].Count - 1)

                    // int index = jb_tab_screen.SelectedIndex;
                    {
                        jbsShowIndex[i] = list_jb_screen[i].Count - 1;
                        lastShowIndex[i] = jbsShowIndex[i];
                    }

                    showPageMassage(i, jbsShowIndex[i]);
                }
            }
            catch(Exception e)
            {
                WriteLog(e.Message);
            }
            

        }
        public void initClientThread()
        {
            connectServer();
        }
        /// <summary>
        /// 获取消息队列中的消息。并处理
        /// </summary>
        public void getMsg()
        {
            Thread.Sleep(100);
            //HY_server hy_server = new HY_server();
            HY_cmd_handle handle = new HY_cmd_handle();
            updateUIcallBack updateUI = refreshUI;// updateUICallBack;
            QueueMsg msgQueue = null;
            while (true)
            {
                if(connect_mode == 1)  msgQueue = hy_server_hzb.getQueueMsg();
                else if (connect_mode == 2)  msgQueue = hy_client_hzb.getQueueMsg();
                if (msgQueue != null)
                {
                    //未加锁，有风险。
                    //多线程发送，粘包
                    //解析数据
                    //printMsg(msgQueue.msg);
                    //pase之后直接在handle类中处理需要显示的控件。控件做成单独封装成一个类
                    //string time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss fff");
                    //tb_sys_log.AppendText("["+time+"]"+printMsg(msgQueue.msg));
                    //tb_sys_log.SelectionStart = this.tb_sys_log.TextLength;
                    //tb_sys_log.ScrollToCaret();
                    handle.hy_cmd_parse(msgQueue.msg, updateUI);
                    Thread.Sleep(100);
                    ///break;
                }
                else
                {
                    Thread.Sleep(100); // 没有数据则100ms后再获取
                }

            }
        }

        [Obsolete]
        /// <summary>
        /// 客户端连接后的回调函数，需要发送上电初始化命令
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="obj"></param>
        public void updateConnectStatusCallBack(int msgType, Object obj)
        {
            SocketConnection client = (SocketConnection)obj;
            string clientIP = ((IPEndPoint)client._socket.RemoteEndPoint).Address.ToString();
            //this.label1.Text += clientIP;
            clientIp = clientIP;
            //lb_sys_ccl_conn.ButtonColor = Color.Green;
            isClientConnect = 1;
            //Thread getInitInfo = new Thread(getInitInfoThread);
            //getInitInfo.Start();
        }

        /// <summary>
        /// 
        /// 发送商店初始化的命令
        /// </summary>
        public void sendInitInfo()
        {
            //目前是发送了ccl的初始化
           // byte[] getInitInfoCmd = { 0xAA ,0x55 ,0x04 ,0x09 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 , 0x00, 0x00 };
            byte[] getInitInfoCmd = { 0xAA, 0x55, 0x04, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            hy_server_ccl.sendData(getInitInfoCmd);
        }
        public void hzbClearFliker()
        {
            if (but_fliker.Contains(but_hzb_show_1_7))
            {
                but_fliker.Remove(but_hzb_show_1_7);
                
            }
            if (but_fliker.Contains(but_hzb_show_1_8))
            {
                but_fliker.Remove(but_hzb_show_1_8);
                
            }
            if (but_fliker.Contains(but_hzb_show_1_9))
            {
                but_fliker.Remove(but_hzb_show_1_9);
                
            }
            if (but_fliker.Contains(but_hzb_show_1_10))
            {
                but_fliker.Remove(but_hzb_show_1_10);
                
            }
            Thread.Sleep(1000);
            but_hzb_show_1_7.ButtonColor = SystemColors.ControlLight;
            but_hzb_show_1_8.ButtonColor = SystemColors.ControlLight;
            but_hzb_show_1_9.ButtonColor = SystemColors.ControlLight;
            but_hzb_show_1_10.ButtonColor = SystemColors.ControlLight;
        }
        /// <summary>
        /// 判断是否通讯故障
        /// </summary>
        int ccl_recv_clear = 0;
        public void checkMsgRecvThread()
        {
            while (true)
            {
                Thread.Sleep(3 * 1000);
                if (hy_sys_recv_msg[0] == 0)
                {
                    lb_sys_cky_conn.ButtonColor = Color.Red;
                    but_cky_show_15.ButtonColor = Color.Red;
                }
                if (hy_sys_recv_msg[1] == 0)
                {
                    lb_sys_bjq_conn.ButtonColor = Color.Red;
                    but_bjq_6.ButtonColor = Color.Red;
                }
                if (hy_sys_recv_msg[2] == 0)
                {
                    lb_sys_jb_conn.ButtonColor = Color.Red;
                }
                if (ccl_msg_recv == 0)
                {
                    lb_sys_ccl_conn.ButtonColor = Color.Red;
                    //lbButton10.ButtonColor = Color.Red;
                }
                else
                {
                    lb_sys_ccl_conn.ButtonColor = Color.Green;
                }
                if (hzb_msg_recv == 0)
                {
                    lb_sys_hzb_conn.ButtonColor = Color.Red;
                    but_hzb_show_1_4.ButtonColor = Color.Red;
                    //取消闪烁状态
                    hzbClearFliker();
                }
                else
                {
                    lb_sys_hzb_conn.ButtonColor = Color.Green;
                }

                ccl_recv_clear++;
                for (int i = 0; i < 5; i++)
                {
                    hy_sys_recv_msg[i] = 0;
                    hzb_msg_recv = 0;
                    ccl_msg_recv = 0;
                    
                }
            }


        }
        public void getInitInfoThread()
        {
            return;// 不用了
            Thread.Sleep(100);
            while (true)
            {
                sendInitInfo();
                if (HY_SYS_main_ccl.ccl_status_init != 1)//收到命令后，该变量被置1
                {
                    HY_SYS_main_ccl.ccl_status_init = 2;
                    Thread.Sleep(500);
                }
                else {
                    Thread.Sleep(5* 1000);
                }
                
            }
        }


        public string  printMsg(byte[] msg)
        {
            string charMsg = "";
            //printMsg(msg.msg);
            foreach (byte bitMsg in msg)
            {
                charMsg += Convert.ToString(bitMsg, 16) + " ";
                //Console.WriteLine(Convert.ToString(bitMsg, 16));
            }
            charMsg += "\r\n";
            //textBox1.Text += charMsg+ "\n";
            //this.Invoke(new RefreshUI(refreshUI), new object[] { $"msg:" + count + " " + charMsg + "\n" });
            //Console.WriteLine(charMsg);
            return charMsg;
        }


        public void initJbTabInfo()
        {

            jbLabelShow = new Label[4];
            jbLabelShow[0] = label_jb_hj;
            jbLabelShow[1] = label_jb_ld;
            jbLabelShow[2] = label_jb_pb;
            jbLabelShow[3] = label_jb_gz;
            //当前显示的索引
            jbsShowIndex = new int[4];
            //消息链表
            list_jb_screen = new List<Jb_data>[4];
            for (int i = 0; i < 4; i++)
            {
                list_jb_screen[i] = new List<Jb_data>();
            }
        }
        public void initUITable()
        {
            but_cmd_ccl = new List<Button>();
            but_cmd_hzb = new List<Button>();
            but_cmd_cky = new List<Button>();
            but_cmd_bjq = new List<Button>();
            but_cmd_jb = new List<Button>();

            but_show_ccl = new List<LBButton>();
            but_show_hzb = new List<LBButton>();
            but_show_cky = new List<LBButton>();
            but_show_bjq = new List<LBButton>();
            but_show_jb = new List<LBButton>();

            but_fliker = new List<LBButton>();
            initCCLTable();
            initCKYTable();
            initHZBTable();
            initBJQTable();
            initJBTable();
        }
        public void initCCLTable()
        {

            //进的状态反的
            //左
            but_show_ccl.Add(but_ccl_show_1_4);
            but_show_ccl.Add(but_ccl_show_1_3);
            but_show_ccl.Add(but_ccl_show_1_2);
            but_show_ccl.Add(but_ccl_show_1_1);
            but_show_ccl.Add(but_ccl_show_1_5);
            but_show_ccl.Add(but_ccl_show_1_6);
            but_show_ccl.Add(but_ccl_show_1_7);
            but_show_ccl.Add(but_ccl_show_1_8);
            but_show_ccl.Add(but_ccl_show_1_9);
            but_show_ccl.Add(but_ccl_show_1_10);
            but_show_ccl.Add(but_ccl_show_1_11);

            //右    _ccl
            but_show_ccl.Add(but_ccl_show_2_4);
            but_show_ccl.Add(but_ccl_show_2_3);
            but_show_ccl.Add(but_ccl_show_2_2);
            but_show_ccl.Add(but_ccl_show_2_1);
            but_show_ccl.Add(but_ccl_show_2_5);
            but_show_ccl.Add(but_ccl_show_2_6);
            but_show_ccl.Add(but_ccl_show_2_7);
            but_show_ccl.Add(but_ccl_show_2_8);
            but_show_ccl.Add(but_ccl_show_2_9);
            but_show_ccl.Add(but_ccl_show_2_10);
            but_show_ccl.Add(but_ccl_show_2_11);

            but_show_ccl.Add(lb_but_ccl_sd_1);
            but_show_ccl.Add(lb_but_ccl_sd_2);

            but_show_ccl.Add(lb_but_ccl_wc_1);
            but_show_ccl.Add(lb_but_ccl_wc_2);
            //26
            but_show_ccl.Add(lb_sys_ccl_conn);
            but_show_ccl.Add(lb_ccl_run);
            //命令按钮 发送cmd
            but_cmd_ccl.Add(but_cmd_ccl_1_4);
            but_cmd_ccl.Add(but_cmd_ccl_1_3);
            but_cmd_ccl.Add(but_cmd_ccl_1_2);
            but_cmd_ccl.Add(but_cmd_ccl_1_1);
            but_cmd_ccl.Add(but_cmd_ccl_1_5);
            but_cmd_ccl.Add(but_cmd_ccl_1_6);
            but_cmd_ccl.Add(but_cmd_ccl_1_7);
            but_cmd_ccl.Add(but_cmd_ccl_1_8);
            but_cmd_ccl.Add(but_cmd_ccl_1_9);
            but_cmd_ccl.Add(but_cmd_ccl_1_10);
            but_cmd_ccl.Add(but_cmd_ccl_1_11);

            but_cmd_ccl.Add(but_cmd_ccl_2_4);
            but_cmd_ccl.Add(but_cmd_ccl_2_3);
            but_cmd_ccl.Add(but_cmd_ccl_2_2);
            but_cmd_ccl.Add(but_cmd_ccl_2_1);
            but_cmd_ccl.Add(but_cmd_ccl_2_5);
            but_cmd_ccl.Add(but_cmd_ccl_2_6);
            but_cmd_ccl.Add(but_cmd_ccl_2_7);
            but_cmd_ccl.Add(but_cmd_ccl_2_8);
            but_cmd_ccl.Add(but_cmd_ccl_2_9);
            but_cmd_ccl.Add(but_cmd_ccl_2_10);
            but_cmd_ccl.Add(but_cmd_ccl_2_11);
            //21
            but_cmd_ccl.Add(but_ccl_send_cmd);
            
            
        }
        public void initCKYTable()
        {
            but_show_cky.Add(but_cky_show_1);
            but_show_cky.Add(but_cky_show_2);
            but_show_cky.Add(but_cky_show_3);
            but_show_cky.Add(but_cky_show_4);
            but_show_cky.Add(but_cky_show_5);
            but_show_cky.Add(but_cky_show_6);
            but_show_cky.Add(but_cky_show_7);
            but_show_cky.Add(but_cky_show_8);
            but_show_cky.Add(but_cky_show_9);
            but_show_cky.Add(but_cky_show_10);
            but_show_cky.Add(but_cky_show_11);
            but_show_cky.Add(but_cky_show_12);
            but_show_cky.Add(but_cky_show_13);
            but_show_cky.Add(but_cky_show_14);
            but_show_cky.Add(but_cky_show_15);
            but_show_cky.Add(but_cky_show_16);
            but_show_cky.Add(but_cky_show_17);
            but_show_cky.Add(but_cky_show_18);
            but_show_cky.Add(but_cky_show_19);
            but_show_cky.Add(but_cky_show_20);
            but_show_cky.Add(but_cky_show_21);
            but_show_cky.Add(but_cky_show_22);
            but_show_cky.Add(but_cky_show_23);
            but_show_cky.Add(but_cky_show_24);
            but_show_cky.Add(but_cky_show_25);

            but_show_cky.Add(lb_sys_cky_conn);

            but_cmd_cky.Add(but_cky_cmd_0);
            but_cmd_cky.Add(but_cky_cmd_1);
            
            but_cmd_cky.Add(but_cky_cmd_3);
            but_cmd_cky.Add(but_cky_cmd_4);
            but_cmd_cky.Add(but_cky_cmd_5);
            but_cmd_cky.Add(but_cky_cmd_6);
            but_cmd_cky.Add(but_cky_cmd_7);
            but_cmd_cky.Add(but_cky_cmd_8);
            but_cmd_cky.Add(but_cky_cmd_9);
            but_cmd_cky.Add(but_cky_cmd_2);//线路
            //but_cky_show_25
        }
        /// <summary>
        /// 初始化航行值班报警系统的状态指示按钮
        /// </summary>
        public void initHZBTable()
        {
            but_show_hzb.Add(but_hzb_show_1_1);
            but_show_hzb.Add(but_hzb_show_1_2);
            but_show_hzb.Add(but_hzb_show_1_3);
            but_show_hzb.Add(but_hzb_show_1_4);
            but_show_hzb.Add(but_hzb_show_1_5);
            but_show_hzb.Add(but_hzb_show_1_6);
            but_show_hzb.Add(but_hzb_show_1_7);
            but_show_hzb.Add(but_hzb_show_1_8);
            but_show_hzb.Add(but_hzb_show_1_9);
            but_show_hzb.Add(but_hzb_show_1_10);
            //10
            but_show_hzb.Add(lb_sys_hzb_conn);
            but_cmd_hzb.Add(but_cmd_hzb_jj);
            but_cmd_hzb.Add(but_cmd_hzb_xy);
            //but_cky_show_25
        }
        public void initBJQTable() 
        {
            but_cmd_bjq.Add(but_bjq_cmd_1);
            but_cmd_bjq.Add(but_bjq_cmd_2);
            but_cmd_bjq.Add(but_bjq_cmd_3);
            but_cmd_bjq.Add(but_bjq_cmd_4);
            but_cmd_bjq.Add(but_bjq_cmd_5);
            but_cmd_bjq.Add(but_bjq_cmd_6);

            but_show_bjq.Add(but_bjq_1);
            but_show_bjq.Add(but_bjq_2);
            but_show_bjq.Add(but_bjq_3);
            but_show_bjq.Add(but_bjq_4);
            but_show_bjq.Add(but_bjq_5);
            but_show_bjq.Add(but_bjq_6);
            //6
            but_show_bjq.Add(lb_sys_bjq_conn);
        }
        public void initJBTable()
        {
            but_cmd_jb.Add(jb_but_cmd_send_1);
            but_cmd_jb.Add(jb_but_cmd_send_2);
            but_cmd_jb.Add(jb_but_cmd_send_3);
            but_cmd_jb.Add(jb_but_cmd_send_4);
            but_cmd_jb.Add(jb_but_cmd_send_5);
            but_cmd_jb.Add(jb_but_cmd_send_6);
            but_cmd_jb.Add(jb_but_cmd_send_7);
            but_cmd_jb.Add(jb_but_cmd_send_8);
            but_cmd_jb.Add(jb_but_cmd_7);
            but_cmd_jb.Add(jb_but_cmd_8);
            

            but_show_jb.Add(but_jb_1);
            but_show_jb.Add(but_jb_2);
            but_show_jb.Add(but_jb_3);
            but_show_jb.Add(but_jb_4);
            but_show_jb.Add(but_jb_5);
            but_show_jb.Add(but_jb_6);
            but_show_jb.Add(but_jb_7);
            but_show_jb.Add(but_jb_8);
            but_show_jb.Add(but_jb_9);
            but_show_jb.Add(but_jb_10);
            but_show_jb.Add(but_jb_11);
            but_show_jb.Add(but_jb_12);
            but_show_jb.Add(but_jb_13);
            but_show_jb.Add(but_jb_14);
            but_show_jb.Add(but_jb_15);
            but_show_jb.Add(but_jb_16);
            but_show_jb.Add(but_jb_17);
            but_show_jb.Add(but_jb_18);
            but_show_jb.Add(but_jb_19);
            but_show_jb.Add(but_jb_20);
            but_show_jb.Add(but_jb_21);
            but_show_jb.Add(but_jb_22);
            but_show_jb.Add(but_jb_23);
            but_show_jb.Add(but_jb_24);
            //24
            but_show_jb.Add(lb_sys_jb_conn);

        }

        //private int fliker_cur_color = 0;// 用于切换当前状态
        /// <summary>
        /// 使用定时器来闪烁list中的按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_fliker_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < but_fliker.Count; i++)
            {
                ///
                ///list的操作需要增加互斥。
                ///
                try
                {
                    if (but_fliker[i].Equals(but_show_hzb[5]))
                    {
                        if (but_fliker[i].ButtonColor.Equals(Color.Gold))
                            but_fliker[i].ButtonColor = SystemColors.ControlLight;
                        else
                            but_fliker[i].ButtonColor = Color.Gold;
                    }
                    else
                    {
                        if (but_fliker[i].ButtonColor.Equals(Color.Red))
                            but_fliker[i].ButtonColor = SystemColors.ControlLight;
                        else
                            but_fliker[i].ButtonColor = Color.Red;
                    }

                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                }

            }
            //foreach (SysPicInfo lpf in listSysPic)
            for (int i = 0;i < listSysPic.Count;i++) 
            {
                if (but_edit_mod == 1)//编辑模式，已有的图标也停止闪烁
                    return;
                if (hy_System_alarm[listSysPic[i].sys] == 1)
                {
                    if (listSysPic[i].pb.Visible == true)
                        listSysPic[i].pb.Visible = false;
                    else
                        listSysPic[i].pb.Visible = true;
                }
                else
                {
                    if (listSysPic[i].pb.Visible == true)
                        listSysPic[i].pb.Visible = false;
                }
            }

        }


        /// <summary>
        /// 不用了。可删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click_1(object sender, EventArgs e)
        {
            byte[] sendMsg;
            byte[] sendMsg1 = { 0xAA, 0x55, 0x03, 0x00, 0x00, 0x01, 0x12, 0x23, 0x24, 0x00, 0x00 };
            sendMsg = s_jb.getCRC(sendMsg1);
            s_cky.addMsgToQueue(sendMsg);
            byte[] sendMsg2 = { 0xAA, 0x55, 0x03, 0x00, 0x00, 0x02, 0x12, 0x23, 0x24, 0x00, 0x00 };
            sendMsg = s_jb.getCRC(sendMsg2);
            s_cky.addMsgToQueue(sendMsg);
        }


        /// <summary>
        /// CCL系统的命令发送按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        public byte ccl_send_cmd_addr = 0x02;//发送的地址
        public byte ccl_send_cmd_msg = 0xff;//初始化为0xff表示无状态
        public int ccl_send_cur_fliker = 0xff;//当前需要发送的index
        static public int ccl_but_status_L = 0;
        static public int ccl_but_status_R = 0;
        public int ccl_but_ACK_L = 0xD0;
        public int ccl_but_ACK_R = 0xE0;
        private void ccl_button_click(object sender, EventArgs e)
        {

            byte[] sendMsg = { 0xAA, 0x55, 0x04, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte cmd_l_ts = 0x10;
            byte cmd_l_ts_cancle = 0x30;

            byte cmd_R_ts = 0x20;
            byte cmd_R_ts_cancle = 0x40;
            HY_crc crc = new HY_crc();
            int index = but_cmd_ccl.IndexOf((Button)sender);
            if (but_press_delay_ccl == 0)
            {
                but_dealy_counter_ccl = 0;
                return;
            }
            but_press_delay_ccl = 0;
            ccl_send_cur_fliker = index;
            if (index < 11)//左
            {
                int lastCmd = HY_SYS_main_ccl.lastStatusLeft >> 4;
                int butIndex = HY_SYS_main_ccl.lastStatusLeft & 0x0f;//上一次的index
                //记录按钮send需要发送的数据
                if (lastCmd == 0x05)
                {
                    if (butIndex != index)
                    {
                        ccl_send_cmd_msg = (byte)(0x50 + index);
                    }
                    else
                    {
                        ccl_send_cmd_msg = (byte)(0xD0 + index);
                    }
                }
                else
                {
                    ccl_send_cmd_msg = (byte)(0x50 + index);
                }


                if (lastCmd == 0x01)
                {
                    sendMsg[5] = (byte)(cmd_l_ts_cancle + (byte)index);
                    ccl_send_cmd_msg = 0xff;
                }
                else
                {
                    sendMsg[5] = (byte)(cmd_l_ts + (byte)index);
                }
                byte[] test = crc.getCRC(sendMsg);
                HY_sys_CCL ccl = new HY_sys_CCL(test);
                ccl.parseMsg();
                if(connect_mode == 1) hy_server_ccl.sendData(0,sendMsg);
                else if (connect_mode == 2) hy_client_ccl.sendData(0, sendMsg);
            }
            else if (index < 22)//右
            {
                index = index - 11;
                int lastCmd = HY_SYS_main_ccl.lastStatusRight >> 4;
                int butIndex = HY_SYS_main_ccl.lastStatusRight & 0x0f;//上一次的index
                Console.WriteLine("-->"+ lastCmd+","+ butIndex+","+index);
                //记录按钮send需要发送的数据
                if (lastCmd == 0x06)
                {
                    if (butIndex != index)
                    {
                        ccl_send_cmd_msg = (byte)(0x60 + index);
                    }
                    else
                    {
                        ccl_send_cmd_msg = (byte)(0xE0 + index);
                    }
                }
                else
                {
                    ccl_send_cmd_msg = (byte)(0x60 + index);
                }


                if (lastCmd == 0x02)
                {
                    sendMsg[5] = (byte)(cmd_R_ts_cancle + (byte)index);
                    ccl_send_cmd_msg = 0xff;
                }
                else
                {
                    sendMsg[5] = (byte)(cmd_R_ts + (byte)index);
                }
                byte[] test = crc.getCRC(sendMsg);
                HY_sys_CCL ccl = new HY_sys_CCL(test);
                ccl.parseMsg();
                if (connect_mode == 1) hy_server_ccl.sendData(0, sendMsg);
                else if (connect_mode == 2) hy_client_ccl.sendData(0, sendMsg);
            }
            else // 发送按钮
            {
                if (HY_SYS_main_ccl.is_ccl_reset == 1) //是否发送消音
                {
                    //此处应该还要判断下是否是消音按键！！！
                    //HY_SYS_main_ccl.is_ccl_reset = 0;
                    byte[] sendMsgXy = { 0xAA, 0x55, 0x04, 0xC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    sendMsgXy[4] = 0x02;//(byte)(HY_SYS_main_ccl.ccl_cmd | (1 << 1));
                    if (connect_mode == 1) hy_server_ccl.sendData(0, sendMsgXy);
                    else if (connect_mode == 2) hy_client_ccl.sendData(0, sendMsgXy);
                    return;
                }
                if (ccl_send_cmd_msg != 0xff)
                {
                    sendMsg[3] = 0x0C;// ccl_send_cmd_addr;
                    sendMsg[4] = 0x00;// 
                    sendMsg[5] = ccl_send_cmd_msg;//保存上位机按下的操作
                    ccl_send_cmd_msg = 0xff;
                    byte[] test = crc.getCRC(sendMsg);
                    HY_sys_CCL ccl = new HY_sys_CCL(test);
                    ccl.parseMsg();

                    if (connect_mode == 1) hy_server_ccl.sendData(0, sendMsg);
                    else if (connect_mode == 2) hy_client_ccl.sendData(0, sendMsg);

                }
                else//没有需要发送的命令
                {
                    byte[] sendMsgXy = { 0xAA, 0x55, 0x04, 0xC, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    if (connect_mode == 1) hy_server_ccl.sendData(0, sendMsg);
                    else if (connect_mode == 2) hy_client_ccl.sendData(0, sendMsg);
                    return;
                }
            }
           
        }


        /*
         private void ccl_button_click(object sender, EventArgs e)
        {

            //byte[] sendMsg = { 0xAA, 0x55, 0x04, 0x0C, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] sendMsg = { 0xAA, 0x55, 0x04, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte cmd_l_ts = 0x10;
            byte cmd_l_ts_cancle = 0x30;

            byte cmd_R_ts = 0x20;
            byte cmd_R_ts_cancle = 0x40;
            HY_crc crc = new HY_crc();

            int index = but_cmd_ccl.IndexOf((Button)sender);
            if (HY_SYS_main_ccl.is_ccl_reset == 1)
            {
                HY_SYS_main_ccl.is_ccl_reset = 0;
                ccl_but_status_L = 0;
                ccl_but_status_R = 0;
                byte[] sendMsgXy = { 0xAA, 0x55, 0x04, 0xC, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                hy_server_ccl.sendData(sendMsg);
                return;
            }
            ccl_send_cur_fliker = index;
            if (index < 11)//左
            {
                int lastCmd = HY_SYS_main_ccl.lastStatusLeft >> 4;
                int butIndex = lastCmd & 0x0f;
                Console.WriteLine("cmd = ============"+lastCmd);
                if (lastCmd == 0x05 && butIndex != index)
                {
                    ccl_send_cmd_msg = (byte)(0xD0 + index);
                }
                else
                {
                    ccl_send_cmd_msg = (byte)(0x50 + index);
                }
                
                if (ccl_but_status_L == 0)
                {
                    sendMsg[5] = (byte)(cmd_l_ts + (byte)index);
                    ccl_but_status_L = 1;
                }
                else
                {
                    sendMsg[5] = (byte)(cmd_l_ts_cancle + (byte)index);
                    ccl_but_status_L = 0;
                }

                byte[] test = crc.getCRC(sendMsg);
                HY_sys_CCL ccl = new HY_sys_CCL(test);
                ccl.parseMsg();
                hy_server_ccl.sendData(0,sendMsg);
            }
            else if (index < 22)
            {
                int lastCmd = HY_SYS_main_ccl.lastStatusRight >> 4;
                int butIndex = lastCmd & 0x0f;
                if (lastCmd == 0x06)
                 {

                     ccl_send_cmd_msg = (byte)(0xE0 + index - 11);
                     return;
                 }
                ccl_send_cmd_msg = (byte)(0x60 + index - 11);
                if (ccl_but_status_R == 0)
                {
                    sendMsg[5] = (byte)(cmd_R_ts + (byte)(index - 11));
                    ccl_but_status_R = 1;
                }
                else
                {
                    sendMsg[5] = (byte)(cmd_R_ts_cancle + (byte)(index -11));
                    ccl_but_status_R = 0;
                }

                byte[] test = crc.getCRC(sendMsg);
                HY_sys_CCL ccl = new HY_sys_CCL(test);
                ccl.parseMsg();
                hy_server_ccl.sendData(0, sendMsg);
            }
            else // 发送按钮
            {
                if (ccl_send_cmd_msg != 0xff)
                {
                    sendMsg[3] = 0x0C;// ccl_send_cmd_addr;
                    sendMsg[4] = 0x00;// 
                    sendMsg[5] = ccl_send_cmd_msg;//保存上位机按下的操作
                    byte[] test = crc.getCRC(sendMsg);
                    HY_sys_CCL ccl = new HY_sys_CCL(test);
                    ccl.parseMsg();
                    hy_server_ccl.sendData(0, sendMsg);
                    ccl_but_status_L = 0;
                }
                else//没有需要发送的命令
                {
                    byte[] sendMsgXy = { 0xAA, 0x55, 0x04, 0xC, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    hy_server_ccl.sendData(sendMsg);
                    return;
                }
            }
           
        }
             */
        /// <summary>
        /// 窗口的关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //readCCL_init_info.Abort();
            readMsg.Abort();
            checkRecvMsg.Abort();
        }

        private void but_ccl_send_MouseDown(object sender, MouseEventArgs e)
        {
            but_ccl_send_cmd.BackColor = Color.Green;
        }

        private void but_ccl_send_MouseUp(object sender, MouseEventArgs e)
        {
            but_ccl_send_cmd.BackColor = SystemColors.ControlLight;
        }

        private void but_debug_Click(object sender, EventArgs e)
        {
            //s_ccl.write
            //通知serial将msg拷贝一份到debug list
            s_ccl.write_to_debug_list = 1;
            //HY_SYS_debug_ccl f = new HY_SYS_debug_ccl();
            //将当前状态传递到子界面。然后在子界面解析！！！
            //f.Show();
        }


        //当前ccl系统的模式。可以配置单模式，双模式，根据模式显示相应的按钮
        public static int ccl_sys_mod = 0;  //默认双系统
        private void button17_Click(object sender, EventArgs e)
        {

        }
        public void ccl_sys_mod_show(int mod)
        {
            if (mod == 1)//单模式
            {
                lb_but_ccl_wc_2.Visible = false;
                lb_but_ccl_sd_2.Visible = false;
                but_ccl_show_2_1.Visible = false;
                but_ccl_show_2_2.Visible = false;
                but_ccl_show_2_3.Visible = false;
                but_ccl_show_2_4.Visible = false;
                but_ccl_show_2_5.Visible = false;
                but_ccl_show_2_6.Visible = false;
                but_ccl_show_2_7.Visible = false;
                but_ccl_show_2_8.Visible = false;
                but_ccl_show_2_9.Visible = false;
                but_ccl_show_2_10.Visible = false;
                but_ccl_show_2_11.Visible = false;
            }
            else
            {
                lb_but_ccl_wc_2.Visible = true;
                lb_but_ccl_sd_2.Visible = true;
                but_ccl_show_2_1.Visible = true;
                but_ccl_show_2_2.Visible = true;
                but_ccl_show_2_3.Visible = true;
                but_ccl_show_2_4.Visible = true;
                but_ccl_show_2_5.Visible = true;
                but_ccl_show_2_6.Visible = true;
                but_ccl_show_2_7.Visible = true;
                but_ccl_show_2_8.Visible = true;
                but_ccl_show_2_9.Visible = true;
                but_ccl_show_2_10.Visible = true;
                but_ccl_show_2_11.Visible = true;
            }
            
        }

        private void button18_Click(object sender, EventArgs e)
        {
            配置用户信息 u = new 配置用户信息();
            u.Show();
        }

        private void but_bjq_mouse_Down(object sender, MouseEventArgs e)
        {
            Button but = (Button)sender;
            //but.BackColor = Color.Yellow;
            int index = but_cmd_bjq.IndexOf((Button)sender);
            byte[] sendMsg = { 0xAA, 0x55, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            sendMsg[5] = 0x05;
            if (index == 0)
            {
                sendMsg[6] = 0x01;
            }
            if (index == 3)
            {
                sendMsg[6] = 0x02;
            }
            HY_crc crc = new HY_crc();
            byte[] test = crc.getCRC(sendMsg);
            if(connect_mode == 1) hy_server_bjq.sendData(sendMsg);
            else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg);
            //send Cmd
        }

        private void but_bjq_mouse_up(object sender, MouseEventArgs e)
        {
            Button but = (Button)sender;
            //but.BackColor = SystemColors.ControlLight;
            byte[] sendMsg = { 0xAA, 0x55, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            sendMsg[5] = 0x05;
            sendMsg[6] = 0;
            HY_crc crc = new HY_crc();
            byte[] test = crc.getCRC(sendMsg);
            if (connect_mode == 1) hy_server_bjq.sendData(sendMsg);
            else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg);
            //功能需要验证
            //throw new NotImplementedException();
        }


        /// <summary>
        /// 通用按钮的状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_ccl_TY_click(object sender, EventArgs e)
        {
            Button but = (Button)sender;
            if (but_press_delay_bjq == 0)
            {
                but_dealy_counter_bjq = 0;
                return;
            }
            but_press_delay_bjq = 0;
            int index = but_cmd_bjq.IndexOf((Button)sender);
            Console.WriteLine("index = " + index);
            byte[] sendMsg = { 0xAA, 0x55, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            if (index == 1)
            {
                if (HY_SYS_main_bjq.but_bjq_cy_status == 0)//船员通用
                {
                    sendMsg[5] = 0x03;
                    sendMsg[6] = 0x01;
                    //but.BackColor = Color.Green;
                    HY_SYS_main_bjq.but_bjq_cy_status = 1;
                    //TODO  send cmd
                }
                else if (HY_SYS_main_bjq.but_bjq_cy_status == 1)
                {
                    sendMsg[5] = 0x03;
                    if (HY_SYS_main_bjq.bjq_status_lkty == 1)
                    {
                        sendMsg[6] = 0x02;
                    }
                    else
                        sendMsg[6] = 0x00;
                    byte[] sendMsg1 = { 0xAA, 0x55, 0x02, 0x02, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    if (connect_mode == 1) hy_server_bjq.sendData(sendMsg1);
                    else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg1);
                    Thread.Sleep(100);
                    if (connect_mode == 1) hy_server_bjq.sendData(sendMsg1);
                    else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg1);
                    Thread.Sleep(100);
                    if (connect_mode == 1) hy_server_bjq.sendData(sendMsg1);
                    else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg1);
                    //but.BackColor = SystemColors.ControlLight;
                    HY_SYS_main_bjq.but_bjq_cy_status = 0;
                }
            }
            if (index == 2)//旅客通用
            {
                if (HY_SYS_main_bjq.but_bjq_lk_status == 0)
                {
                    sendMsg[5] = 0x03;
                    sendMsg[6] = 0x02;
                    //but.BackColor = Color.Green;
                    HY_SYS_main_bjq.but_bjq_lk_status = 1;
                    //TODO  send cmd
                }
                else if (HY_SYS_main_bjq.but_bjq_lk_status == 1)
                {
                    sendMsg[5] = 0x03;
                    if (HY_SYS_main_bjq.bjq_status_cyty == 1)
                    {
                        sendMsg[6] = 0x01;
                    }
                    else
                        sendMsg[6] = 0x00;

                    byte[] sendMsg1 = { 0xAA, 0x55, 0x02, 0x02, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    if (connect_mode == 1) hy_server_bjq.sendData(sendMsg1);
                    else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg1);
                    Thread.Sleep(100);
                    if (connect_mode == 1) hy_server_bjq.sendData(sendMsg1);
                    else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg1);
                    Thread.Sleep(100);
                    if (connect_mode == 1) hy_server_bjq.sendData(sendMsg1);
                    else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg1);
                    //but.BackColor = SystemColors.ControlLight;
                    HY_SYS_main_bjq.but_bjq_lk_status = 0;
                }
            }
            //printMsg(sendMsg);
            if (connect_mode == 1) hy_server_bjq.sendData(sendMsg);
            else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg);
            Thread.Sleep(100);
            if (connect_mode == 1) hy_server_bjq.sendData(sendMsg);
            else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg);
            Thread.Sleep(100);
            if (connect_mode == 1) hy_server_bjq.sendData(sendMsg);
            else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg);
        }

        private void but_bjq_function_cmd_click(object sender, EventArgs e)
        {
            //复位或者应答的按钮事件
            Button but = (Button)sender;
            int index = but_cmd_bjq.IndexOf((Button)sender);
            byte[] sendMsg = { 0xAA, 0x55, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            if (index == 4)
            {
                sendMsg[4] = 0x10;
            }
            else if (index == 5)
            {
                sendMsg[4] = 0x20;
            }
            sendMsg[5] = HY_SYS_main_bjq.last_status_cmd2;
            sendMsg[6] = HY_SYS_main_bjq.last_status_cmd3;

            if (connect_mode == 1) hy_server_bjq.sendData(sendMsg);
            else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg);

            but_bjq_cmd_2.BackColor = Color.Transparent;
            but_bjq_cmd_3.BackColor = Color.Transparent;
            if (but_fliker.Contains(but_show_bjq[5]))
            {
                but_fliker.Remove(but_show_bjq[5]);
                but_show_bjq[5].ButtonColor = Color.Red;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] sendMsg = { 0xAA, 0x55, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            sendMsg[5] = 0x03;
            sendMsg[6] = 0x00;
            if (connect_mode == 1) hy_server_bjq.sendData(sendMsg);
            else if (connect_mode == 2) hy_client_bjq.sendData(sendMsg);

        }
        //紧急
        static public byte[] JBqsendMsg = { 0xAA, 0x55, 0x05, 0x20, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00 };
        //消音
        
        private void but_cmd_hzb_xy_Click(object sender, EventArgs e)
        {
            byte[] sendMsg = { 0xAA, 0x55, 0x05, 0x20, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            /*for (int i = 0; i < 4; i++)
            {
                if (but_fliker.Contains(but_show_hzb[i + 6]))
                {
                    but_fliker.Remove(but_show_hzb[i + 6]);
                }
                but_show_hzb[i + 6].ButtonColor = SystemColors.ControlLight;
            }*/
            //Array.Copy(sendMsg,0, JBqsendMsg,0,11);
            //s_hzb.sendData(0, sendMsg);
            if(connect_mode == 1) hy_server_hzb.sendData(0, sendMsg);
            else if (connect_mode == 2) hy_client_hzb.sendData(0, sendMsg);
            if (but_fliker.Contains(but_show_hzb[5]))
            {
                HY_SYS_main_hzb.hzb_sys_is_xy = 1;
                but_fliker.Remove(but_show_hzb[5]);
                but_show_hzb[5].ButtonColor = Color.Gold;
            }
        }
        public int sys_hzb_jj_tick = 0;
        private void but_cmd_hzb_jj_MouseDown(object sender, MouseEventArgs e)
        {
            Button but = (Button)sender;
            but.BackColor = Color.FromArgb(255, 255, 0, 0);
            timer_hzb_jj.Start();
            sys_hzb_jj_tick = 0;
        }

        private void but_cmd_hzb_jj_MouseUp(object sender, MouseEventArgs e)
        {
            Button but = (Button)sender;
            but.BackColor = SystemColors.ControlLight;
            timer_hzb_jj.Stop();
        }
        
        private void timer_hzb_jj_Tick(object sender, EventArgs e) //紧急报警需要按3S才生效
        {
            byte[] sendMsg = { 0xAA, 0x55, 0x05, 0x20, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00 };
            sys_hzb_jj_tick++;
            if (sys_hzb_jj_tick >= 30)
            {
                if (sys_hzb_jj_tick % 10 == 0)
                {
                    //todo 发送命令。需要收到0x20的命令后发送
                    Array.Copy(sendMsg, 0, JBqsendMsg, 0, 11);
                    if (connect_mode == 1) hy_server_hzb.sendData(0, sendMsg);
                    else if (connect_mode == 2) hy_client_hzb.sendData(0, sendMsg);
                }

            }
            
        }

        //配置图的事件
        public int but_edit_mod = 0; //0 显示模式，1编辑模式
        private void but_edit_pic_Click(object sender, EventArgs e)
        {
            if (but_edit_mod == 0)//进入编辑模式。
            {
                picClick = 0;//0 开启操作
                but_edit_mod = 1;
                this.but_edit_pic1.Text = "保存配置";
                butChose_pic1.Visible = true;
                panel_pic.Enabled = true;
                foreach (PictureBox pb in listPic)
                {
                    pb.Visible = true;
                }
            }
            else//进入保存模式
            {
                picClick = 1;//0 关闭操作
                but_edit_mod = 0;
                this.but_edit_pic1.Text = "编辑全景图";
                //隐藏所有编辑的图片
                foreach (PictureBox pb in listPic)
                {
                    pb.Visible = false;
                }
                butChose_pic1.Visible = false;
                panel_pic.Enabled = false;
                //将图标,全景图保存到配置文件
                //1.保存全景图
                    savefullPicCfg(fullPicCfgName,fullPicPath);
                //2.保存图标
                if (listSysPic.Count != 0)
                {
                    string allSysPic = "";
                    foreach (SysPicInfo sysInfo in listSysPic)
                    {
                        allSysPic += sysInfo.sys+","+ sysInfo.pb.Location.X + "," + sysInfo.pb.Location.Y + "," + sysInfo.path+"\n";
                    }
                    savefullPicCfg(sysPicCfgName, allSysPic);
                }
            }
        }
        public void savefullPicCfg(string cfgPath, string info)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(cfgPath, false);
            //保存数据到文件
            file.WriteLine(info);
            //关闭文件
            file.Close();
            //释放对象
            file.Dispose();
        }
        public void loadCfg()
        {
            try
            {
                string line;
                // 创建一个 StreamReader 的实例来读取文件 ,using 语句也能关闭 StreamReader
                using (System.IO.StreamReader sr = new System.IO.StreamReader(fullPicCfgName))
                {
                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        line.Trim().Replace("\r\n", "");
                        //Console.WriteLine(line);
                        if (System.IO.File.Exists(line))
                        {
                            panel_pic.BackgroundImage = Image.FromFile(line);
                            fullPicPath = line;
                        }
                        
                    }
                }
                using (System.IO.StreamReader sr = new System.IO.StreamReader(sysPicCfgName))
                {
                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        line.Trim().Replace("\r\n", "");
                        string[] pbInfo = line.Split(',');
                        //Console.WriteLine(line);
                        if (pbInfo.Length == 4)
                        {
                            if (System.IO.File.Exists(pbInfo[3]))
                            {
                                PictureBox pb = new PictureBox();
                                Point p = new Point();

                                p.X = int.Parse(pbInfo[1]);
                                p.Y = int.Parse(pbInfo[2]);
                                pb.Height = 50;
                                pb.Width = 50;
                                pb.Location = p;
                                pb.BackgroundImageLayout = ImageLayout.Zoom;
                                pb.BackgroundImage = Image.FromFile(pbInfo[3]);
                                pb.Click += new EventHandler(newPic_click);
                                panel_pic.Controls.Add(pb);
                                pb.Visible = false;
                                listPic.Add(pb);
                                SysPicInfo sp = new SysPicInfo();
                                sp.pb = pb;
                                sp.sys = int.Parse(pbInfo[0]);
                                sp.path = pbInfo[3];
                                listSysPic.Add(sp);

                            }
                        }
                        

                    }
                }
            }
            catch (Exception e)
            {
            }
        }
        private void butChose_pic_Click(object sender, EventArgs e)
        {
            string fileName;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;
                panel_pic.BackgroundImage = Image.FromFile(fileName);
                fullPicPath = fileName;
            }
        }
        Point clickPoint;
        private void getQjChoseData(int sys, int subSys, string fileName)
        {
            foreach (SysPicInfo sp in listSysPic)//如果已经有系统则删除该系统
            {
                if (sp.sys == sys)
                {
                    listSysPic.Remove(sp);
                    break;
                }
            }
            PictureBox pb = new PictureBox();
            clickPoint.X -= 25;
            clickPoint.Y -= 25;
            pb.Location = clickPoint;
            pb.Height = 50;
            pb.Width = 50;
            pb.BackgroundImageLayout = ImageLayout.Zoom;
            pb.BackgroundImage = Image.FromFile(fileName);
            pb.Click += new EventHandler(newPic_click);
            panel_pic.Controls.Add(pb);
            listPic.Add(pb);
            SysPicInfo sysPic = new SysPicInfo();
            sysPic.pb = pb;
            sysPic.path = fileName;
            sysPic.sys = sys;
            listSysPic.Add(sysPic);
        }
        private void newPic_click(object sender, EventArgs e)
        {

            panel_pic.Controls.Remove((PictureBox)sender);
            if (listPic.Contains((PictureBox)sender))
                listPic.Remove((PictureBox)sender);
            //foreach (SysPicInfo sysPic in listSysPic)
            for (int i = 0;i < listSysPic.Count;i++) 
            {
                if (listSysPic[i].pb == (PictureBox)sender)
                    listSysPic.Remove(listSysPic[i]);
            }
        }
        public static int picClick = 1;
        public Point panelP;
        public int  panelW = 0;
        public int panelH = 0;
        private void panel_pic_MouseClick(object sender, MouseEventArgs e)
        {
            if (picClick == 0)//编辑模式
            {
                picClick = 1;
                clickPoint = e.Location;
                qjChose q = new qjChose();
                //q.StartPosition = FormStartPosition.WindowsDefaultLocation;
                q.StartPosition = FormStartPosition.CenterScreen;
                q.ShowUpdate += new qjChose.DisplayUpdateDelegate(getQjChoseData);
                q.Show();
            }
            else //非编辑模式。最大化
            {
                /* PictureBox p = (PictureBox)sender;
                 if (p.Dock == DockStyle.Bottom)
                 {
                     panel1.Dock = DockStyle.Fill;
                     panelP = panel1.Location;
                     panelW = panel1.Width;
                     panelH = panel1.Height;
                     p.Anchor = AnchorStyles.None;
                     p.Dock = DockStyle.Fill;
                 }
                 else
                 {
                     //panel1.Dock = DockStyle.Right;
                     Point pp = new Point(1332, 0);
                     panel1.Location = pp;//panelP;
                     panel1.Width = panelW;
                     panel1.Height = panelH;
                     //panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left| AnchorStyles.Bottom | AnchorStyles.Right;
                     p.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                     p.Dock = DockStyle.Bottom;
                 }*/
            }


        }


        public int test_jb = 0;
        private void button3_Click(object sender, EventArgs e)
        {
            byte[] debugMsg = { 0xAA,0x55,0x03,0x00, 0x00, 0x03, 0x00, 0x00, 0xff, 0x00, 0x00};
            byte[] debugMsg2 = { 0xAA, 0x55, 0x03, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00 };
            if (test_jb == 0)
            {
                byte[] sendMsg = s_jb.getCRC(debugMsg);
                s_jb.addMsgToQueue(sendMsg);
                test_jb = 1;
            }
            else
            {
                byte[] sendMsg = s_jb.getCRC(debugMsg2);
                s_jb.addMsgToQueue(sendMsg);
                test_jb = 0;
            }
            
            
        }
        /// <summary>
        /// jb系统的cmd按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jb_but_cmd_1_Click(object sender, EventArgs e)
        {
            byte[] cmdMsg = { 0xAA, 0x55, 0x03, 0x00, 0x81, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int index = but_cmd_jb.IndexOf((Button)sender);
            cmdMsg[4] = (byte)(0x81 + (byte)index);
            if(connect_mode == 1) hy_server_jb.sendData(cmdMsg);
            else if (connect_mode == 2) hy_client_jb.sendData(cmdMsg);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //byte[] cmdMsg = { 0xAA, 0x55, 0x01, 0x02, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00 };
            byte[] cmdMsg = { 0xAA, 0x55, 0x01, 0x02, 0x00, 0xD, 0xF1, 0x00, 0x00, 0x00, 0x00 };
            byte[] sendMsg = s_jb.getCRC(cmdMsg);
            s_cky.addMsgToQueue(sendMsg);
        }

        private void tb_sys_log_TextChanged(object sender, EventArgs e)
        {
            //tb_sys_log.SelectionStart = tb_sys_log.Text.Length; //Set the current caret position at the end
            //tb_sys_log.ScrollToCaret(); //Now scroll it automatically
        }
        /// <summary>
        /// 显示系统时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_time_Tick(object sender, EventArgs e)
        {
            label_sys_time.Text = "当前时间:" + DateTime.Now.ToLocalTime().ToString();
        }

        /// <summary>
        /// cky的按键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void but_cky_cmd_7_Click(object sender, EventArgs e)
        {
            //HY_SYS_main_cky cky = new HY_SYS_main_cky();
            byte[] cmdMsg = { 0xAA, 0x55, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00 };
            //byte[] sendMsg = s_jb.getCRC(cmdMsg);
            //s_cky.addMsgToQueue(sendMsg);
            int index = but_cmd_cky.IndexOf((Button)sender);
            if (index == 0)//复位按钮
            {
                cmdMsg[4] = (byte)(HY_SYS_main_cky.cky_cmd1 | (1 << 3));
            }
            if (index == 1)//消音按钮
            {
                cmdMsg[4] = (byte)(HY_SYS_main_cky.cky_cmd1 | (1 << 2));
            }
            if (index > 1)
            {
                cmdMsg[4] = HY_SYS_main_cky.cky_cmd1;
                cmdMsg[5] = 0x0D;
                HY_SYS_main_cky.cky_cmd3 = (byte)(HY_SYS_main_cky.cky_cmd3 ^ 1 << (index - 2));
                cmdMsg[6] = HY_SYS_main_cky.cky_cmd3;
            }

            if(connect_mode == 1) hy_server_cky.sendData(cmdMsg);
            else if (connect_mode == 2) hy_client_cky.sendData(cmdMsg);
            
        }
        public void initTable()
        {


            List<Jb_data>[] list_jb_screen;//警报在左边显示的数据链表
            Label[] jbLabelShow;//警报显示状态的label
            int[] jbsShowIndex;//每个label当前显示的信息
            //label
            jbLabelShow = new Label[4];
            jbLabelShow[0] = label_jb_hj;
            jbLabelShow[1] = label_jb_ld;
            jbLabelShow[2] = label_jb_pb;
            jbLabelShow[3] = label_jb_gz;
            //当前显示的索引
            jbsShowIndex = new int[4];
            //消息链表
            list_jb_screen = new List<Jb_data>[4];
            for (int i = 0; i < 4; i++)
            {
                list_jb_screen[i] = new List<Jb_data>();
            }
        }
        //火警的左右箭头事件
        private void tabControl1_Click(object sender, EventArgs e)
        {
            //int index = but_cmd_jb.IndexOf((Button)sender);
            int index = jb_tab_screen.SelectedIndex;
            if ((Button)sender == jb_but_cmd_7)
            {
                
                if (jbsShowIndex[index] == 0)
                    jbsShowIndex[index] = list_jb_screen[index].Count - 1;
                else
                    jbsShowIndex[index]--;
                
            }
            else if((Button)sender == jb_but_cmd_8)
            {
                if (jbsShowIndex[index] == list_jb_screen[index].Count - 1)
                    jbsShowIndex[index] = 0;
                else
                    jbsShowIndex[index]++;
            }
            showPageMassage(index, jbsShowIndex[index]);
        }
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
        /// temp 可删除，测试jb
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        private void button2_Click_1(object sender, EventArgs e)
        {
            /*byte[] sendMsg = { 0xAA, 0x55, 0x03, 0x00, 0x07, 0x02, 0x03, 0x04, 0x00, 0x00, 0x00 };
            HY_crc crc = new HY_crc();
            byte[] test = crc.getCRC(sendMsg);*/
            s_hzb.writeTest();
        }
        public void setButtonBackColor()
        {
            //设置鼠标按下的背景色 ccl
            this.but_cmd_ccl_1_1.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_1.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_1_2.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_2.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_1_3.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_3.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_1_4.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_4.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_1_5.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_5.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_1_6.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_6.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_1_7.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_7.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_1_8.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_8.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_1_9.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_9.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_1_10.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_10.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_1_11.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_1_11.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下

            this.but_cmd_ccl_2_1.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_1.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_2_2.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_2.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_2_3.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_3.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_2_4.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_4.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_2_5.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_5.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_2_6.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_6.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_2_7.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_7.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_2_8.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_8.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_2_9.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_9.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_2_10.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_10.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_cmd_ccl_2_11.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_ccl_2_11.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下

            this.but_ccl_send_cmd.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_ccl_send_cmd.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
                                                                                        //设置鼠标按下的背景色 hzb

            this.but_cmd_hzb_jj.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_hzb_jj.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下

            this.but_cmd_hzb_xy.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cmd_hzb_xy.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
                                                                                      //设置鼠标按下的背景色 hzb

            this.but_bjq_cmd_1.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_bjq_cmd_1.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_bjq_cmd_2.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 0, 0, 255);//鼠标按下
            this.but_bjq_cmd_2.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_bjq_cmd_3.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 0, 0, 255);//鼠标按下
            this.but_bjq_cmd_3.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_bjq_cmd_4.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_bjq_cmd_4.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_bjq_cmd_5.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_bjq_cmd_5.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.but_bjq_cmd_6.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_bjq_cmd_6.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下

            this.jb_but_cmd_send_1.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.jb_but_cmd_send_1.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.jb_but_cmd_send_2.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.jb_but_cmd_send_2.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.jb_but_cmd_send_3.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.jb_but_cmd_send_3.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.jb_but_cmd_send_4.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.jb_but_cmd_send_4.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.jb_but_cmd_send_5.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.jb_but_cmd_send_5.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.jb_but_cmd_send_6.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.jb_but_cmd_send_6.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.jb_but_cmd_send_7.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.jb_but_cmd_send_7.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.jb_but_cmd_send_8.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.jb_but_cmd_send_8.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下



            this.but_cky_cmd_0.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.but_cky_cmd_0.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.jb_but_cmd_7.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.jb_but_cmd_7.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
            this.jb_but_cmd_8.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 255, 255, 255);//鼠标按下
            this.jb_but_cmd_8.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标按下
        }

        private void but_hzb_show_1_4_Click(object sender, EventArgs e)
        {
            //隐藏功能，清除buffer
            Console.WriteLine("=================clear==================");
            s_hzb.clearAllMsg();
            s_ccl.clearAllMsg();
            s_bjq.clearAllMsg();
            s_jb.clearAllMsg();
            s_cky.clearAllMsg();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            byte[] sendMsg = { 0xaa,0x55,0x04,0x0c,0xa8,00,0x21,0x00,0x00,0x00,0x00};
            HY_crc crc = new HY_crc();
            byte[] test = crc.getCRC(sendMsg);
            hy_PC.addMsgToQueue(sendMsg);
        }
        private void button1_Click(object sender, EventArgs e)
        {

            //AA 55 04 0C 8A 15 28 D2 E2 6A CD
            byte[] sendMsg = { 0xaa, 0x55, 0x04, 0x07, 0xab, 0x00, 0x23, 0x00, 0x00, 0x00, 0x00 };
            //byte[] test1  = strToToHexByte(textBox1.Text); 
            HY_crc crc = new HY_crc();
            byte[] test = crc.getCRC(sendMsg);
            printMsg(sendMsg);
            hy_PC.addMsgToQueue(sendMsg);
            //功能需要验证
            //throw new NotImplementedException();
        }

        private void panel_click_showPanel(object sender, EventArgs e)
        {
            if ((Panel)sender == panel_but_ccl)
            {

                panel_CCL.Visible = true;
                panel_HZB.Visible = false;
                panel_BJQ.Visible = false;
                panel_CKY.Visible = false;
                panel_JB.Visible = false;
            }
            else if ((Panel)sender == panel_but_hzb)
            {
                panel_CCL.Visible = false;
                panel_HZB.Visible = true;
                panel_BJQ.Visible = false;
                panel_CKY.Visible = false;
                panel_JB.Visible = false;

            }
            else if ((Panel)sender == panel_but_cky)
            {
                panel_CCL.Visible = false;
                panel_HZB.Visible = false;
                panel_BJQ.Visible = false;
                panel_CKY.Visible = true;
                panel_JB.Visible = false;

            }
            else if ((Panel)sender == panel_but_bjq)
            {
                panel_CCL.Visible = false;
                panel_HZB.Visible = false;
                panel_BJQ.Visible = true;
                panel_CKY.Visible = false;
                panel_JB.Visible = false;

            }
            else if ((Panel)sender == panel_but_jb)
            {
                panel_CCL.Visible = false;
                panel_HZB.Visible = false;
                panel_BJQ.Visible = false;
                panel_CKY.Visible = false;
                panel_JB.Visible = true;

            }
        }

        private void panel_main_left_down(object sender, MouseEventArgs e)
        {
            ((Panel)sender).BackgroundImage = Properties.Resources.left_main_bgU;
        }

        private void panel_main_left_up(object sender, MouseEventArgs e)
        {
            ((Panel)sender).BackgroundImage = Properties.Resources.left_main_bgD;
        }

        private void label_left_click(object sender, EventArgs e)
        {
            if ((Label)sender == label_left_ccl)
            {

                panel_CCL.Visible = true;
                panel_HZB.Visible = false;
                panel_BJQ.Visible = false;
                panel_CKY.Visible = false;
                panel_JB.Visible = false;
            }
            else if ((Label)sender == label_left_hzb)
            {
                panel_CCL.Visible = false;
                panel_HZB.Visible = true;
                panel_BJQ.Visible = false;
                panel_CKY.Visible = false;
                panel_JB.Visible = false;

            }
            else if ((Label)sender == label_left_cky)
            {
                panel_CCL.Visible = false;
                panel_HZB.Visible = false;
                panel_BJQ.Visible = false;
                panel_CKY.Visible = true;
                panel_JB.Visible = false;

            }
            else if ((Label)sender == label_left_bjq)
            {
                panel_CCL.Visible = false;
                panel_HZB.Visible = false;
                panel_BJQ.Visible = true;
                panel_CKY.Visible = false;
                panel_JB.Visible = false;

            }
            else if ((Label)sender == label_left_jb)
            {
                panel_CCL.Visible = false;
                panel_HZB.Visible = false;
                panel_BJQ.Visible = false;
                panel_CKY.Visible = false;
                panel_JB.Visible = true;

            }
        }

        private void label_left_D(object sender, MouseEventArgs e)
        {
            ((Label)sender).Parent.BackgroundImage = Properties.Resources.left_main_bgU;
        }

        private void label_left_U(object sender, MouseEventArgs e)
        {
            ((Label)sender).Parent.BackgroundImage = Properties.Resources.left_main_bgD;
        }



        private void but_bjq_mouse_cmd_down(object sender, MouseEventArgs e)
        {
            Button but = (Button)sender;
            but.Image = Properties.Resources.BJQ_yd;
        }

        private void but_bjq_mouse_cmd_up(object sender, MouseEventArgs e)
        {
            Button but = (Button)sender;
            but.Image = Properties.Resources.BJQ_yd_2;
        }

        private void but_hzb_cmd_xy_down(object sender, MouseEventArgs e)
        {
            Button but = (Button)sender;
            but.BackColor = Color.FromArgb(255,255,0,0);
        }

        private void but_hzb_cmd_xy_up(object sender, MouseEventArgs e)
        {
            Button but = (Button)sender;
            but.BackColor = SystemColors.ControlLight;
        }

        private void button1_Click_2(object sender, EventArgs e)
        {

        }

        private void timer_but_delay_Tick(object sender, EventArgs e)
        {

            if (but_dealy_counter_ccl++ == 20)
            {
                but_press_delay_ccl = 1;
                but_dealy_counter_ccl = 0;
            }
            if (but_dealy_counter_bjq++ == 10)
            {
                but_press_delay_bjq = 1;
                but_dealy_counter_bjq = 0;
            }


        }
    }

}

