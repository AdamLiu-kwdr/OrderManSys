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
    //This Controller is for Creating/query orders from database.
    //Processing incoming API call.

    /*
    Functions:
    GetAll  [Get] Order
    GetByid [Get] Order/id
    Create [Post] Order/Create
    (Test) Dynamic [Get] Order/Dynamic
    */

    [Route("[controller]")]
    public class OrdersController : Controller
    {
        //Creating Repository instences
        private readonly OrderRepo orderRepo;

        public OrdersController(IOptions<ConnectionStringOption> conn)
        {
            orderRepo = new OrderRepo(conn.Value.Factory);
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
        public IActionResult Create([FromBody][Bind("Quantity,FinishTime,OrderName,Product.Id")] Orders order)
        {
            //Check the Model Binding 
            if (!ModelState.IsValid)
            {
                //Return 403 If model binding failed.
                return BadRequest(ModelState);
            }

            //Creating custom Datetime.Now Because Dapper will drop ticks when converting Datetime, causing error. >Need better soulution.
            DateTime TheUnwated = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);

            //Complete the rest of rows in Orders.
            order.OrderTime = TheUnwated;
            order.Finished = false;
            orderRepo.Create(order);

            //Find the newly created order by using orderRepo.Get(WhereParameters)
            var Parameters = new Dictionary<string, object>();
            Parameters.Add("OrderTime", order.OrderTime);
            Parameters.Add("OrderName", order.OrderName);
            return Created($"/Orders/{orderRepo.Get(Parameters).First().Id}", null);

        }

        //Testing function for orderRepo's dynamic query.
        [HttpGet("Dynamic")]
        public IEnumerable<Orders> Get()
        {
            var Parameters = new Dictionary<string, object>();
            Parameters.Add("Finished", true);

            return orderRepo.Get(Parameters);
        }

    }
}