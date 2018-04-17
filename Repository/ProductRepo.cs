using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Dapper;
using OrderManSys.Model;

//This class is for accessing Factory.Orders in database. Using Orders model class. !Class will be renamed in future!

namespace OrderManSys.Repository
{
    public class ProductRepo
    {
        //Creating Connection string
        private string ConnectionString;
        public ProductRepo()
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
        public IEnumerable<Product> Getall()
        {
            using(IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<Product>("SELECT * FROM Product"); 
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Product>. 
            }
        }

        //Return records in table where id=Product Id  
        public Product GetbyId(int id)
        {
            using(IDbConnection dbConnection = Connection)
            {
                string sQuerry = "SELECT * FROM Product Where Id = @ID"; //Querry string
                dbConnection.Open();
                return dbConnection.Query<Product>(sQuerry,new{ID = id}).FirstOrDefault();
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Product>. 
            }
        }
    }
}