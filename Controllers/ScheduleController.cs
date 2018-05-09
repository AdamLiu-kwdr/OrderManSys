using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using OrderManSys.Model;
using OrderManSys.Repository;
using OrderManSys.Sequence;

namespace OrderManSys.Controllers
{
    public class ScheduleController : Controller
    {
        //Depandancy Injection. Injecting OderRepo in Repository folder.
        private readonly OrderRepo orderRepo;
        private readonly ScheduleRepo scheduleRepo;
        public ScheduleController() //Class constructer
        {
            orderRepo = new OrderRepo();
            scheduleRepo = new ScheduleRepo();
        }

        // [Get]/Schedule  Activate Scheduling engine, return schedule table.
        [HttpGet]
        [Route("/Schedule")]
        public IEnumerable<Schedule> RunSequence()
        {
            //Get all the current unfinished orders
            var QParameter = new Dictionary<string,object>(){{"Finished",false}};
            var Orders = orderRepo.Get(QParameter);

            //Initate new Sequence engine and get the schedule.
            SequenceEngine Engine = new SequenceEngine();
            var result = Engine.CreateSchedule(Orders.ToList());

            //Clear all existing records and Write the Schedule back to database.
            scheduleRepo.DropAll();
            scheduleRepo.Create(result);

            //send resullt back.
            return result;
        }

        //Return all current schedules
        [HttpGet]
        public IEnumerable<Schedule> GetAll()
        {
            return scheduleRepo.GetAll();
        }

    }
}