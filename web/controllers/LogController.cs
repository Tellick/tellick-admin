using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using tellick_admin.Repository;

namespace tellick_admin.Controllers {
    [Route("api/[controller]")]
    public class LogController : Controller {
        private GenericRepository<Project> _projectRepository;
        private GenericRepository<Log> _logRepository;

        public LogController(TellickAdminContext context) {
            _projectRepository = new GenericRepository<Project>(context);
            _logRepository = new GenericRepository<Log>(context);
        }

        [HttpGet(Name = "GetAllLogs")]
        public IActionResult GetAllLogs() {
            return NotFound();
        }

        [HttpGet("{projectName}", Name = "GetLogByProjectName")]
        public IActionResult GetLogByProjectName(string projectName) {
            Log[] logs = _logRepository.SearchFor(i => i.Project.Name == projectName, includeProperties: "Project").ToArray();
            return Ok(logs);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Log item) {
            if (item == null) return BadRequest();

            Project p = _projectRepository.GetByID(item.ProjectId);
            if (p == null) BadRequest("Project does not exist");

            _logRepository.Insert(item);
            _logRepository.Save();
            
            return CreatedAtRoute("GetLogByProjectName", new { projectName = p.Name }, item);
        }
    }
}