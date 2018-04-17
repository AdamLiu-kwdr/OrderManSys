using System;
using System.Data;
using System.ComponentModel.DataAnnotations;

//Model class for Factory.Order in database

namespace OrderManSys.Model
{
    public class Orders
    {
        public int Id{get;set;} //Primary Key
        [Required] //ModelBinding Valdiation
        public string Product{get; set;} //Should be ProductName, Later on will add in reference to actual Product
        public int Quantity{get; set;}
        [Required] //ModelBinding Valdiation
        public DateTime FinishTime{get;set;}
        public DateTime OrderTime{get; set;}
        [Required]
        public string OrderName{get;set;}
        public bool? Finished{get; set;} //True for Orders that's finished
    }
}