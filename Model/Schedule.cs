using System;
using OrderManSys.Model;

//Model class for Factory.Schedule in database

namespace OrderManSys.Model
{
    public class Schedule
    {
        public int Id{get; set;} //Primary Ket
        public bool? Working{get; set;} //True for Orders that's currently running by the production line
        //Maps to Orders
        public Orders Orders{get; set;}
    }
}