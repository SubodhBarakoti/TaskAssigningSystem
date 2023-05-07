using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace TSAIdentity.Models;
public class Skill
{
    [Key]
    public Guid SkillId { get; set; }

    [Required]
    [DisplayName("Skill Name")]
    [MaxLength(30, ErrorMessage = "Skill name should have at most 30 characters.")]
    public string SkillName { get; set; }

    // Navigation property for employees
    public ICollection<EmployeeSkill>? EmployeeSkills { get; set; }

    // Navigation property for tasks :later deleted

    // Navigation property for organization
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
