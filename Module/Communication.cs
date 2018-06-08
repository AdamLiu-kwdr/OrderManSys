using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json; //Serializer
using OrderManSys.Model;
using OrderManSys.Repository;
using System.Collections.Generic;
using System.Linq;

namespace OrderManSys.Module
{
    //Using HttpClient class to send http request. Unified string method will be studied later.
    //private static readonly HttpClient client = new HttpClient{BaseAddress=new Uri("")};
    public class Communication
    {
        string OrderManSysAddress;
        LogRepo logrepo;
        HttpClient client = new HttpClient();
        HttpResponseMessage response = new HttpResponseMessage();

        public Communication(ConnectionStringOption conn)
        {
            OrderManSysAddress = conn.OrderManSys;
            logrepo = new LogRepo(conn.Factory);
        }

        //This method will send POST with serualized object as body
        //Using HttpClient class to send http request to OrderManSys. Later or I "should" build URL with paraments.
        //Like so: private static readonly HttpClient client = new HttpClient{BaseAddress=new Uri("")};
        public async Task<HttpResponseMessage> SendAsync(string Controller, string Action, object toSerialize) //is object as parameter bad?
        {

            //Using HttpClient class to send http request.
            var JsonContent = new StringContent(JsonConvert.SerializeObject(toSerialize), Encoding.UTF8, "application/json");
            try
            {
                //Construct the URL for HttpClient.
                response = await client.PostAsync($"{OrderManSysAddress}{Controller}/{Action}", JsonContent);
                //Ensure the response is successful (No timeout/not found.)
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                //Write Error to internal record
                logrepo.Create(new Log
                {
                    type = "Warning",
                    Author = "CommunicationModule@OrderManSys",
                    Message = $"Connection to AutoManSys failed"
                }
                );

                //Write error detail as Debug message.
                Console.WriteLine($"[Debug]:Request failed when sending request to http://192.168.0.103/{Controller}/{Action} status code:{response.StatusCode}");
                throw e;
            }
            //Return OK with status code from AutoManSys (Should be 200!)
            return response;
        }

        //This overloaded method will send GET with parameters in URL
        public async Task<HttpResponseMessage> SendAsync(string Controller, string Action, string parameters) //is object as parameter bad?
        {
            //Using HttpClient class to send http request.
            try
            {
                //The fixed ip for now is 192.168.0.100, will move to appsettings.json later.
                response = await client.GetAsync($"{OrderManSysAddress}{Controller}/{Action}{parameters}");
                //Ensure the response is successful (No timeout/not found.)
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                //Write Error to internal record
                logrepo.Create(new Log
                {
                    type = "Warning",
                    Author = "CommunicationModule@OrderManSys",
                    Message = $"Connection to AutoManSys failed"
                }
                );

                //Write error detail as Debug message.
                Console.WriteLine($"[Debug]:Request failed when sending request to http://192.168.0.103/{Controller}/{Action}{parameters} status code:{response.StatusCode}");
                throw e;
            }
            //Return OK with status code from AutoManSys (Should be 200!)
            return response;
        }
    }
}