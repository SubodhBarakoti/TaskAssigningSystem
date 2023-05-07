namespace TSAIdentity.Models
{
    public class TaskDesignation
    {
        public Guid TaskId { get; set; }
        public Tasks Task { get; set; }

        public Guid DesignationId { get; set; }
        public Designation Designation { get; set; }
    }
}
