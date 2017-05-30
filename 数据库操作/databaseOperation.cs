using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace parking.数据库操作
{
    class databaseOperation
    {
        public MySqlConnection conn;
        //public SqlConnection conn;
        public MySqlDataAdapter da;
        public DataSet ds;
        public MySqlCommand cmd;
        public Boolean res = false;
        public databaseOperation()
        {

            //conn = new SqlConnection("server = bds252438393.my3w.com;"
            //   + "uid=bds252438393;pwd=0725ouer;database=bds252438393_db");
            conn = new MySqlConnection("server=118.89.145.65;User Id=root;password=root;Database=yibo");
            //conn = new SqlConnection("Server=.;Database=parking;Trusted_Connection=SSPI");
            da = new MySqlDataAdapter();
            ds = new DataSet();
            cmd = new MySqlCommand();
            cmd.Connection = conn;
            res = true;
        }

        /*
         * 返回查询结果集中的第一行第一列，其他结果忽略
         * 即查询数据库中一个值
         */
        public string selectOneValue(string command)
        {
            string res = null;
            this.conn.Open();
            cmd.CommandText = command;
            try
            {
                res = this.cmd.ExecuteScalar().ToString().Trim();
            }
            catch (Exception e)
            {
                //MessageBox.Show("错误!");
            }

            this.conn.Close();
            return res;
        }

        /*
         * 修改数据库
         * 实现对数据库的insert modify delete
         */
        public int modifyDatabase(string command)
        {
            int colcount = 0;
            this.conn.Open();
            cmd.CommandText = command;
            try
            {
                colcount = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {

            }
            this.conn.Close();
            return colcount;
        }

        public int modifyDatabase3(string command1, string command2, string command3)//修改三项
        {
            int colcount = 0;
            this.conn.Open();
            cmd.CommandText = command1;
            try
            {
                colcount = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {

            }
            colcount = 0;
            cmd.CommandText = command2;
            try
            {
                colcount = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {

            }
            colcount = 0;
            cmd.CommandText = command3;
            try
            {
                colcount = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {

            }
            this.conn.Close();
            return colcount;
        }

        /*
         * 查表
         * 实现对数据库查询的select
         * 返回类型为DataTable
         * 可直接dataGridView.DataSource=selectTable(...)
         */
        public DataTable selectTable(string sql)
        {
            da = new MySqlDataAdapter(sql, conn);
            da.Fill(ds);
            return ds.Tables[0];
        }


    }
}
