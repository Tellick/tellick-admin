using System;
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

        [HttpGet("{projectName}", Name = "GetLog")]
        public IActionResult GetLog(string projectName) {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            Log[] logs = _logRepository.SearchFor(i => i.Project.Name == projectName && i.ForDate.Month == month && i.ForDate.Year == year, includeProperties: "Project").ToArray();
            return Ok(logs);
        }

        [HttpGet("{projectName}/{dateSpecification}", Name = "GetLogByProjectName")]
        public IActionResult GetLogSpecific(string projectName, string dateSpecification) {
            Project p = _projectRepository.SearchFor(i => i.Name == projectName).SingleOrDefault();
            if (p == null) BadRequest("Project does not exist");
            
            // DateSpecification is either yyyy or yyyy-M and nothing else
            string[] parts = dateSpecification.Split('-');
            if (parts.Length == 1) {
                int year;
                if (Int32.TryParse(parts[0], out year)) {
                    Log[] logs = _logRepository.SearchFor(i => i.Project.Name == projectName && i.ForDate.Year == year, includeProperties: "Project").ToArray();
                    return Ok(logs);
                } else {
                    return BadRequest();
                }
            }
            if (parts.Length == 2) {
                int year;
                int month;
                if (Int32.TryParse(parts[0], out year) && Int32.TryParse(parts[1], out month) && month >= 1 && month <= 12) {
                    Log[] logs = _logRepository.SearchFor(i => i.Project.Name == projectName && i.ForDate.Month == month && i.ForDate.Year == year, includeProperties: "Project").ToArray();
                    return Ok(logs);
                } else {
                    return BadRequest();
                }
            }
            return BadRequest();
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