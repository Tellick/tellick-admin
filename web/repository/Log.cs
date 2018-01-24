using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace tellick_admin.Repository {
    public class Log {
        public int Id { get; set; }
        public float Hours { get; set; }
        public string Message { get; set; }
        public DateTime ForDate { get; set; }
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }
    }
}
