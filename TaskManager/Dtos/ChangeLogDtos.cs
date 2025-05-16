namespace TaskManager.Dtos
{
    public class ChangeLogDtos
    {
        public class ChangeLogResponseDto
        {
            public int Id { get; set; } 
            public int TaskItemId { get; set; }
            public string TaskItemTitle { get; set; } 
            public string? TaskAssignedUserName { get; set; }
            public string ChangedByUserName { get; set; }
            public string ChangeTimestamp { get; set; }
            public string ChangeType { get; set; }      
            public string? FieldName { get; set; }     
            public string? OldValue { get; set; }      
            public string? NewValue { get; set; }   
        }
    }
}
