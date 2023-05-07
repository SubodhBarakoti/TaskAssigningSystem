using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace TSAIdentity.Models;

public enum TaskStatus
{
    [Display(Name = "Pending")]
    Pending,

    [Display(Name = "In Progress")]
    InProgress,

    [Display(Name = "Completed")]
    Completed

}
public class Tasks
{
    [Key]
    public Guid TaskId { get; set; }

    [Required]
    [DisplayName("Task Name")]
    [MaxLength(50, ErrorMessage = "Name of Task should have at most 50 characters.")]
    public string TaskName { get; set; }

    [Required]
    [DisplayName("Task Description")]
    [MaxLength(256, ErrorMessage = "Description of Task should have at most 256 characters.")]
    public string TaskDescription { get; set; }

    [DisplayName("Task Status")]
    public TaskStatus TaskStatus { get; set; } = TaskStatus.Pending;

    [DefaultValue(false)]
    [DisplayName("Is Assigned")]
    public bool isassigned { get; set; }

    // Foreign key for required skill
    [DisplayName("Required Skill")]
    public Guid SkillId { get; set; }
    public Skill? Skill { get; set; }

    // Foreign key for assigned employee
    [DisplayName("Assigned To")]
    public Guid? AssignedEmployeeId { get; set; }
    public Employee? AssignedEmployee { get; set; }

    // Foreign key for project
    [DisplayName("Project")]
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    // Navigation property for organization
    [DisplayName("Organization")]
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
