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

        //Will return ALL records in table  
        public IEnumerable<Schedule> GetAll()
        {
            using(IDbConnection dbConnection = Connection)
            {
                string sQuerry = @"SELECT S.Working,S.OrderId,S.Id,O.Id,O.Quantity,O.FinishTime,O.OrderTime,O.Finished,O.OrderName from Schedule S
                    INNER JOIN Orders O ON S.OrderId = O.Id";
                dbConnection.Open();
                return dbConnection.Query<Schedule,Orders,Schedule>(sQuerry,(S,O)=>{S.Orders=O;return S;},splitOn:"Id").AsEnumerable(); 
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Schedule>. 
            }
        }

        //Return single Schedule accroding to id.
        public Schedule GetById(int Id)
        {
            using(IDbConnection dbConnection = Connection)
            {
                string sQuerry = @"SELECT S.Working,S.OrderId,S.Id,O.Id,O.Quantity,O.FinishTime,O.OrderTime,O.Finished,O.OrderName from Schedule S
                    INNER JOIN Orders O ON S.OrderId = O.Id WHERE S.Id = @SchId";
                dbConnection.Open();
                return dbConnection.Query<Schedule>(sQuerry,new{SchId = Id}).FirstOrDefault();
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Schedule>. 
            }
        }

        //Write a single schedule records back to database.
        public void Create(Schedule schedule)
        {
            using (IDbConnection dbconnection = Connection)
            {
                dbconnection.Open();
                //SQL querry, using Insert satatment to create records. (ID NEEDS TO START FROM 1!)
                string sqlstr="INSERT INTO Schedule(Id,Working,OrderId) VALUES (@Id,@Working,@OrderId);";
                dbconnection.Execute(sqlstr,new
                {   //Custom Parameters set.
                    Id = schedule.Id,
                    Working = schedule.Working,
                    OrderId = schedule.Orders.Id
                });
            }
        }

        //Overloaded function. Write all the schedule records back to database.
        public void Create(IEnumerable<Schedule> ScheduleList)
        {
            using (IDbConnection dbconnection = Connection)
            {
                List<object> Parameters = new List<object>();
                foreach (var schedule in ScheduleList)
                {
                    Parameters.Add(new
                    {   
                        //Custom Parameters set.
                        Id = schedule.Id,
                        Working = schedule.Working,
                        OrderId = schedule.Orders.Id
                    });
                }

                dbconnection.Open();
                //SQL querry, using Insert satatment to create records. (ID NEEDS TO START FROM 1!)
                string sqlstr="INSERT INTO Schedule(Id,Working,OrderId) VALUES (@Id,@Working,@OrderId);";
                dbconnection.Execute(sqlstr,Parameters);
            }
        }

        //Delete an single schedule (Need test!)
        public void Delete(int id)
        {
            using(IDbConnection dbConnection = Connection)
            {
                string sQuerry = "DELETE from Schedule where id = @Id";
                dbConnection.Execute(sQuerry,new{Id = id});
            }
        }

        //Extended function, Use with caution!
        public void DropAll()
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