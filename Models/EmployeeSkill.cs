namespace TSAIdentity.Models
{
    public class EmployeeSkill
    {
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public Guid SkillId { get; set; }
        public Skill Skill { get; set; }

        public static implicit operator Guid(EmployeeSkill v)
        {
            throw new NotImplementedException();
        }
    }
}
