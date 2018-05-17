using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Dapper;
using OrderManSys.Model;

//This class is for accessing Factory.Instruction in database. Using Instruction model class.
//Providing unified object-orinated access to database throughout whole system.

namespace OrderManSys.Repository
{
    public class InstructionRepo
    {
        //Creating Connection string. 
        private string ConnectionString;
        public InstructionRepo()
        {
            ConnectionString = "Uid=adam;Pwd=1996-1120*mariadb;Host=192.168.23.131;database=Factory;Character Set=utf8;port=3306;SslMode=none;";
            //Connection string should be moved to appsettings.json later.
        }

        public IDbConnection Connection
        {
            get{
                return new MySqlConnection(ConnectionString); //Connection Object created from Mysql.Data.MySqlClient
            }
        }

        //Get all the registred product's instructions
        public IEnumerable<Instruction> GetAll()
        {
            using (IDbConnection dbConnection = Connection)
            {
            //Selecting Instructions with it's corresponding parent table Products.
            string sQuerry=@"select I.Step,I.Component,I.Action,I.Parameter,I.Id,
                P.Id,P.ProductName,P.Description,P.Price from Instruction I Inner JOIN Product P on I.Product=P.Id";
            dbConnection.Open();
            //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Orders>, 
            //Plus single Child object <Product> 
            return dbConnection.Query<Instruction,Product,Instruction>(sQuerry,(I,P)=>{I.Product=P;return I;},splitOn:"Id").AsEnumerable();
            }
        }

        //get a single instruction.
        public Instruction GetbyId(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
            //Selecting Instructions with it's corresponding parent table Products.
            string sQuerry=@"select I.Step,I.Component,I.Action,I.Parameter,I.Id,
                P.Id,P.ProductName,P.Description,P.Price from Instruction I Inner JOIN Product P on I.Product=P.Id WHERE I.Id = @InsId";
            dbConnection.Open();
            //Dapper extened function in IdbConnection, Querry the database and serillize results accroding to type <Orders>, 
            //Plus single Child object <Product> 
            return dbConnection.Query<Instruction,Product,Instruction>(sQuerry,(I,P)=>{I.Product=P;return I;},new{InsId = id},splitOn:"Id").First();
            }
        }

        /* 
        Query the database with dynamically selected column
        When calling this function, create an dictionary contains <Column name string,Querry value object> as Querry Parameters.
        
        For example:
        var Parameters = new Dictionary<string,object>();
        Parameters.Add("Product",Product_A.Id);

        DO NOT expose this function! SQL Inject vulnerable.
        */
        public IEnumerable<Instruction> Get(Dictionary<string,object> WhereParameters)
        {
            if (WhereParameters.Count == 0)
            {
                //throw exception if empty Dictionary is passed in (consider use GetAll()?)
                throw new ArgumentNullException();
            }
            using(IDbConnection dbConnection = Connection)
            {
                //Create the sql querry dynamically, this is the starter.
                string sQuerry = @"select I.Step,I.Component,I.Action,I.Parameter,I.Id,
                P.Id,P.ProductName,P.Description,P.Price from Instruction I Inner JOIN Product P on I.Product=P.Id WHERE I.Id = @InsId WHERE";
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
                    sQuerry = sQuerry + $" {item.Key} = @{item.Key}";//new where clause.
                    dp.Add($"{item.Key}",item.Value);//add new parameter in dp
                    if (item.Key != last.Key) //No dulpicated key will occur because every key in dictionary is unique (C# rule)
                    {
                        sQuerry = sQuerry + " AND";
                    }
                }
                
                dbConnection.Open();
                return dbConnection.Query<Instruction,Product,Instruction>(sQuerry,(I,P)=>{I.Product=P;return I;},dp,splitOn:"Id").AsEnumerable();
            }
        }
    }
}