using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Dapper;
using OrderManSys.Model;
using OrderManSys.Repository;

//This class is for accessing Factory.Orders in database. Using Orders model class. !Class will be renamed in future!
//This class also implements IDBRepository interface. Have following function: GetAll,GetById,Get(Paraments),Create,Update,Delete

namespace OrderManSys.Repository
{
    public class ProductRepo : IDBRepository<Product>
    {
        //Creating Connection string
        private string _connectionstring;
        public ProductRepo(string ConnectionString)
        {
            _connectionstring = ConnectionString;
            //Connection string should be moved to appsettings.json later.
        }

        //Create IdbConnection instance.
        public IDbConnection Connection
        {
            get
            {
                return new MySqlConnection(_connectionstring); //Connection Object created from Mysql.Data.MySqlClient
            }
        }

        //Will return ALL records in table  
        public IEnumerable<Product> GetAll()
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<Product>("SELECT * FROM Product");
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Product>. 
            }
        }

        //Return records in table where id=Product Id  
        public Product GetbyId(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string sQuerry = "SELECT * FROM Product Where Id = @ID"; //Querry string
                dbConnection.Open();
                return dbConnection.Query<Product>(sQuerry, new { ID = id }).FirstOrDefault();
                //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Product>. 
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
        public IEnumerable<Product> Get(Dictionary<string, object> WhereParameters)
        {
            if (WhereParameters.Count == 0)
            {
                //throw exception if empty Dictionary is passed in (consider use GetAll()?)
                throw new ArgumentNullException();
            }
            using (IDbConnection dbConnection = Connection)
            {
                //Create the sql querry dynamically, this is the starter.
                string sQuerry = @"select * from Product p Where";
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
                    sQuerry = sQuerry + $" p.{item.Key} = @{item.Key}";//new where clause.
                    dp.Add($"{item.Key}", item.Value);//add new parameter in dp
                    if (item.Key != last.Key) //No dulpicated key will occur because every key in dictionary is unique (C# rule)
                    {
                        sQuerry = sQuerry + " AND";
                    }
                }
                //Console.WriteLine("[Debug](Querry):"+sQuerry);
                dbConnection.Open();
                return dbConnection.Query<Product>(sQuerry, dp).ToList();
            }
        }

        //Create a new Product.
        public void Create(Product entity)
        {
            using (IDbConnection dbConnection = Connection)
            {
                //Querry, DB will handle Id
                string sQuerry = "Insert Into Product(Id,ProductName,Description,Price)" +
                    $"Values (@Id,@ProductName,@Description,@Price)";
                dbConnection.Execute(sQuerry, entity);
            }
        }

        //Delete a Product (Need test!) (Should I change to "IsDeleted?")
        public void Delete(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string sQuerry = "DELETE from Product where id = @Id";
                //Check affectedRows for delete result.
                int affectedRows = dbConnection.Execute(sQuerry, new { Id = id });
                if (affectedRows == 0)
                {
                    throw new KeyNotFoundException($"No recrod found with id: {id}");
                }
            }
        }

        //Update Product.
        public void Update(Product entity)
        {
            using (IDbConnection dbConnection = Connection)
            {
                

                //Check if product alreadt exits
                Product currentIndex = this.GetbyId(entity.Id);
                if (currentIndex == null)
                {
                    //redirect to Create if non exits, and escape.
                    this.Create(entity);
                    return;
                }

                //Execute Update querry
                string sQuerry = "Update Product SET ProductName=@ProductName,Description=@Description,Price=@Price" +
                    $" Where Id = @Id";
                dbConnection.Execute(sQuerry, entity);
            }
        }

    }
}