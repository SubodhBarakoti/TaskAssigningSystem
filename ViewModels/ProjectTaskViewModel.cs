using System.ComponentModel.DataAnnotations.Schema;
using TSAIdentity.Models;

namespace TSAIdentity.ViewModels
{
    [NotMapped]
    public class ProjectTaskViewModel
    {
        public Project Project { get; set; }
        public IEnumerable<Tasks> Tasks { get; set; }
    }
}
