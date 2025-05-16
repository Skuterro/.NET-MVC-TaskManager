using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class TaskChangeLog
    {
        public int Id { get; set; }

        [Required]
        public int TaskItemId { get; set; }

        [ForeignKey("TaskItemId")]
        public virtual TaskItem TaskItem { get; set; }

        [Required]
        public string ChangedByUserId { get; set; } 

        [ForeignKey("ChangedByUserId")]
        public virtual User ChangedByUser { get; set; }

        [Required]
        public DateTime ChangeTimestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string ChangeType { get; set; }

        [StringLength(255)]
        public string? FieldName { get; set; } 

        [Column(TypeName = "text")]
        public string? OldValue { get; set; }

        [Column(TypeName = "text")]
        public string? NewValue { get; set; }
    }
}
