using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public enum Status
    {
        ToDo = 1,       
        InProgress = 2, 
        Done = 3        
    }
    public class TaskItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(50)]
        public string Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; } = string.Empty;

        [Required]
        public Status Status { get; set; } = Status.ToDo;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? AssignedUserId { get; set; }

        [ForeignKey("AssignedUserId")]
        public virtual User? AssignedUser { get; set; }
    }
}
