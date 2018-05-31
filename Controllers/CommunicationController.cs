using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using OrderManSys.Model;
using System.Collections.Generic;
using System.Linq;
using OrderManSys.Engine;
using OrderManSys.Repository;

namespace OrderManSys.Controllers
{
    //This controller is for communcting between OrderMan and AutoMan. Bascailly it's more like a communcation class.
    [Route("[controller]")]
    public class CommunicationController : Controller
    {
        //Using HttpClient class to send http request. Unified string method will be studied later.
        //private static readonly HttpClient client = new HttpClient{BaseAddress=new Uri("")};
        //Location:[Host]/Emitter/CheckService
        [HttpGet("CheckService")]
        public async Task<IActionResult> CheckService()
        {
            //Using HttpClient class to send http request.
            HttpClient client = new HttpClient();
            try
            {
                //Use HttpClient to check if AutoMan service is up and running.
                HttpResponseMessage response = await client.GetAsync("http://192.168.0.103/Diagno/CheckService");
                //Ensure the response is successful (No timeout/not found.)
                response.EnsureSuccessStatusCode();
            }
            catch (System.Exception)
            {
                //If CheckService request failed, return 503 Service Unavailable.
                return StatusCode(503);
            }
            //Return OK with status code from AutoManSys (Should be 200!)
            return Ok(Response.StatusCode);
        }

        //This method is for AutoManSys ONLY (or other process in future) to report a log.
        //Only accepting Log type, Log Author and message. rest will be created later.
        [HttpPost]
        public IActionResult LogReport([FromBody][Bind("type,Author,Message")] Log model)
        {
            //Create a LogRepo class and call the create method
            model.Time = DateTime.Now;
            LogRepo repo = new LogRepo();
            repo.Create(model);

            Dictionary<string,object> para = new Dictionary<string,object>();
            para.Add("Message",model.Message);
            para.Add("Author",model.Author);
            return Ok(repo.Get(para));
        }

        [HttpGet("Instruct")]
        //DeBug Method, get all the current registreted instructions.
        public IEnumerable<Instruction> Get()
        {
            var product = new ProductRepo();
            var blue = new BlueprintEngine();
            return blue.GetRegistred(product.GetbyId(1));
        }

        
    }
}