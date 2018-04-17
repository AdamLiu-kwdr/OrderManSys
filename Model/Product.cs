using System;
using System.Data;
using System.ComponentModel.DataAnnotations;

//Model class for Factory.Order in database

namespace OrderManSys.Model
{
    public class Product
    {
        public int Id{get; set;} //Primary Key
        public string ProductName{get; set;}
        public string Description{get; set;}
        public double Price{get; set;}

    }
}