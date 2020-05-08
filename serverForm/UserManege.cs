using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace serverForm
{
    class UserManege
    {

        SQLiteCommand cmd;
        SQLiteConnection cn;
        SQLiteDataAdapter mAdapter;
        DataTable mTable;
        string getName;//查询到的用户名
        string getPasswd;//查询到的密码
        public UserManege()
        {
            dataBaseInit();
        }
        public void dataBaseInit()
        {
            CreateDB();
            CreateTable(0);
        }
        /// <summary>
        /// 获取整个表格。
        /// </summary>
        /// <returns></returns>
        public DataTable getTable()
        {

            mAdapter = new SQLiteDataAdapter("SELECT * FROM [userInfo]", cn);
            mTable = new DataTable(); // Don't forget initialize!
            mAdapter.Fill(mTable);
            return mTable;
        }

        public void addUser(string userName,string passwd)
        {
            cn.Open();
            cmd.CommandText = "INSERT INTO userInfo VALUES(\'" + userName + "\',\'" + passwd+"\')";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            { 
            
            }
            
            cn.Close();
        }
        /// <summary>
        /// delete user
        /// </summary>
        public void deleUser()
        {

        }
        /// <summary>
        /// modify usre passwd
        /// </summary>
        public bool changeUser(string userName, string passwd)
        {

            return true;
        }

        public bool searchUser(string userName, string passwd)
        {
            cmd.CommandText = "SELECT * FROM userInfo WHERE userName=\'"+ userName + "\'and passwd=\'" + passwd + "\'";
            Console.WriteLine(cmd.CommandText);
            cn.Open();
            SQLiteDataReader sr = cmd.ExecuteReader();
            while (sr.Read())
            {
                sr.Close();
                return true;
            }
            sr.Close();
            return false;
        }
        public void CreateDB()
        {
            string path = @"123.sqlite";
            if (System.IO.File.Exists(path))
            {
                SQLiteConnection cn = new SQLiteConnection("data source=" + path);
                cn.Open();
                cn.Close();
            }

        }
        /// <summary>
        /// 删除数据库
        /// </summary>
        public void DeleteDB()
        {
            string path = @"user.sqlite";
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        /// <summary>
        /// 创建表格
        /// </summary>
        public void CreateTable(int flag)
        {
            string path = @"user.sqlite";
            cn = new SQLiteConnection("data source=" + path);

            if (cn.State != System.Data.ConnectionState.Open)
            {
                cn.Open();
                cmd = new SQLiteCommand();
                cmd.Connection = cn;
                //cmd.CommandText = "CREATE TABLE t1(id varchar(4),score int)";
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS userInfo(userName varchar(32),passwd varchar(32))";
                cmd.ExecuteNonQuery();
            }
            //创建表格时添加root不可以删除
            cmd.CommandText = "INSERT INTO userInfo(userName,passwd) select 'root',123456 from userInfo  WHERE NOT EXISTS( SELECT *  FROM userInfo  WHERE userName = 'root')";
            cmd.ExecuteNonQuery();
            cn.Close();
        }
        public void deleTable()
        {
            cn.Open();
            cmd = new SQLiteCommand();
            cmd.Connection = cn;
            cmd.CommandText = "DROP TABLE userInfo";
            cmd.ExecuteNonQuery();
            cn.Close();
        }
    }
}
