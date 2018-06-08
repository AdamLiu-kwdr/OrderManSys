using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrderManSys.Model;
using OrderManSys.Repository;
using OrderManSys.Engine;

namespace OrderManSys.Controllers
{
    public class ScheduleController : Controller
    {
        //Creating Repository instences
        private readonly OrderRepo orderRepo;
        private readonly ScheduleRepo scheduleRepo;
        private readonly InstructionRepo instructionrepo;

        //DI, injects connection strings
        public ScheduleController(IOptions<ConnectionStringOption> conn)
        {
            orderRepo = new OrderRepo(conn.Value.Factory);
            instructionrepo = new InstructionRepo(conn.Value.Factory);
            scheduleRepo = new ScheduleRepo(conn.Value.Factory);
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

        //Temp testing function, getting all Insturctions
        [HttpGet("/Inst")]
        public IEnumerable<Instruction> GetAllIns()
        {
            return instructionrepo.GetAll().OrderBy(x => x.id);
        }

    }
}