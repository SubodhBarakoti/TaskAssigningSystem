using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TSAIdentity.Models;

public class Employee
{
    [Key]
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

    [Required]
    [DisplayName("Employee Email")]
    [DataType(DataType.EmailAddress)]
    public string? EmployeeEmail { get; set; }

    [Required]
    [DisplayName("Password")]
    [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^\da-zA-Z]).{6,}$",
        ErrorMessage = "The password must contain at least one digit, one lowercase character, one uppercase character, and one non-alphanumeric character.")]
    public string EmployeePassword { get; set; }

    [NotMapped]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("EmployeePassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    // Foreign key for designation
    [DisplayName("Designation")]
    public Guid DesignationId { get; set; }
    public Designation? Designation { get; set; }

    [DefaultValue(false)]
    public bool IsBusy { get; set; }

    [DefaultValue(true)]
    public bool IsActive { get; set; }

    // some thing for holding skills id
    [NotMapped]
    [DisplayName("Skills")]
    public List<Guid>? SkillIds { get; set; }
    [NotMapped]
    public ICollection<Skill>? Skills { get; set; }
    // Navigation property for skills
    public ICollection<EmployeeSkill>? EmployeeSkills { get; set; }

    // Navigation property for projects
    public ICollection<EmployeeProject>? EmployeeProjects { get; set; }

    // Navigation property for assigned tasks
    public ICollection<EmployeeTask>? EmployeeTasks { get; set; }

    // Navigation property for organization
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
