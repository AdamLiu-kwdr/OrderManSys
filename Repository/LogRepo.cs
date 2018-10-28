using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Options;
using Dapper;
using OrderManSys.Model;

//This class is for accessing Factory.Log in database. Using Log.cs model class.
//For providing unified access to database throughout whole system.
//This class also implements IDBRepository interface. Have following function: GetAll,GetById,Get,Create (Warning: Update and Delete is not supported!) 
//Note: Because Log table contains too many records. All sql is limited to return first 100 results.

namespace OrderManSys.Repository
{
    public class LogRepo : IDBRepository<Log>
    {
        private readonly string _connectionstring;
        public LogRepo(string ConnectionString)
        {
            _connectionstring = ConnectionString;
        }

        //Create IdbConnection instance.
        public IDbConnection Connection
        {
            get{
                return new MySqlConnection(_connectionstring); //Connection Object created from Mysql.Data.MySqlClient
            }
        }

        //Return ALL records in the Log table.
        public IEnumerable<Log> GetAll()
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<Log>("SELECT * FROM Log ORDER BY Time DESC LIMIT 100"); 
            }
        }

        //Search one record with id passed in. Return Null if nothing found.
        public Log GetbyId(int id)
        {
            using(IDbConnection dbConnection = Connection)
            {
                string sQuerry = "SELECT * FROM Log Where Id = @ID"; //Querry string
                dbConnection.Open();
                return dbConnection.Query<Log>(sQuerry,new{ID = id}).FirstOrDefault();
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Log>. 
            }
        }

        /* 
        Query the database with dynamically selected column
        When calling this function, create an dictionary contains <Column name string,Querry value object> as Querry Parameters.
        
        For example:
        var Parameters = new Dictionary<string,object>();
        Parameters.Add("Ordertime",DateTime.Parse("2018-04-25"));

        DO NOT expose this function! SQL injection not protected.
        */
        public IEnumerable<Log> Get(Dictionary<string,object> WhereParameters)
        {
            if (WhereParameters.Count == 0)
            {
                //throw exception if empty Dictionary is passed in (consider use GetAll()?)
                throw new ArgumentNullException();
            }
            using(IDbConnection dbConnection = Connection)
            {
                //Create the sql querry dynamically, this is the starter.
                string sQuerry = @"select * from Log l Where";
                //The parameters to be added to the querry later.
                DynamicParameters dp = new DynamicParameters();

                /* 
                Unboxing the Dictionary, for every keyPair in dictionary create a new parameter in dp, name it as keyPair's key.
                then add a new where clause to the query "Where keyPair.keyname = @keypair.keyname"
                Finally check if current pair is the last pair in dictionary, if not put a "AND" between current and next where clause. 
                */
                var last = WhereParameters.Last();
                foreach (var item in WhereParameters)
                {
                    sQuerry = sQuerry + $" l.{item.Key} = @{item.Key}";//new where clause.
                    dp.Add($"{item.Key}",item.Value);//add new parameter in dp
                    if (item.Key != last.Key) //No dulpicated key will occur because every key in dictionary is unique (C# rule)
                    {
                        sQuerry = sQuerry + " AND";
                    }
                }
                //Console.WriteLine("[Debug](Querry):"+sQuerry);
                sQuerry = sQuerry + " ORDER BY Time DESC LIMIT 100"; //Limit to first 100 results.
                dbConnection.Open();
                return dbConnection.Query<Log>(sQuerry,dp).ToList();
            }
        }


        //Create a new Log.
        public void Create(Log entity)
        {
            using(IDbConnection dbConnection = Connection)
            {
                entity.Time = DateTime.Now;
                //Querry
                string sQuerry = "Insert Into Log(ID,type,Author,Message,Time) " +
                    $"Values (@ID,@type,@Author,@Message,@Time)";
                dbConnection.Execute(sQuerry,entity);
            }
        }

        //Delete Log is not allowed! why would you do that?
        //Function placed to support interface
        public void Delete(int id)
        {
            // using(IDbConnection dbConnection = Connection)
            // {
            //     string sQuerry = "DELETE from Log where id = @Id";
            //     dbConnection.Execute(sQuerry,new{Id = id});
            // }
            throw new NotSupportedException("Deleteing log is not supported.");
            
        }

        //NO LOG UPDATE! why would you do that?
        //Function placed to support interface
        public void Update(Log entity)
        {
            throw new NotSupportedException("Log modification is not supported.");
        }
    }
}