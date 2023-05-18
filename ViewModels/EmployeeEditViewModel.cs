using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using TSAIdentity.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace TSAIdentity.ViewModels
{
    [NotMapped]
    public class EmployeeEditViewModel
    {
        public Guid EmployeeId { get; set; }

        [Required]
        [DisplayName("Employee Name")]
        [MaxLength(50, ErrorMessage = "Name of employee should have at most 50 characters.")]
        public string EmployeeName { get; set; }

        [Required]
        [DisplayName("Employee Contact")]
        [StringLength(10, ErrorMessage = "Contact should be of 10 characters.")]
        [DataType(DataType.PhoneNumber)]
        public string EmployeeContact { get; set; }
    
        // Navigation property for organization
        public Guid OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        // some thing for holding skills id
        [NotMapped]
        [DisplayName("Skills")]
        public List<Guid>? SkillIds { get; set; }
        [NotMapped]
        public ICollection<Skill>? Skills { get; set; }

        // Navigation property for skills
        public ICollection<EmployeeSkill>? EmployeeSkills { get; set; }
    }
}
