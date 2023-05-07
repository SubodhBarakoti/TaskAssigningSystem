using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TSAIdentity.Models;
public class Organization
{
    [Key]
    public Guid OrganizationId { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Organization Name")]
    public string OrganizationName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Organization Email")]
    public string OrganizationEmail { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Organization Password")]
    [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^\da-zA-Z]).{6,}$",
        ErrorMessage = "The password must contain at least one digit, one lowercase character, one uppercase character, and one non-alphanumeric character.")]
    public string OrganizationPassword { get; set; }

    [NotMapped]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("OrganizationPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    // Navigation property
    public ICollection<Employee>? Employees { get; set; }
    public ICollection<Designation>? Designations { get; set; }
    public ICollection<Project>? Projects { get; set; }
    public ICollection<Tasks>? Tasks { get; set; }
    public ICollection<Skill>? Skills { get; set; }
}