using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using OData.Models;
using System.Linq;

namespace OData.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly AdventureWorksLT2014Context _context;

        public ApiController(AdventureWorksLT2014Context context)
        {
            _context = context;
        }

        [HttpGet("customers")]
        [EnableQuery]
        public IQueryable<Customer> GetCustomers()
        {
            return _context.Customers;
        }

        [HttpGet("customers({id})")]
        [EnableQuery]
        public IActionResult GetCustomerById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = SingleResult.Create(_context.Customers.Where(p => p.CustomerId == id));

            return Ok(customer);
        }

        [HttpGet("customeraddress")]
        [EnableQuery(PageSize = 10)]
        public IActionResult GetCustomerAddressByCustomerId([FromQuery] int customerid)
        {
            var result = SingleResult.Create(_context.CustomerAddresses.Where(item => item.CustomerId == customerid));
            return Ok(result);
        }

        [HttpGet("products")]
        [EnableQuery(PageSize = 10)]
        public IQueryable<Product> GetProducts()
        {
            return _context.Products;
        }

        [HttpGet("custaddress")]
        [EnableQuery(PageSize = 10)]
        public IQueryable<CustomerAddress> GetCustomerAddresses()
        {
            return _context.CustomerAddresses;
        }
    }
}
