using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Dapper;
using OrderManSys.Model;

//This class is for accessing Factory.Orders in database. Using Orders model class.
//For providing unified access to database for whole system.

namespace OrderManSys.Repository
{
    public class OrderRepo
    {
        //Creating Connection string. 
        private readonly string _connectionstring;
        public OrderRepo(string ConnectionString)
        {
            ConnectionString = _connectionstring;
        }

        //Create IdbConnection instance.
        public IDbConnection Connection
        {
            get
            {
                return new MySqlConnection(_connectionstring); //Connection Object created from Mysql.Data.MySqlClient
            }
        }

        //Return ALL records in the Order table.
        public IEnumerable<Orders> GetAll()
        {
            using (IDbConnection dbConnection = Connection)
            {
                //Selecting Orders with it's corresponding parent table Products.
                string sQuerry = @"select O.Quantity,O.FinishTime,O.OrderTime,O.OrderName,O.Finished,O.Id,
                P.Id, P.ProductName,P.Description,P.Price from Orders O Inner JOIN Product P on O.Product=P.Id";
                dbConnection.Open();
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Orders>, 
                //Plus single Child object <Product> 
                return dbConnection.Query<Orders, Product, Orders>(sQuerry, (O, P) => { O.product = P; return O; }, splitOn: "Id").AsEnumerable();
            }
        }

        //Search one record with id passed in. Return Null if nothing found.
        public Orders GetbyId(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                //Selecting Orders with it's corresponding parent table Products, with WHERE clause.
                string sQuerry = @"select O.Quantity,O.FinishTime,O.OrderTime,O.OrderName,O.Finished,O.Id,
                P.Id, P.ProductName,P.Description,P.Price from Orders O Inner JOIN Product P on O.Product=P.Id WHERE O.Id = @OrderID";
                dbConnection.Open();
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Orders>, 
                //Plus single Child object <Product> 
                return dbConnection.Query<Orders, Product, Orders>(sQuerry, (O, P) => { O.product = P; return O; }, new { OrderID = id }, splitOn: "Id").FirstOrDefault();
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Orders>.
            }
        }

        /* 
        Query the database with dynamically selected column
        When calling this function, create an dictionary contains <Column name string,Querry value object> as Querry Parameters.
        
        For example:
        var Parameters = new Dictionary<string,object>();
        Parameters.Add("Ordertime",DateTime.Parse("2018-04-25"));

        DO NOT expose this function!
        */
        public IEnumerable<Orders> Get(Dictionary<string, object> WhereParameters)
        {
            if (WhereParameters.Count == 0)
            {
                //throw exception if empty Dictionary is passed in (consider use GetAll()?)
                throw new ArgumentNullException();
            }
            using (IDbConnection dbConnection = Connection)
            {
                //Create the sql querry dynamically, this is the starter.
                string sQuerry = @"select O.Quantity,O.FinishTime,O.OrderTime,O.OrderName,O.Finished,O.Id,
                P.Id, P.ProductName,P.Description,P.Price from Orders O Inner JOIN Product P on O.Product=P.Id Where";
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
                    sQuerry = sQuerry + $" O.{item.Key} = @{item.Key}";//new where clause.
                    dp.Add($"{item.Key}", item.Value);//add new parameter in dp
                    if (item.Key != last.Key) //No dulpicated key will occur because every key in dictionary is unique (C# rule)
                    {
                        sQuerry = sQuerry + " AND";
                    }
                }
                //Console.WriteLine("[Debug](Querry):"+sQuerry);
                dbConnection.Open();
                return dbConnection.Query<Orders, Product, Orders>(sQuerry, (O, P) => { O.product = P; return O; }, dp, splitOn: "Id").ToList();
            }
        }


        //Place a new order.
        public void Create(Orders entity)
        {
            using (IDbConnection dbConnection = Connection)
            {
                //
                string sQuerry = "Insert Into Orders(Product,Quantity,FinishTime,OrderName,OrderTime,Finished) " +
                    $"Values (@ProductId,@Quantity,@FinishTime,@OrderName,@OrderTime,@Finished)";
                dbConnection.Execute(sQuerry, new
                {   //Custom Parameters set.
                    Quantity = entity.Quantity,
                    FinishTime = entity.FinishTime,
                    OrderName = entity.OrderName,
                    OrderTime = entity.OrderTime,
                    Finished = entity.Finished,
                    ProductId = entity.product.Id
                });
            }
        }

        //Delete a order (Need test!) (Should I change to "IsDeleted?")
        public void Delete(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string sQuerry = "DELETE from Orders where id = @Id";
                dbConnection.Execute(sQuerry, new { Id = id });
            }
        }
    }
}