using Microsoft.AspNetCore.Identity;

namespace TaskManager.Models
{
    public class User : IdentityUser
    {
        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
