using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
using TSAIdentity.Models;

[NotMapped]
public class AssignTaskViewModel
{
    public Guid TaskId { get; set; }

    public string TaskName { get; set; }

    [Display(Name = "Assigned Employee")]
    [Required(ErrorMessage = "Please select an employee")]
    public Guid SelectedEmployeeId { get; set; }
}
