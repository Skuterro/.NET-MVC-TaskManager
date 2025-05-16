using System.ComponentModel.DataAnnotations;
using TaskManager.Models;

namespace TaskManager.Dtos
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(50, ErrorMessage = "The title cannot be longer than 50 characters.")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "The description cannot be longer than 500 characters.")]
        public string? Description { get; set; }

        public string? AssignedUserId { get; set; }
    }

    public class UpdateTaskDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(50, ErrorMessage = "The title cannot be longer than 50 characters.")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "The description cannot be longer than 500 characters.")]
        public string? Description { get; set; }

        public string? AssignedUserId { get; set; }
    }


    public class UpdateTaskStatusDto
    {
        [Required]
        public Status NewStatus { get; set; }
    }


    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } 
        public string CreatedAt { get; set; } 
        public string UpdatedAt { get; set; } 
        public string? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
    }
}
