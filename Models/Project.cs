using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace TSAIdentity.Models;
public enum ProjectStatus
{
    [Display(Name = "Pending")]
    Pending,

    [Display(Name = "Ongoing")]
    Ongoing,

    [Display(Name = "Completed")]
    Completed
}
public class Project
{
    [Key]
    public Guid ProjectId { get; set; }

    [Required]
    [DisplayName("Project Name")]
    [MaxLength(50, ErrorMessage = "Name of Project should have at most 50 characters.")]
    public string ProjectName { get; set; }

    [Required]
    [DisplayName("Project Description")]
    [MaxLength(256, ErrorMessage = "Name of Project should have at most 256 characters.")]
    public string ProjectDescription { get; set; }

    [Required]
    [DisplayName("Project Deadline")]
    public DateTime ProjectDeadline { get; set; }


    [DisplayName("Project Status")]
    public ProjectStatus ProjectStatus { get; set; }= ProjectStatus.Pending;

    // Navigation property for tasks
    public ICollection<ProjectTask>? ProjectTasks { get; set; }

    // Navigation property for employee
    public ICollection<EmployeeProject>? EmployeeProjects { get; set; }
    // Navigation property for organization
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
