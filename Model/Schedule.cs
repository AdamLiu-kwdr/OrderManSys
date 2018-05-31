using System;
using OrderManSys.Model;

//Model class for Factory.Schedule in database

namespace OrderManSys.Model
{
    public class Schedule
    {
        public int Id{get; set;} //Primary Key

        //True for Orders that's currently running by the production line
        public bool? Working{get; set;} 
        
        //For recording currently finished products.
        public int ProducedCount{get;set;}
        public Orders Orders{get; set;}
    }
}