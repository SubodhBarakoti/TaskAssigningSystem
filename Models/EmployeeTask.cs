namespace TSAIdentity.Models
{
    public class EmployeeTask
    {
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public Guid TaskId { get; set; }
        public Tasks Task { get; set; }
    }
}
