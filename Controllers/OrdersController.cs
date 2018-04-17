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
            //Check the Model Binding 
            if (!ModelState.IsValid) 
            {
                //Return 403 If model binding failed failed.
                return BadRequest(ModelState); 
            }

            //Creating custom Datetime.Now Because Dapper will drop ticks when converting Datetime
            //need better soulution.
            DateTime TheUnwated = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);

            //Complete rows in Orders.
            order.OrderTime= TheUnwated;
            order.Finished=false;
            orderRepo.InsertNew(order);
            
            //Return the created object index as responce. 
            return Created($"/Order/{orderRepo.FindId(order.OrderName,order.OrderTime)}", null);
        }
    }
}