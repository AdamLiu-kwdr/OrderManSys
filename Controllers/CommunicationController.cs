using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderManSys.Module;
using OrderManSys.Model;
using OrderManSys.Engine;
using OrderManSys.Repository;

namespace OrderManSys.Controllers
{
    //This controller is for communcting between OrderMan and AutoMan. So bascailly it's more like a communcation class.
    //Yes, it's pretty big.
    [Route("[controller]")]
    public class CommunicationController : Controller
    {
        private readonly ConnectionStringOption _connectionstring;
        private readonly Communication comm;
        private readonly BlueprintEngine BluePrint;
        private readonly OrderRepo orderRepo;
        private readonly LogRepo logRepo;
        private readonly InstructionRepo instrepo;
        private readonly ProductRepo productRepo;
        private readonly ScheduleRepo scheRepo;

        //Depandency Injection
        public CommunicationController(IOptions<ConnectionStringOption> ConnectionString)
        {
            _connectionstring = ConnectionString.Value;
            comm = new Communication(_connectionstring);
            instrepo = new InstructionRepo(_connectionstring.Factory);
            logRepo = new LogRepo(_connectionstring.Factory);
            orderRepo = new OrderRepo(_connectionstring.Factory);
            productRepo = new ProductRepo(_connectionstring.Factory);
            scheRepo = new ScheduleRepo(_connectionstring.Factory);
            BluePrint = new BlueprintEngine(_connectionstring);
        }

        //Check Connection to automan, Won't initalize sensors so it's much faster.
        //Location:[Host]/CheckConnection
        [Route("/CheckConnection")]
        public IActionResult CheckConnection()
        {
           //send http request.
            try
            {
                //Use HttpClient to check if AutoMan service is up and running.
                var response = comm.SendAsync("InstructionRunner", "CheckService", "").Result;
                //Ensure the response is successful (No timeout/not found.)
                response.EnsureSuccessStatusCode();
            }
            catch (System.Exception)
            {
                //If CheckService request failed, return 503 Service Unavailable.
                //Don't Write log to database from here, Communication module already does that.
                return StatusCode(503,"Connection timeout");
            }
            //Return OK with status code from AutoManSys (Should be 200!)
            return Ok("Automan:" + Response.StatusCode.ToString());
        }
        
        //Check AutoManSys' Lego status (Will initalize Automan's sensors too.)
        //Location:[Host]/CheckService
        [Route("/CheckService")]
        public IActionResult CheckService()
        {
            //send http request.
            try
            {
                //Use HttpClient to check if AutoMan service is up and running.
                var response = comm.SendAsync("Diagno", "CheckService", "").Result;
                //Ensure the response is successful (No timeout/not found.)
                response.EnsureSuccessStatusCode();
            }
            catch (System.Exception)
            {
                //If CheckService request failed, return 503 Service Unavailable.
                //Don't Write log to database from here, Communication module already does that.
                return StatusCode(503,"Connection timeout");
            }
            //Return OK with status code from AutoManSys (Should be 200!)
            return Ok("Automan:" + Response.StatusCode.ToString());
        }
        
        //Return Log by takeQuantity parament (default:10)
        [HttpGet("/Log")]
        public IEnumerable<Log> GetLog(int takeQuantity = 10)
        {
            return logRepo.GetAll().Take(takeQuantity);
        }

        //Send first instruction set to AutoMan and starts working.
        //Location:[Host]/Run
        //1.Get next schedule in database
        //2.get the instructions for scheduled product from BluePrintEngine.
        //3.Send to AutoManSys.
        [HttpGet]
        [Route("/Run")]
        public IActionResult RunSchedule([FromQuery(Name = "Contiune")]bool ContiuneMode)
        {
            //1.Get next schedule in database
            var nextSchedule = scheRepo.GetAll().First();

            //2.get the instructions for scheduled product from BluePrintEngine.
            var InstructionSets = BluePrint.GetRegistred(orderRepo.GetbyId(nextSchedule.Orders.Id).product);
            //Console.WriteLine($"[Debug]:Running Instructions for:{InstructionSets.First().Product.ProductName}");

            //3.Send to AutoManSys.
            logRepo.Create(new Log { Author = "CommunicationController@OrderManSys", type = "Info", Message = "Sending AutoManSys Instructions." });
            
            HttpResponseMessage response = new HttpResponseMessage(); 
            response = comm.SendAsync("InstructionRunner","Execute",InstructionSets,$"?Contiune={ContiuneMode}").Result;

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(503,response.Content.ToString());   
            }
            return Accepted($"First running Schedule Id:{nextSchedule.Id}");
        }


