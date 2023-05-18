using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TSAIdentity.Models;

namespace TSAIdentity.ViewModels
{
    [NotMapped]
    public class PromoteViewModel
    {
        public Guid EmployeeId { get; set; }

        [Required]
        [DisplayName("Employee Name")]
        [MaxLength(50, ErrorMessage = "Name of employee should have at most 50 characters.")]
        public string? EmployeeName { get; set; }

        [DisplayName("Designation")]
        public Guid DesignationId { get; set; }
        public Designation? Designation { get; set; }
    }
}
