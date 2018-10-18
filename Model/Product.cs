using System;
using System.Data;
using System.ComponentModel.DataAnnotations;

//Model class for Factory.Product in database

namespace OrderManSys.Model
{
    public class Product
    {
        public int Id{get; set;} //Primary Key
        [Required]
        public string ProductName{get; set;}
        public string Description{get; set;}
        [Required]
        public double Price{get; set;}

    }
}