using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using tellick_admin.Repository;

namespace tellick_admin.Controllers {
    [Route("api/[controller]")]
    public class ProjectController : Controller {
        private GenericRepository<Customer> _customerRepository;
        private GenericRepository<Project> _projectRepository;

        public ProjectController(TellickAdminContext context) {
            _customerRepository = new GenericRepository<Customer>(context);
            _projectRepository = new GenericRepository<Project>(context);
        }

        [HttpGet(Name = "GetAllProjects")]
        public IActionResult GetAllProjects() {
            return Ok(_projectRepository.SearchFor(includeProperties: "Customer"));
        }

        [HttpGet("{name}", Name = "GetProject")]
        public IActionResult GetProject(string name) {
            Project p = _projectRepository.SearchFor(i => i.Name == name, includeProperties: "Customer").SingleOrDefault();
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Project item) {
            if (item == null) return BadRequest();

            Customer c = _customerRepository.GetByID(item.CustomerId);
            if (c == null) BadRequest("CustomerId does not exist");

            _projectRepository.Insert(item);
            _projectRepository.Save();
            
            return CreatedAtRoute("GetProject", new { id = item.Id }, item);
        }
    }
}