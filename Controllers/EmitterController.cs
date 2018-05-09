using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace OrderManSys.Controllers
{
    //This controller is a prototype. For testing connection between OrderMan and AutoMan.
    [Route("[controller]")]
    public class EmitterController : Controller
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
                HttpResponseMessage response = await client.GetAsync("http://ev3dev/Diagno/CheckService");
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


    }
}