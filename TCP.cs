using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using parking.数据库操作;

namespace parking
{
    class TCP
    {
        public delegate void showData(string msg);//委托,防止跨线程的访问控件，引起的安全异常
        private const int bufferSize = 8000;//缓存空间
        private TcpClient client;
        private TcpListener server;
        private databaseOperation ope;

        /// <summary>
        /// 结构体：Ip、端口
        /// </summary>
        struct IpAndPort
        {
            public string Ip;
            public string Port;
        }

        /// <summary>
        /// 开始侦听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void start(string ip,string port)
        {
            Thread thread = new Thread(reciveAndListener);
            //如果线程绑定的方法带有参数的话，那么这个参数的类型必须是object类型，所以讲ip,和端口号 写成一个结构体进行传递
            IpAndPort ipHePort = new IpAndPort();
            ipHePort.Ip = ip;
            ipHePort.Port = port;

            thread.Start((object)ipHePort);
        }

        /// <summary>
        /// 发送信息给客户端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void send(string sendmessage)
        {
            NetworkStream sendStream = null;
            sendStream=client.GetStream();//获得用于数据传输的流
            byte[] buffer = Encoding.Default.GetBytes(sendmessage.Trim());//将数据存进缓存中
            sendStream.Write(buffer, 0, buffer.Length);//最终写入流中
        }

        /// <summary>
        /// 侦听客户端的连接并接收客户端发送的信息
        /// </summary>
        /// <param name="ipAndPort">服务端Ip、侦听端口</param>
        private void reciveAndListener(object ipAndPort)
        {
            IpAndPort ipHePort = (IpAndPort)ipAndPort;

            IPAddress ip = IPAddress.Parse(ipHePort.Ip);
            server = new TcpListener(ip, int.Parse(ipHePort.Port));
            server.Start();//启动监听
            //rtbtxtShowData.Dispatcher.Invoke(new showData(rtbtxtShowData.AppendText), "服务端开启侦听....\n");
            //  btnStart.IsEnabled = false;

            //获取连接的客户端对象
            client = server.AcceptTcpClient();
            //rtbtxtShowData.Dispatcher.Invoke(new showData(rtbtxtShowData.AppendText), "有客户端请求连接，连接已建立！");//AcceptTcpClient 是同步方法，会阻塞进程，得到连接对象后才会执行这一步  

            //获得流
            NetworkStream reciveStream = client.GetStream();

            #region 循环监听客户端发来的信息

            do
            {
                byte[] buffer = new byte[bufferSize];
                int msgSize;
                try
                {
                    lock (reciveStream)
                    {
                        msgSize = reciveStream.Read(buffer, 0, bufferSize);
                    }
                    if (msgSize == 0)
                        return;
                    string msg = Encoding.Default.GetString(buffer, 0, bufferSize);
                    ope = new databaseOperation();
                    char[] msg1 = msg.ToCharArray();
                    if (msg1[2] == '1')
                    {
                        ope.modifyDatabase("update parkingPlace set property = 1, plateNumber = '京AF0236', parkingTime = CURRENT_TIMESTAMP  where ppid = 2");
                        //ope.modifyDatabase("delete from appointment where plateNumber='京AF0236'");
                    }
                    else
                    {
                        ope.modifyDatabase("update parkingPlace set property = 0, plateNumber = NULL, parkingTime = NULL where ppid = 2");
                    }
                    //rtbtxtShowData.Dispatcher.Invoke(new showData(rtbtxtShowData.AppendText), "\n客户端曰：" + Encoding.Default.GetString(buffer, 0, msgSize));
                }
                catch
                {
                    //rtbtxtShowData.Dispatcher.Invoke(new showData(rtbtxtShowData.AppendText), "\n 出现异常：连接被迫关闭");
                    break;
                }
            } while (true);

            #endregion
        }
    }
}
