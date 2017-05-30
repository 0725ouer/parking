using System;
using System.Windows.Forms;
using parking.数据库操作;
using parking.窗体自适应;
using parking.界面;


namespace parking
{
    public partial class login : Form
    {
        private databaseOperation ope;
        private string id=null, psd=null;
        AutoSizeFormClass asc = new AutoSizeFormClass();
        
        public login()
        {
            InitializeComponent();
            ope = new databaseOperation();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                login1_Click(null, null);
            }
        }


        /*private void login_FormClosing(object sender, FormClosingEventArgs e)      //login退出提示
        {
            if (DialogResult.OK == MessageBox.Show("你确定要关闭应用吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
            {
                this.FormClosing -= new FormClosingEventHandler(this.login_FormClosing);//为保证Application.Exit();时不再弹出提示，所以将FormClosing事件取消
                Application.Exit();//退出整个应用程序
            }
            else
            {
                e.Cancel = true;  //取消关闭事件
            }
        }*/
        private void login_Load(object sender, EventArgs e)    //login加载
        {
           // asc.controlAutoSize(this);
            name_save.Checked = Properties.Settings.Default.check_namesave;
            if (name_save.Checked)
            {
                name.Text = Properties.Settings.Default.name;
            }
            else
            {
                name.Text = "";
            }
        }

        private void login_layout(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }

        private void exit_Click(object sender, EventArgs e) //Exit按钮
        {
            this.Close();
        }

        private void login1_Click(object sender, System.EventArgs e)
        {
            id = name.Text;
            psd = password.Text;
            string res = ope.selectOneValue(
                    "select password from manager where name='" + id + "';");
            if (id.Equals("") || psd.Equals(""))
            {
                MessageBox.Show("请输入用户名或密码", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (psd.Equals(res))
                {
                    if(name_save.Checked)
                    {
                        Properties.Settings.Default.name = id;
                        Properties.Settings.Default.check_namesave = true;
                        Properties.Settings.Default.Save();//使用Save方法保存更改

                    }
                    else
                    {
                        Properties.Settings.Default.check_namesave = false;
                        Properties.Settings.Default.Save();//使用Save方法保存更改
                    }
                    main main1=new main();
                    main1.Show();
                    this.Visible = false;
                    
                    //MessageBox.Show("登录成功","提示");
                    
                }
                else
                {
                    MessageBox.Show("用户名或密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    password.Text = "";
                }
            }
        }
        
    }
    }
