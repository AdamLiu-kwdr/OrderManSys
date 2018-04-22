using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Dapper;
using OrderManSys.Model;

namespace OrderManSys.Repository
{
    //This class is for accessing Factory.Schedule in database. Using Schedule model class.
    //For providing unified access to database for whole system.
    public class ScheduleRepo
    {
        
        private string ConnectionString;
        public ScheduleRepo()
        {
            ConnectionString = "Uid=adam;Pwd=1996-1120*mariadb;Host=192.168.23.131;database=Factory;Character Set=utf8;port=3306;SslMode=none;";
            //Connection string should be moved to appsettings.json later.
        }

                //Create IdbConnection instance.
        public IDbConnection Connection
        {
            get{
                return new MySqlConnection(ConnectionString); //Connection Object created from Mysql.Data.MySqlClient
            }
        }


        //Write all the schedule records back to database.
        public void WriteAll(IEnumerable<Schedule> ScheduleList)
        {
            using (IDbConnection dbconnection = Connection)
            {
                dbconnection.Open();
                //SQL querry, using Insert satatment to create records. (ID NEEDS TO START FROM 1!)
                string sqlstr="INSERT INTO Schedule(Id,Quantity,Working,OrderId) VALUES (@Id,@Quantity,@Working,@OrderId);";
                dbconnection.Execute(sqlstr,ScheduleList);
            }
        }

        //Use with caution!
        public void DeleteAll()
        {
            using (IDbConnection dbconnection = Connection)
            {
                dbconnection.Open();
                //SQL querry
                string sqlstr="Delete from Schedule;";
                dbconnection.Execute(sqlstr);
            }
        }
    }
}