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
    //This Controller is for Creating/query Product from database.

    /*
    Functions:
    GetAll  [Get] /Product
    GetById [Get] /Product/{id}
    Create  [Post] /Product
    Update  [PUT]  /Product/{id}
    Delete  [Delete] /Product/{id}
    */

    [Route("/Product")]
    public class ProductController : Controller
    {
        //Inject ProductRepo, for accessing products in database.
        private readonly ProductRepo productRepo;

        public ProductController(IOptions<ConnectionStringOption> conn)
        {
            productRepo = new ProductRepo(conn.Value.Factory);
        }


        // [Get]/Product  Get all the registered products
        [HttpGet]
        public IEnumerable<Product> GetAll()
        {
            return productRepo.GetAll();
        }

        // [Get]/Product/{id}  Get a single product from id
        [HttpGet("{id}")]
        public Product GetById(int id)
        {
          return productRepo.GetbyId(id);
        }

        // [Post]/Product  Create a new Product
        /*
        This aaction will try to bind the passed in Product from post body, as in JSOM format.
        Make sure setting Content-type header to application/json.
         */
        [HttpPost]
        public IActionResult CreateProduct([FromBody][Bind("ProductName,Description,Price")] Product product)
        {
            //Check the Model Binding, Return 403 If model binding failed.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //Create Product
            productRepo.Create(product);
            
            //Find the newly created order by using productRepo.Get(WhereParameters)
            var Parameters = new Dictionary<string, object>();
            Parameters.Add("ProductName", product.ProductName);
            Parameters.Add("Price", product.Price);

            return Created($"/Product/{productRepo.Get(Parameters).First().Id}", null);
        }

        // [Post]/Product  Update a current Product or Create new product if non exits.
        /*This action will ignore the id from body and use the id from route insted.*/
        [HttpPut("{id}")]
        public IActionResult UpdateProduct([FromBody][Bind("ProductName,Description,Price")] Product model,[FromRoute]int id)
        {
            //Check the Model Binding, Return 403 If model binding failed.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            model.Id=id;
            //Update Product Product
            productRepo.Update(model);
            return Accepted();
        }

        //[Delete]/Product/{id} Delete a product from id
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                productRepo.Delete(id);
            }
            catch (Exception e)
                {
                    if (e.GetType() == typeof(KeyNotFoundException))
                    {
                        return NotFound(e.Message);
                    }
                    return StatusCode(500,e.Message.ToString());
                }
            return Accepted();
        }

    }
}