using System;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace OrderManSys.Model
{
    //This model class is for Instructions in the AutoManSys's Taskrunner and 
    //OrderManSys's Analyze engine. contains running instructions for lego motors.

    //Version 0.1
    public class Instruction
    {
        public int id{get;set;} //Unique ID
        public int Step{get;set;} //Instruction's sequence
        public string Component{get;set;} //Selecting the machines to use (Ballloader,Convyer etc...)
        public string Action{get;set;} //Which action the selected machine need to do
        public string Parameter{get;set;} //action's paraments.
        public Product Product{get; set;}
    }
}