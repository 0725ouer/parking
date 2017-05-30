using System;
using System.Windows.Forms;
using parking.数据库操作;
using parking.窗体自适应;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Sockets;

namespace parking.界面
{
    public partial class main : Form
    {
        private databaseOperation ope;
        private databaseOperation ope1;
        private databaseOperation ope2;
        private databaseOperation ope3;
        private databaseOperation ope4;
        private databaseOperation ope5;
        private databaseOperation ope6;
        private databaseOperation ope7;
        private databaseOperation ope8;
        private databaseOperation ope9;
        private TCP tcp;
        private string num_1, num_2, num_3;   //用来比较上一次数值，以判定表格是否刷新

        AutoSizeFormClass asc = new AutoSizeFormClass();
        public main()
        {
            InitializeComponent();
            Form.CheckForIllegalCrossThreadCalls = false;
            ope = new databaseOperation();
            ope1 = new databaseOperation();
            ope2 = new databaseOperation();
            ope3 = new databaseOperation();
            ope4 = new databaseOperation();
            ope5 = new databaseOperation();
            ope6 = new databaseOperation();
            ope7 = new databaseOperation();
            ope8 = new databaseOperation();
            ope9 = new databaseOperation();
            this.StartPosition = FormStartPosition.CenterScreen;
            CheckForIllegalCrossThreadCalls = false;
            textBox1.Text = "192.168.0.98";
            textBox2.Text = null;
            button2.Enabled = false;
            tcp = new TCP();
            tcp.start("192.168.16.100", "8080");
        }
        SerialPort Myport = null;
        private void main_FormClosing(object sender, FormClosingEventArgs e)      //login退出提示
        {
            if (DialogResult.OK == MessageBox.Show("你确定要关闭应用吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
            {
                this.FormClosing -= new FormClosingEventHandler(this.main_FormClosing);//为保证Application.Exit();时不再弹出提示，所以将FormClosing事件取消
                Application.Exit();//退出整个应用程序
            }
            else
            {
                e.Cancel = true;  //取消关闭事件
            }
        }
        private void main_Load(object sender, EventArgs e)    //login加载,初始化
        {
            string num1 = ope.selectOneValue("select count(*) from parkingPlace where property=0");   //num1为剩余车位数，剩余车位初始化(1界面)
            label_num1.Text = "剩余车位数：" + num1;
            num_1 = num1;
            string num2 = ope1.selectOneValue("select count(*) from appointment"); //num2为预约车位数，预约车位数初始化
            label_num2.Text = "预约车辆数：" + num2;
            num_2 = num2;
            string num3 = ope7.selectOneValue("select count(*) from rentparkingPlace where property=0");   //num3为剩余车位数，剩余车位初始化(2界面)
            label_num3.Text = "剩余车位数：" + num3;
            num_3 = num3;

            ope2.da = new MySqlDataAdapter("select ppid, plateNumber,parkingTime from parkingPlace;",ope2.conn);
            ope2.da.Fill(ope2.ds);
            dataGridView2.DataSource = ope2.ds.Tables[0];

            ope3.da = new MySqlDataAdapter("select plateNumber, appTime from appointment;",ope3.conn);
            ope3.da.Fill(ope3.ds);
            dataGridView1.DataSource = ope3.ds.Tables[0];

            ope6.da = new MySqlDataAdapter(    //车位信息表初始化，dataGridView3为租出停车场信息表
                "select ppid, plateNumber,parkingTime from rentparkingPlace;", ope6.conn);
            ope6.da.Fill(ope6.ds);
            dataGridView3.DataSource = ope6.ds.Tables[0];

            //串口初始化
            label_com.Text = "无串口打开";
            txtSendView.Text = null;
            //实例化
            Myport = new SerialPort();
            //这里需要添加引用Microsoft.VisualBasic的引用，提供操作计算机组件（如：音频，时钟，键盘文件系统等）的属性
            Microsoft.VisualBasic.Devices.Computer pc = new Microsoft.VisualBasic.Devices.Computer();
            //循环该计算机上所有串行端口的集合
            foreach (string s in pc.Ports.SerialPortNames)
            {
                //串口名称添加到cbxPortName下拉框上
                //一般计算机上是有COM1和COM2串口的，如果没有自己在cbxPortName下拉框里写COM1 和 COM2的字符串(如：this.cbxPortName.Items.Add("COM2"))
                this.cbxPortName.Items.Add(s);
            }
            //防止报错，万一计算机上没有串口，就不走这一步
            if (pc.Ports.SerialPortNames.Count > 0)
            {
                cbxPortName.SelectedIndex = 0;
            }
            cmbbaud.SelectedIndex = 0;
            cmbParity.SelectedIndex = 0;
            cmbBits.SelectedIndex = 0;
            cmbStop.SelectedIndex = 0;
            

        }

        private void TplateNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(null, null);
            }
        }
        private void txtSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnNotAutoSend_Click(null, null);
            }
        }

        private void main_layout(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }



        private void timer2_Tick(object sender, EventArgs e)  //定时执行器，0.5s
        {
            string num1 = ope.selectOneValue("select count(*) from parkingPlace where property=0");   //num1为剩余车位数
            label_num1.Text = "剩余车位数：" + num1;
            if (num_1.Equals(num1)) { }   //如果剩余车位数改变，则刷新表格
            else
            {
                num_1 = num1;
                DataTable dt = (DataTable)dataGridView2.DataSource;
                dt.Rows.Clear();
                dataGridView2.DataSource = dt;
                ope2.da = new MySqlDataAdapter(    //dataGridView2为停车场信息表
                "select ppid, plateNumber,parkingTime from parkingPlace;", ope2.conn);
                ope2.da.Fill(ope2.ds);
                dataGridView2.DataSource = ope2.ds.Tables[0];
            }

            string num2 = ope1.selectOneValue("select count(*) from appointment");  //num2为预约车位数
            label_num2.Text = "预约车辆数：" + num2;
            if (num_2.Equals(num2)) { }   //如果预约车辆数目改变，则刷新表格
            else
            {
                num_2 = num2;
                DataTable dt = (DataTable)dataGridView1.DataSource;
                dt.Rows.Clear();
                dataGridView1.DataSource = dt;
                ope3.da = new MySqlDataAdapter(    //dataGridView1为停车场信息表
                "select plateNumber, appTime from appointment;", ope3.conn);
                ope3.da.Fill(ope3.ds);
                dataGridView1.DataSource = ope3.ds.Tables[0];
                //MessageBox.Show("贵F66666预约车位地锁已关闭，可停车", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            string num3 = ope7.selectOneValue("select count(*) from rentparkingPlace where property=0");   //num1为剩余车位数
            label_num3.Text = "剩余车位数：" + num3;
            if (num_3.Equals(num3)) { }   //如果剩余车位数改变，则刷新表格
            else
            {
                num_3 = num3;
                DataTable dt = (DataTable)dataGridView3.DataSource;
                dt.Rows.Clear();
                dataGridView3.DataSource = dt;
                ope6.da = new MySqlDataAdapter(    //dataGridView2为停车场信息表
                "select ppid, plateNumber,parkingTime from rentparkingPlace;", ope6.conn);
                ope6.da.Fill(ope6.ds);
                dataGridView3.DataSource = ope6.ds.Tables[0];
            }
            
        }

        private void exit1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void exit2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!Myport.IsOpen)
            {
                //设置串口端口
                Myport.PortName = cbxPortName.Text;
                //设置比特率
                Myport.BaudRate = Convert.ToInt32(cmbbaud.Text);
                //设置数据位
                Myport.DataBits = Convert.ToInt32(cmbBits.Text);
                //根据选择的数据，设置停止位
                //if (cmbStop.SelectedIndex == 0)
                //    Myport.StopBits = StopBits.None;
                if (cmbStop.SelectedIndex == 1)
                    Myport.StopBits = StopBits.One;
                if (cmbStop.SelectedIndex == 2)
                    Myport.StopBits = StopBits.OnePointFive;
                if (cmbStop.SelectedIndex == 3)
                    Myport.StopBits = StopBits.Two;

                //根据选择的数据，设置奇偶校验位
                if (cmbParity.SelectedIndex == 0)
                    Myport.Parity = Parity.Even;
                if (cmbParity.SelectedIndex == 1)
                    Myport.Parity = Parity.Mark;
                if (cmbParity.SelectedIndex == 2)
                    Myport.Parity = Parity.None;
                if (cmbParity.SelectedIndex == 3)
                    Myport.Parity = Parity.Odd;
                if (cmbParity.SelectedIndex == 4)
                    Myport.Parity = Parity.Space;

                //此委托应该是异步获取数据的触发事件，即是：当有串口有数据传过来时触发
                Myport.DataReceived += new SerialDataReceivedEventHandler(port1_DataReceived);//DataReceived事件委托
                                                                                              //打开串口的方法
                try
                {
                    Myport.Open();
                    if (Myport.IsOpen)
                    {
                        MessageBox.Show("串口已打开", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        label_com.Text = Myport.PortName + "已经打开";
                    }
                    else
                    {
                        MessageBox.Show("无法打开串口", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("无法打开窗口" + ex.ToString());
                }
            }
            else {
                MessageBox.Show("串口已打开，请勿重复打开", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
        }


        ///
        /// 关闭串口的方法
        ///
        public void ClosePort()
        {
            if (!Myport.IsOpen)
            {
                MessageBox.Show("无串口需关闭", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else {
                Myport.Close();
                MessageBox.Show("串口已关闭", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                label_com.Text = "无串口打开";
            }
        }

        private void btnNotAutoSend_Click(object sender, EventArgs e)
        {
            if (Myport.IsOpen)
            {
                SendCommand(txtSend.Text.Trim());
                if (txtSendView.Text.Trim() == null)
                {
                    txtSendView.Text = txtSend.Text.Trim();
                }
                else {
                    txtSendView.Text = txtSendView.Text.Trim() + "\r\n" + txtSend.Text.Trim();
                }
                //MessageBox.Show("发送完成","提示",MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSend.Text = null;
            }
            else
            {
                MessageBox.Show("串口未打开", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
           
        }
        public void SendCommand(string CommandString)
        {
            //转换
            //串口只能读取ASCII码或者进制数（1，2，3.....的进制，一般是16进制）
            byte[] WriteBuffer = Encoding.ASCII.GetBytes(CommandString);
            //将数据缓冲区的数据写入到串口端口
            Myport.Write(WriteBuffer, 0, WriteBuffer.Length);
        }

        private void port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string currentline = "";
                //循环接收串口中的数据
                while (Myport.BytesToRead > 0)
                {
                    char ch = (char)Myport.ReadByte();
                    currentline += ch.ToString();
                }
                //在这里对接收到的数据进行显示
                //如果不在窗体加载的事件里写上：Form.CheckForIllegalCrossThreadCalls = false; 就会报错）
                char[] a = currentline.ToCharArray();
                if (a[2] == '1')
                {
                    ope8.modifyDatabase("update parkingPlace set property = 1, plateNumber = '鲁K48596', parkingTime = CURRENT_TIMESTAMP  where ppid = 2");
                }
                else
                {
                    ope8.modifyDatabase("update parkingPlace set property = 0, plateNumber = '空', parkingTime = NULL where ppid = 2");
                }
                /*if (isNumberic(currentline))
                {
                    int data = 0;
                    data=Convert.ToInt32(currentline);
                    int data_ge = 0;
                    data_ge= data % 10;
                    int data_shi = 0;
                    data_shi= (data - data_ge) / 10;
                    if (data_ge == 0)
                    {
                        ope8.modifyDatabase("update parkingPlace set property = 0, plateNumber = '空', parkingTime = NULL where ppid = '"+data_shi+"'");
                    }
                    else
                    {
                        ope8.modifyDatabase("update parkingPlace set property = 1, plateNumber = '鲁K48596', parkingTime = CURRENT_TIMESTAMP  where ppid = '" + data_shi + "'");
                    }
                }*/
                this.txtReceive.Text = currentline;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }


        protected bool isNumberic(string message)

        {

            if (message != "" && Regex.IsMatch(message, @"^\d{3}$"))

            {

                //成功

                return true;

            }

            else

                //失败

                return false;

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            ClosePort();
        }


        private void btnempty_Click(object sender, EventArgs e)
        {
            txtSendView.Text=null;
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (Myport.IsOpen)
            {
                SendCommand("U");
                MessageBox.Show("地锁已升起", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("串口未打开", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (Myport.IsOpen)
            {
                SendCommand("D");
                MessageBox.Show("地锁已降下", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("串口未打开", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show("贵F66666已完成线上支付，请放行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)   //计算费用按钮
        {
            
            tcp.send("D");

            label_view.Text = null;
            string textPlateNumber = null;
            textPlateNumber = TplateNumber.Text;
            string parkingTime = null;
            parkingTime = ope4.selectOneValue("select parkingTime from time where plateNumber='" + textPlateNumber + "';");
            string gettingTime = null;
            gettingTime = ope5.selectOneValue("select gettingTime from time where plateNumber='" + textPlateNumber + "';");
            TimeSpan time = Convert.ToDateTime(gettingTime) - Convert.ToDateTime(parkingTime);    //计算停车时间
            int cost = 0;
            cost = time.Hours * 2;
            if(time.Minutes<=30)
            {
                cost = cost + 1;
            }
            else
            {
                cost = cost + 2;
            }
            if (parkingTime != null)
            {
                label_view.Text = "本次共停车" + time.Hours.ToString() + "小时" + time.Minutes.ToString() + "分钟" + time.Seconds.ToString() + "秒，共计消费" + cost + "元";   //停车时间和费用计算
            }
            else
            {
                MessageBox.Show("贵F66666已完成线上支付，请放行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void exit3_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        //连接状态
        private WTY.KHTSDK.WTYConnectCallback ConnectCallback = null;

        //JPEG流
        private WTY.KHTSDK.WTYJpegCallback JpegExCallback = null;

        //识别结果
        //private WTY.KHTSDK.WTYDataExCallback DataExCallbcak = null;

        //新的识别结果
        private WTY.KHTSDK.WTYDataEx2Callback DataExCallbcak = null;

        // 定义显示车牌的委托
        public unsafe delegate void delShowPlate(String strPlate, String strColor, Label lbl);

        // 定义显示识别时间的委托
        public unsafe delegate void delShowRecogniseTime(Int32 nYear,
                                        Int32 nMonth,
                                        Int32 nDay,
                                        Int32 nHour,
                                        Int32 nMinute,
                                        Int32 nSecond,
                                        Int32 nMillisecond);

        // 定义显示车牌坐标的委托
        public unsafe delegate void delShowPlateC(Int32 nLeft,
                                Int32 nTop,
                                Int32 nRight,
                                Int32 nBottom);

        //自定义消息ID   
        private const int WM_GETDATA1 = 0x0400 + 101;
        private const int WM_GETDATA2 = 0x0400 + 102;
        public readonly int BIGIMAGE_LEN = WTY.KHTSDK.BIG_PICSTREAM_SIZE;
        public readonly int SMALLIMAGE_LEN = WTY.KHTSDK.SMALL_PICSTREAM_SIZE;
        public readonly int PLATE_LEN = 20;
        public readonly int COLOR_LEN = 5;
        public readonly int IP_LEN = 5;

        bool g_bRecgoRuing = false;

        //WTY.plate_result recRes1;
        //WTY.plate_result recRes2;
        /*************************************7.1.6.0新增*******************************/
        static WTY.plate_result_ex recRes1;
        static WTY.plate_result_ex recRes2;
        bool nCallbackTrigger1 = false;
        bool nCallbackTrigger2 = false;
        string sIp1;
        string sIp2;

        byte[] recRes1FullImg = new byte[WTY.KHTSDK.BIG_PICSTREAM_SIZE_EX];
        byte[] recRes1PlateImg = new byte[WTY.KHTSDK.SMALL_PICSTREAM_SIZE_EX];

        byte[] recRes2FullImg = new byte[WTY.KHTSDK.BIG_PICSTREAM_SIZE_EX];
        byte[] recRes2PlateImg = new byte[WTY.KHTSDK.SMALL_PICSTREAM_SIZE_EX];


        // 将byte[]转成指定的结构体
        public StructType ConverBytesToStructure<StructType>(byte[] bytesBuffer)
        {
            // 检查长度
            if (bytesBuffer.Length != Marshal.SizeOf(typeof(StructType)))
            {
                throw new ArgumentException("bytesBuffer参数和structObject参数字节长度不一致。");
            }

            IntPtr bufferHandler = Marshal.AllocHGlobal(bytesBuffer.Length);
            for (int index = 0; index < bytesBuffer.Length; index++)
            {
                Marshal.WriteByte(bufferHandler, index, bytesBuffer[index]);
            }
            StructType structObject = (StructType)Marshal.PtrToStructure(bufferHandler, typeof(StructType));
            Marshal.FreeHGlobal(bufferHandler);

            return structObject;
        }

        /*************************************7.1.6.0改变的识别结果结构体*******************************/
        //public void pateShow(WTY.plate_result recResult, String fullImgFile, String plateImgFile, PictureBox FullImg, PictureBox PlateImg, Label lbl)

        public void pateShow(WTY.plate_result_ex recResult, String fullImgFile, String plateImgFile, PictureBox FullImg, PictureBox PlateImg, Label lbl)
        {
            string sIp1;
            sIp1 = (new string(recResult.chWTYIP)).TrimEnd('\0');
            string fileNameTime = DateTime.Now.ToString("hh-mm-ss");
            //string fileNameTime = System.DateTime.Now.ToString();
            string directoryPath = recResult.chWTYIP.ToString();
            // 显示车牌
            string strLicesen = new string(recResult.chLicense);
            string strColor = new string(recResult.chColor);
            object[] Dl = {
                              strLicesen,
                              strColor,
                              lbl
                          };
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new delShowPlate(ShowPlate), Dl);
            }


            Directory.CreateDirectory(recResult.chWTYIP.ToString());

            // 显示识别时间
            object[] D2 = {
                              recResult.shootTime.Year,
                              recResult.shootTime.Month,
                              recResult.shootTime.Day,
                              recResult.shootTime.Hour,
                              recResult.shootTime.Minute,
                              recResult.shootTime.Second,
                              recResult.shootTime.Millisecond
                          };
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new delShowRecogniseTime(ShowRecogniseTime), D2);
            }

            // 显示识别坐标
            object[] D3 = {
                             recResult.pcLocation.Left,
                             recResult.pcLocation.Top,
                             recResult.pcLocation.Right,
                             recResult.pcLocation.Bottom
                          };
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new delShowPlateC(ShowPlateC), D3);
            }

            // 显示识别图像
            // if (recResult.nFullLen > 0)   7.1.6.0改变的识别结果结构体成员更改
            if (recResult.pFullImage.nLen > 0)
            {

                // 保存全景图
                //System.IO.FileStream fs = new System.IO.FileStream(fullImgFile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, FileShare.ReadWrite);
                System.IO.FileStream fs = new System.IO.FileStream(directoryPath + fileNameTime + ".jpg", System.IO.FileMode.CreateNew, System.IO.FileAccess.Write, FileShare.ReadWrite);
                try
                {
                    //fs.Write(recResult.chFullImage, 0, recResult.nFullLen);
                    if (String.Compare(sIp1.Trim(), textBox1.Text.ToString().Trim(), true) == 0)
                    {
                        fs.Write(recRes1FullImg, 0, recResult.pFullImage.nLen);
                        fs.Close();
                    }
                    if (String.Compare(sIp1.Trim(), textBox2.Text.ToString().Trim(), true) == 0)
                    {
                        fs.Write(recRes2FullImg, 0, recResult.pFullImage.nLen);
                        fs.Close();
                    }

                    fs.Close();
                }
                catch (Exception ex)
                {
                    fs.Close();
                    //Console.WriteLine(ex.Message);
                }
                // 将全景图显示在界面上
                FullImg.Image = null;
                FileInfo fi = new FileInfo(fullImgFile);
                if (fi.Exists)
                {

                    try
                    {
                        FullImg.Load(fullImgFile);
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.Message);
                    }

                }
            }
            //if (recResult.nPlateLen > 0)7.1.6.0改变的识别结果结构体成员更改
            if (recResult.pPlateImage.nLen > 0)
            {
                // 保存车牌小图
                System.IO.FileStream fs = new System.IO.FileStream(plateImgFile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, FileShare.ReadWrite);
                try
                {
                    if (String.Compare(sIp1.Trim(), textBox1.Text.ToString().Trim(), true) == 0)
                    {
                        fs.Write(recRes1PlateImg, 0, recResult.pPlateImage.nLen);
                    }
                    if (String.Compare(sIp1.Trim(), textBox2.Text.ToString().Trim(), true) == 0)
                    {
                        fs.Write(recRes2PlateImg, 0, recResult.pPlateImage.nLen);
                    }
                    fs.Close();
                }
                catch (Exception ex)
                {
                    fs.Close();
                    Console.WriteLine(ex.Message);
                }

                // 将车牌小图显示在界面上
                PlateImg.Image = null;
                FileInfo fi = new FileInfo(plateImgFile);
                if (fi.Exists)
                {
                    PlateImg.Load(plateImgFile);
                }
            }
        }


        // 根据IP地址判断显示哪个设备识别数据
        public void DeviceSelect()
        {
            string sIp;
            String strFullFile1 = "FullImage1.jpg";
            String strPlateFile1 = "PlateImage1.jpg";
            String strFullFile2 = "FullImage2.jpg";
            String strPlateFile2 = "PlateImage2.jpg";

            while (g_bRecgoRuing == true)
            {
                Thread.Sleep(1);
                if (nCallbackTrigger1 == true)
                {
                    sIp = (new string(recRes1.chWTYIP)).TrimEnd('\0');
                    if (String.Compare(sIp.Trim(), textBox1.Text.ToString().Trim(), true) == 0)
                    {
                        // 显示IP地址1的识别数据
                        pateShow(recRes1, strFullFile1, strPlateFile1, this.pictureBox1, this.pictureBox2, PlateLabel1);
                    }
                    nCallbackTrigger1 = false;
                }
                if (nCallbackTrigger2 == true)
                {
                    sIp = new string(recRes2.chWTYIP);
                    if (String.Compare(sIp, textBox2.Text.ToString().Trim(), true) == 0)
                    {
                        // 显示IP地址2的识别数据
                        pateShow(recRes2, strFullFile2, strPlateFile2, this.pictureBox3, this.pictureBox4, PlateLabel2);
                    }
                    nCallbackTrigger2 = false;
                }
            }
        }

        // 回调方式接收识别结果
        public void CallbackFuntion()
        {
            int nNum = 0;
            int ret;
            rbMessage.Enabled = false;
            rbCallBack.Checked = true;

            IntPtr pIP1 = IntPtr.Zero;
            IntPtr pIP2 = IntPtr.Zero;


            this.ConnectCallback = new WTY.KHTSDK.WTYConnectCallback(this.ConnectStatue);
            this.JpegExCallback = new WTY.KHTSDK.WTYJpegCallback(this.JpegCallback);
            /*------------------------------7.1.6.0替换------------------------------------*/
            //this.DataExCallbcak = new WTY.KHTSDK.WTYDataExCallback(this.WTYDataExCallback);
            this.DataExCallbcak = new WTY.KHTSDK.WTYDataEx2Callback(this.WTYDataEx2Callback);

            // 注册通讯状态的回调函数（必选）
            WTY.KHTSDK.WTY_RegWTYConnEvent(this.ConnectCallback);

            // 注册获取JPEG流的回调函数(可选)
            WTY.KHTSDK.WTY_RegJpegEvent(this.JpegExCallback);

            /*------------------------------7.1.6.0替换------------------------------------*/
            // 注册获取识别结果的回调函数（必选）
            //WTY.KHTSDK.WTY_RegDataExEvent(this.DataExCallbcak);
            WTY.KHTSDK.WTY_RegDataEx2Event(this.DataExCallbcak);
            // 设置图片保存的路径。（根据需求来调用此函数）
            //WTY.KHTSDK.WTY_SetSavePath("E:\\videos\\");

            // IP地址1的设备初始化
            if (textBox1.Text != "")
            {
                pIP1 = Marshal.StringToHGlobalAnsi(textBox1.Text.Trim());
                // 链接设备
                ret = WTY.KHTSDK.WTY_InitSDK(8080, IntPtr.Zero, 0, pIP1);
                if (ret != 0)
                    listBox1.Items.Add(textBox1.Text.ToString() + "初始化失败！");
                else
                {
                    nNum = 1;
                    button2.Enabled = true;
                    g_bRecgoRuing = true;
                    listBox1.Items.Add(textBox1.Text.ToString() + "初始化成功！");
                    button3.Text = "断开连接";
                }
            }

            // IP地址2的设备初始化
            if (textBox2.Text != "")
            {
                pIP2 = Marshal.StringToHGlobalAnsi(textBox2.Text.Trim());
                // 链接设备
                ret = WTY.KHTSDK.WTY_InitSDK(8080, IntPtr.Zero, 0, pIP2);
                if (ret != 0)
                    listBox1.Items.Add(textBox2.Text.ToString() + "初始化失败！");
                else
                {
                    nNum = 2;
                    button2.Enabled = true;
                    g_bRecgoRuing = true;
                    listBox1.Items.Add(textBox2.Text.ToString() + "初始化成功！");
                    button3.Text = "断开连接";
                }
            }

            // 启用线程，用来显示识别结果到界面上
            if (nNum > 0)
            {
                Thread thread = new Thread(DeviceSelect);
                thread.Start();
            }
        }

        //消息的方式接收识别结果
        public void MessageFuntion()
        {
            int ret;
            IntPtr pIP = IntPtr.Zero;
            this.JpegExCallback = new WTY.KHTSDK.WTYJpegCallback(this.JpegCallback);
            // 注册通讯状态的回调函数
            WTY.KHTSDK.WTY_RegWTYConnEvent(this.ConnectCallback);

            // 注册获取JPEG流的回调函数
            WTY.KHTSDK.WTY_RegJpegEvent(this.JpegExCallback);

            // 设置图片保存的路径。（根据需求来调用此函数）
            //WTY.KHTSDK.WTY_SetSavePath("E:\\videos\\");

            // IP地址1的设备初始化
            if (textBox1.Text != "")
            {
                pIP = Marshal.StringToHGlobalAnsi(textBox1.Text.Trim());
                // 初始化设备
                ret = WTY.KHTSDK.WTY_InitSDK(8080, this.Handle, WM_GETDATA1, pIP);
                if (ret != 0)
                    listBox1.Items.Add(textBox1.Text.ToString() + "初始化失败！");
                else
                {
                    button2.Enabled = true;
                    g_bRecgoRuing = true;
                    listBox1.Items.Add(textBox1.Text.ToString() + "初始化成功！");
                    button3.Text = "断开连接";
                }
            }

            // IP地址2的设备初始化
            if (textBox2.Text != "")
            {
                pIP = Marshal.StringToHGlobalAnsi(textBox2.Text.Trim());
                // 初始化设备
                ret = WTY.KHTSDK.WTY_InitSDK(8080, this.Handle, WM_GETDATA2, pIP);
                if (ret != 0)
                    listBox1.Items.Add(textBox2.Text.ToString() + "初始化失败！");
                else
                {
                    button2.Enabled = true;
                    g_bRecgoRuing = true;
                    listBox1.Items.Add(textBox2.Text.ToString() + "初始化成功！");
                    button3.Text = "断开连接";
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (g_bRecgoRuing == true)
            {
                // 断开所有设备，并释放所有设备占用的资源
                WTY.KHTSDK.WTY_QuitSDK();
                rbMessage.Enabled = true;
                rbCallBack.Enabled = true;

                button2.Enabled = false;
                g_bRecgoRuing = false;
                button3.Text = "连接相机";
                listBox1.Items.Add("断开所有相机！");
            }
            else
            {
                sIp1 = textBox1.Text;
                sIp2 = textBox1.Text;
                // 回调方式获取识别结果
                if (rbCallBack.Checked)
                {
                    CallbackFuntion();
                }
                else if (rbMessage.Checked)// 消息方式获取识别结果
                {
                    MessageFuntion();
                }
            }
        }
        // 相机的连接状态
        public void ConnectStatue(StringBuilder chWTYIP, UInt32 Status)
        {
            if (Status == 0)
            {
                listBox1.Items.Add(chWTYIP + "连接失败！");
            }
            else
            {
                //listBox1.Items.Add("连接成功！");
            }
        }


        //获取JPEG流的回调函数
        public unsafe void JpegCallback(IntPtr JpegInfo)
        {
            WTY.DevData_info jpegResult_ss = new WTY.DevData_info();
            int size = Marshal.SizeOf(jpegResult_ss);
            byte[] bytes = new byte[size];
            Marshal.Copy(JpegInfo, bytes, 0, size);
            WTY.DevData_info jpegResult_s = ConverBytesToStructure<WTY.DevData_info>(bytes);

            byte[] chJpegStream = new byte[WTY.KHTSDK.BIG_PICSTREAM_SIZE + 312];

            string devIP = new string(jpegResult_s.chIp);
            devIP = devIP.Split('\0')[0];


            if (String.Compare(sIp1, devIP, true) == 0) //连接多台相机时，通过IP地址判断是哪台相机返回的数据
            {
                if (jpegResult_s.nStatus == 0)
                {
                    if ((jpegResult_s.nLen > 0) && (jpegResult_s.pchBuf != null))
                    {
                        //把图像数据拷贝到指定内存
                        Int32 nJpegStream = jpegResult_s.nLen;
                        Array.Clear(chJpegStream, 0, chJpegStream.Length);
                        Marshal.Copy(jpegResult_s.pchBuf, chJpegStream, 0, nJpegStream);

                        //显示JPEG流
                        pictureBox1.Image = Image.FromStream(new MemoryStream(chJpegStream));
                    }
                }
            }
            else if (String.Compare(sIp2, devIP, true) == 0)
            {
                if (jpegResult_s.nStatus == 0)
                {
                    if ((jpegResult_s.nLen > 0) && (jpegResult_s.pchBuf != null))
                    {
                        //把图像数据拷贝到指定内存
                        Int32 nJpegStream = jpegResult_s.nLen;
                        Array.Clear(chJpegStream, 0, chJpegStream.Length);
                        Marshal.Copy(jpegResult_s.pchBuf, chJpegStream, 0, nJpegStream);

                        //显示JPEG流
                        pictureBox3.Image = Image.FromStream(new MemoryStream(chJpegStream));
                    }
                }
            }
        }

        // 获取识别结果的回调函数 （新的使用方式）
        /*
         public unsafe void WTYDataExCallback(IntPtr recResult)
         {
             WTY.plate_result recRes;
             string sIp;

               注：
                 客户在挂接的时候，不要将自己的事物处理放到此回调函数中。
                 否则，可能会影响DLL的正常工作。
                 将识别数据拷贝全局缓冲区，去处理。

             WTY.plate_result recResult_ss = new WTY.plate_result();
             int size = Marshal.SizeOf(recResult_ss);
             byte[] bytes = new byte[size];
             Marshal.Copy(recResult, bytes, 0, size);
             recRes = ConverBytesToStructure<WTY.plate_result>(bytes);

             sIp = new string(recRes.chWTYIP);
             if (String.Compare(sIp, textBox1.Text.ToString().Trim(), true) == 0)
             {
                 // 将识别结果拷贝全局缓冲区
                 recRes1 = ConverBytesToStructure<WTY.plate_result>(bytes);
                 // 通知显示线程去显示识别结果
                 nCallbackTrigger1 = true;
             }
             if (String.Compare(sIp, textBox2.Text.ToString().Trim(), true) == 0)
             {
                 // 将识别结果拷贝全局缓冲区
                 recRes2 = ConverBytesToStructure<WTY.plate_result>(bytes);
                 // 通知显示线程去显示识别结果
                 nCallbackTrigger2 = true;
             }
         }
         */
        //至7.1.6.0起，采用新的识别结果结构及回调函数

        public unsafe void WTYDataEx2Callback(IntPtr recResult)
        {
            WTY.plate_result_ex recRes;
            string sIp;
            /*        注：
                    客户在挂接的时候，不要将自己的事物处理放到此回调函数中。
                    否则，可能会影响DLL的正常工作。
                    将识别数据拷贝全局缓冲区，去处理。*/
            WTY.plate_result_ex recResult_ss = new WTY.plate_result_ex();
            int size = Marshal.SizeOf(recResult_ss);
            byte[] bytes1 = new byte[size];
            Marshal.Copy(recResult, bytes1, 0, size);
            recRes = ConverBytesToStructure<WTY.plate_result_ex>(bytes1);




            sIp = new string(recRes.chWTYIP);
            if (String.Compare(sIp, textBox1.Text.ToString().Trim(), true) == 0)
            {
                Array.Clear(recRes1FullImg, 0, recRes1FullImg.Length);
                Marshal.Copy(recRes.pFullImage.pBuffer, recRes1FullImg, 0, recRes.pFullImage.nLen);

                Array.Clear(recRes1PlateImg, 0, recRes1PlateImg.Length);
                Marshal.Copy(recRes.pPlateImage.pBuffer, recRes1PlateImg, 0, recRes.pPlateImage.nLen);

                // 将识别结果拷贝全局缓冲区
                recRes1 = ConverBytesToStructure<WTY.plate_result_ex>(bytes1);
                // 通知显示线程去显示识别结果
                nCallbackTrigger1 = true;
            }
            if (String.Compare(sIp, textBox2.Text.ToString().Trim(), true) == 0)
            {
                Array.Clear(recRes2FullImg, 0, recRes2FullImg.Length);
                Marshal.Copy(recRes.pFullImage.pBuffer, recRes2FullImg, 0, recRes.pFullImage.nLen);

                Array.Clear(recRes2PlateImg, 0, recRes2PlateImg.Length);
                Marshal.Copy(recRes.pPlateImage.pBuffer, recRes2PlateImg, 0, recRes.pPlateImage.nLen);

                // 将识别结果拷贝全局缓冲区
                recRes2 = ConverBytesToStructure<WTY.plate_result_ex>(bytes1);
                // 通知显示线程去显示识别结果
                nCallbackTrigger2 = true;
            }
        }
        // 显示车牌函数
        public void ShowPlate(String strPlate, String strColor, Label lbl)
        {
            if (strPlate.TrimEnd('\0').Length == 0)
            {
                listBox1.Items.Add("");
                listBox1.Items.Add("识别结果为：无牌车！");
                lbl.Text = "无牌车";
                lbl.BackColor = Color.Gray;
                lbl.ForeColor = Color.White;
            }
            else
            {
                listBox1.Items.Add("");
                string str = "识别结果为：" + strColor.TrimEnd('\0') + "," + strPlate;
                listBox1.Items.Add(str);
                lbl.Text = strPlate;
                if (String.Compare("蓝", strColor) == 0)
                {
                    lbl.BackColor = Color.Blue;
                    lbl.ForeColor = Color.White;
                }
                else if (String.Compare("白", strColor) == 0)
                {
                    lbl.BackColor = Color.White;
                    lbl.ForeColor = Color.Black;
                }
                else if (String.Compare("黄", strColor) == 0)
                {
                    lbl.BackColor = Color.Yellow;
                    lbl.ForeColor = Color.Black;
                }
                else if (String.Compare("黑", strColor) == 0)
                {
                    lbl.BackColor = Color.Black;
                    lbl.ForeColor = Color.White;
                }
                string time = null;
                string sm = strPlate.Trim();
                string sn = sm.Trim('\0');
                time = ope9.selectOneValue("select apptime from appointment where plateNumber='" + sn + "';");
                if (time == null)
                {
                    MessageBox.Show("此车未预约", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    tcp.send("D");
                    MessageBox.Show("此车已预约，车位上地锁即将下降", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                }
            }
        }

        // 显示识别时间
        public void ShowPlateC(Int32 nLeft,
                                Int32 nTop,
                                Int32 nRight,
                                Int32 nBottom)
        {
            if ((nRight != 0) || (nBottom != 0))
            {
                string str = "车牌坐标为："
                                + "left:" + nLeft + ", "
                                + "top:" + nTop + ", "
                                + "right:" + nRight + ", "
                                + "bottom:" + nBottom;
                listBox1.Items.Add(str);
            }
        }

        // 显示识别时间
        public void ShowRecogniseTime(Int32 nYear,
                                        Int32 nMonth,
                                        Int32 nDay,
                                        Int32 nHour,
                                        Int32 nMinute,
                                        Int32 nSecond,
                                        Int32 nMillisecond)
        {
            string str = "识别时间为：" + nYear + "-"
                            + nMonth + "-"
                            + nDay + " "
                            + nHour + ":"
                            + nMinute + ":"
                            + nSecond + "."
                            + nMillisecond;
            listBox1.Items.Add(str);
        }


        // 接收到识别数据后的处理
        public void MessageFunPro(String sIp, String fullImgFile, String plateImgFile, PictureBox FullImg, PictureBox PlateImg, Label lbl)
        {
            //IP
            StringBuilder sIP = null;
            //全景图
            byte[] bytFullImage = null;
            //车牌小图
            byte[] bytPlateImage = null;
            //车牌号码
            StringBuilder sPlate = null;
            //车牌颜色
            StringBuilder sColor = null;

            // 车牌号码
            sPlate = new StringBuilder(PLATE_LEN);
            // 车牌颜色
            sColor = new StringBuilder(COLOR_LEN);
            // 保存全景图
            bytFullImage = new byte[BIGIMAGE_LEN];
            // 保存车牌小图
            bytPlateImage = new byte[SMALLIMAGE_LEN];
            // IP
            sIP = new StringBuilder(sIp);

            int nFullLen = 0;
            int nPlateLen = 0;

            System.IntPtr pFull = Marshal.UnsafeAddrOfPinnedArrayElement(bytFullImage, 0);
            System.IntPtr pPlate = Marshal.UnsafeAddrOfPinnedArrayElement(bytPlateImage, 0);

            //获取识别信息
            int Res = WTY.KHTSDK.WTY_GetVehicleInfo(sIP, sPlate, sColor, pFull, ref nFullLen, pPlate, ref nPlateLen);
            if (Res != -1)
            {
                string strLicesen = sPlate.ToString();
                
                string strColor = sColor.ToString();
                // 显示车牌
                ShowPlate(strLicesen, strColor, lbl);

                if (nFullLen > 0)
                {
                    //保存全景图
                    FileStream fs = new FileStream(fullImgFile, FileMode.Create, FileAccess.Write);
                    try
                    {
                        fs.Write(bytFullImage, 0, nFullLen);
                        fs.Close();
                    }
                    catch (Exception ex)
                    {
                        fs.Close();
                        // Console.WriteLine(ex.Message);
                    }
                    // 将全景图显示在界面上
                    FullImg.Image = null;
                    FileInfo fi = new FileInfo(fullImgFile);
                    if (fi.Exists)
                    {
                        try
                        {
                            FullImg.Load(fullImgFile);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                if (nPlateLen > 0)
                {
                    // 保存车牌小图
                    FileStream fs = new FileStream(plateImgFile, FileMode.Create, FileAccess.Write);
                    try
                    {
                        fs.Write(bytPlateImage, 0, nPlateLen);
                        fs.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    // 将车牌小图显示在界面上
                    PlateImg.Image = null;
                    FileInfo fi = new FileInfo(plateImgFile);
                    if (fi.Exists)
                    {
                        PlateImg.Load(plateImgFile);
                    }
                }
            }
        }

        // 响应消息
        protected override void DefWndProc(ref Message m)
        {
            String sIp1;
            String sIp2;

            String strFullFile1 = "FullImage1.jpg";
            String strPlateFile1 = "PlateImage1.jpg";
            String strFullFile2 = "FullImage2.jpg";
            String strPlateFile2 = "PlateImage2.jpg";

            switch (m.Msg)
            {
                // IP地址1设备的消息处理函数
                case WM_GETDATA1:
                    {
                        if (g_bRecgoRuing == true)
                        {
                            sIp1 = textBox1.Text.ToString();
                            MessageFunPro(sIp1, strFullFile1, strPlateFile1, this.pictureBox1, this.pictureBox2, PlateLabel1);
                        }
                    }
                    break;
                // IP地址2设备的消息处理函数
                case WM_GETDATA2:
                    {
                        if (g_bRecgoRuing == true)
                        {
                            sIp2 = textBox2.Text.ToString();
                            MessageFunPro(sIp2, strFullFile2, strPlateFile2, this.pictureBox3, this.pictureBox4, PlateLabel2);
                        }
                    }
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tcp.send("U");
        }



        // 模拟地感触发，通过网络向设备发送触发信号
        private void button2_Click(object sender, EventArgs e)
        {
            int ret;
            string sIp;

            sIp = textBox1.Text.ToString();
            // IP地址1的设备发送模拟触发指令
            if (sIp.Length != 0)
            {
                ret = WTY.KHTSDK.WTY_SetTrigger(sIp, 8080);
                if (ret < 0)
                {
                    listBox1.Items.Add(sIp + "触发失败");
                }
            }

            sIp = textBox2.Text.ToString();
            // IP地址2的设备发送模拟触发指令
            if (sIp.Length != 0)
            {
                ret = WTY.KHTSDK.WTY_SetTrigger(sIp, 8080);
                if (ret < 0)
                {
                    listBox1.Items.Add(sIp + "触发失败");
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            g_bRecgoRuing = false;
            WTY.KHTSDK.WTY_QuitSDK();
        }
    

}
}
