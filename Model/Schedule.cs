using System;
using OrderManSys.Model;

//Model class for Factory.Order in database

namespace OrderManSys.Model
{
    public class Schedule
    {
        public int Id{get; set;} //Primary Ket
        public int? Quantity{get; set;}
        public bool? Working{get; set;} //True for Orders that's currently running by the production line
        public int OrderId{get; set;} //Forigen Key for Referrencing OrderId
        public Orders Orders{get; set;}
    }
}