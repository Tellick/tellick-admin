using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tellick_admin.Repository;

namespace tellick_admin.Controllers {
    [Authorize]
    [Route("api/[controller]")]
    public class CustomerController : Controller {
        private GenericRepository<Customer> _customerRepository;

        public CustomerController(TellickAdminContext context) {
            _customerRepository = new GenericRepository<Customer>(context);
        }

        [HttpGet(Name = "GetAllCustomers")]
        public IActionResult GetAllCustomers() {
            return Ok(_customerRepository.SearchFor());
        }

        [HttpGet("{name}", Name = "GetCustomer")]
        public IActionResult GetCustomer(string name) {
            Customer c = _customerRepository.SearchFor(i => i.Name == name).SingleOrDefault();
            if (c == null) return NotFound();
            return Ok(c);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Customer item) {
            if (item == null) return BadRequest("No customer data provided.");

            Customer c = _customerRepository.SearchFor(i => i.Name == item.Name).SingleOrDefault();
            if (c != null) return BadRequest("Customer name already exists.");

            _customerRepository.Insert(item);
            _customerRepository.Save();
            
            return CreatedAtRoute("GetCustomer", new { id = item.Id }, item);
        }
    }
}