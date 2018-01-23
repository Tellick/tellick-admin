using System.ComponentModel.DataAnnotations.Schema;

namespace tellick_admin.Repository {
    public class Project {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }
}
