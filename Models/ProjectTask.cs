namespace TSAIdentity.Models
{
    public class ProjectTask
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        public Guid TaskId { get; set; }
        public Tasks Task { get; set; }
    }
}
