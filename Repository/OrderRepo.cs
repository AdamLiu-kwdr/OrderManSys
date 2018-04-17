using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Dapper;
using OrderManSys.Model;

//This class is for accessing Factory.Orders in database. Using Orders model class.

namespace OrderManSys.Repository
{
    public class OrderRepo
    {
        //Creating Connection string. 
        private string ConnectionString;
        public OrderRepo()
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
        public IEnumerable<Orders> GetAll()
        {
            using (IDbConnection dbConnection = Connection)
            {
                string sQuerry="SELECT * FROM Orders";
                dbConnection.Open();
                return dbConnection.Query<Orders>(sQuerry);
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Orders>. 
            }
        }

        //Search one record with id passed in. Return Null if nothing found.
        public Orders GetbyId(int id)
        {
            using(IDbConnection dbConnection = Connection)
            {
                string sQuerry = "SELECT * FROM Orders Where Id = @ID";
                dbConnection.Open();
                return dbConnection.Query<Orders>(sQuerry,new{ID = id}).FirstOrDefault();
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Orders>.
            }
        }

        //Find the Id based on Order's name and order time, throw exception if nothing found.
        //If looking for an Order Object, call it as nested function. like this: OrderRepo.GetById(OrderRepo.FindId(Odername,Ordertime))
        public int FindId(string Ordername,DateTime Ordertime)
        {
            using(IDbConnection dbConnection = Connection)
            {
                string sQuerry = "SELECT Id FROM Orders Where OrderName = @OrderName and OrderTime = @OrderTime";
                dbConnection.Open();
                return dbConnection.Query<int>(sQuerry,new{OrderName = Ordername,OrderTime=Ordertime}).First();
                //Find the Id based on name and order time.
            }
        }



        public void InsertNew(Orders order)
        {
            using(IDbConnection dbConnection = Connection)
            {
                string sQuerry = "Insert Into Orders(Product,Quantity,FinishTime,OrderName,OrderTime,Finished) " +
                    "Values (@Product,@Quantity,@FinishTime,@OrderName,@OrderTime,@Finished)";
                dbConnection.Execute(sQuerry,order);
            }
        }
    }
}