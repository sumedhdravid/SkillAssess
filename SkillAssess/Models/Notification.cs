namespace SkillAssess.Models
{
    public class Notification
    {
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }

        public bool IsNew { get; set; }
    }
}
