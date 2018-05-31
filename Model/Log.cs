using System;

namespace OrderManSys.Model
{
    //This class is for mapping Log table in the database.
    public class Log
    {
        public int ID{get;set;}
        public string type{get;set;}
        public string Author{get;set;}
        public string Message{get;set;}
        public DateTime Time{get;set;}

    }
}