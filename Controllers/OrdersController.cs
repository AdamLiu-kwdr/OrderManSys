using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderManSys.Model;
using OrderManSys.Repository;

namespace OrderManSys.Controllers
{
    [Route("[controller]")]
    public class OrdersController : Controller
    {
        //Depandancy Injection. Injecting OderRepo in Repository folder.
        private readonly OrderRepo orderRepo;
        private readonly ScheduleRepo scheduleRepo;
        public OrdersController() //Class constructer
        {
            orderRepo = new OrderRepo();
            scheduleRepo = new ScheduleRepo();
        }

        // [Get]/Order  Get all the Current Orders
        [HttpGet]
        public IEnumerable<Orders> GetAll()
        {
            return orderRepo.GetAll();
        }
        
        // [Get]/Order/id  Get the Order from id
        [HttpGet("{id}")]
        public Orders GetById(int id)
        {
            return orderRepo.GetbyId(id);
        }

        // [Post]/Order  Create new Order (Need testing.)
        /*
        This aaction will try to bind the passed in Orders from post body, JSOM format.
        Make sure setting Content-type header to application/json.
         */
        [HttpPost]
        public IActionResult Create([FromBody][Bind("Product,Quantity,FinishTime,OrderName")] Orders order)
        {
            if (!ModelState.IsValid) //Check the Model Binding 
            {
                return BadRequest(ModelState); //Return 403 If model binding failed failed.
            }
            order.OrderTime= DateTime.Now;
            order.Finished=false;
            orderRepo.InsertNew(order);
            //Because Dapper will drop milin seconds when converting Datetime
            //So this need better work.
            DateTime TheUnwated = order.OrderTime.Date.AddHours(order.OrderTime.Minute).AddMinutes(order.OrderTime.Minute).AddSeconds(order.OrderTime.Second);
            return Created($"/Order/{orderRepo.FindId(order.OrderName,TheUnwated)}", null); //Return the created object as responce. 
        }
    }
}