        //This method is for AutoManSys ONLY (or other process in future), to report a log.
        [HttpPost("LogReport")]
        public IActionResult LogReport([FromBody][Bind("type,Author,Message")] Log model)
        {
            //Only accepting Log type, Log Author and message. rest will be created later.
            //Create a LogRepo class and call the create method
            model.Time = DateTime.Now;
            logRepo.Create(model);

            Dictionary<string, object> para = new Dictionary<string, object>();
            para.Add("Message", model.Message);
            para.Add("Author", model.Author);
            return Ok(logRepo.Get(para));
        }

        // This method is for AutoManSys ONLY (or other process in future), to report when finished executing instructions.
        // List of things will happen here:
        // 1.Check if the execution successed. 
        // 2.Update current Schedule and check if there's still unfinished schedules need to run. 
        // 3.Delete current Schedule if ProducedCount = order's quantity. And update Order.
        // 4.Write Log
        // 5.Redirect to Sned instructions action if contiune = true
        [HttpGet("ResultReport")]
        public IActionResult ResultReport([FromQuery(Name = "Success")]bool success, [FromQuery(Name = "Contiune")]bool Contiune)
        {

            logRepo.Create(new Log { Author = "CommunicationController@OrderManSys", type = "Info", Message = $"AutoManSys reported, checking result..." });

            //1.Check if AutoManSys reports Unsuccessful result. and exit program.
            if (success == false)
            {
                logRepo.Create(new Log { Author = "CommunicationController@OrderManSys", type = "Warn", Message = "AutoManSys reports Action unsuccessful. Production halted!" });
                return Ok();
            }

            //2.Update current Schedule and check if there's still unfinished schedules need to run. 
            var currentSchedule = scheRepo.GetAll().First();
            //But we will check if there's Schedule there first.
            if (currentSchedule == null)
            {
                logRepo.Create(new Log
                {
                    Author = "CommunicationController@OrderManSys",
                    type = "Error",
                    Message = "execution successed but " +
                " schedule table is empty. (Was schedule table modified during production?)."
                });
                return Ok();
            }
            else
            {
                // 3.Delete current Schedule if ProducedCount = order's quantity. And update Order.
                currentSchedule.ProducedCount++;
                if (currentSchedule.ProducedCount >= currentSchedule.Orders.Quantity)
                {
                    //Current Schedule finished. Log.
                    logRepo.Create(new Log { Author = "CommunicationController@OrderManSys", type = "Info", Message = $"AutoManSys reports execution successed. Finished execution current Schedule, for Order with Id:{currentSchedule.Orders.Id}" });
                    
                    //Update the order, set Finished to true.
                    var currentOrder = orderRepo.GetbyId(currentSchedule.Orders.Id);
                    currentOrder.Finished = true;
                    orderRepo.Update(currentOrder);
                    
                    //Delete Schedule
                    scheRepo.Delete(currentSchedule.Id);
                    
                    
                    //Check if this is last schedule.
                    var nextSchedule = scheRepo.GetAll().First();
                    if (nextSchedule == null)
                    {
                        logRepo.Create(new Log { Author = "CommunicationController@OrderManSys", type = "Info", Message = "AutoManSys reports execution successed. All schedule done." });
                        return Ok();
                    }
                    else
                    {
                        nextSchedule.Working = true;
                        scheRepo.Update(nextSchedule);
                    }
                }
                else
                {
                    scheRepo.Update(currentSchedule);
                }
            }

            //Console.WriteLine($"[Debug]:Contiune mode: {Contiune}");

            // 4.Write Log
            logRepo.Create(new Log { Author = "CommunicationController@OrderManSys", type = "Info", Message = "AutoManSys reports execution successed." });

            // 5.Redirect to Sned instructions action if contiune = true 
            // (Means We're running all the schedules in the databast and there's still schedule need to run)
            if (Contiune == true)
            {
                //Console.WriteLine("[DEBUG]Redirecting to run.");
                //send automansys NEXT instructions.
                return RedirectToAction("RunSchedule",new{Contiune = true});
            }

            return Ok();
        }

        [HttpGet]
        [Route("/Instruct")]
        //DeBug Method, get all the current registreted instructions.
        public IEnumerable<Instruction> Get()
        {
            var blue = new BlueprintEngine(_connectionstring);
            return blue.GetRegistred(productRepo.GetbyId(1));
        }


    }
}