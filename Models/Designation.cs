using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace TSAIdentity.Models;
public class Designation
{
    [Key]
    public Guid DesignationId { get; set; }

    [Required]
    [DisplayName("Designation Name")]
    [MaxLength(30, ErrorMessage = "Designation Name must be atmost 30 character.")]
    public string DesignationName { get; set; }

    // Navigation property for employees
    public ICollection<Employee>? Employees { get; set; }

    // Navigation property for tasks
    public ICollection<TaskDesignation>? TaskDesignations { get; set; }

    // Navigation property for organization
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
