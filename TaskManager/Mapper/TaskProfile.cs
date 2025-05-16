using AutoMapper;
using TaskManager.Dtos;
using TaskManager.Models;
using static TaskManager.Dtos.ChangeLogDtos;

namespace TaskManager.Mapper
{
    public class TaskProfile : Profile
    {
        public TaskProfile() 
        {
            CreateMap<TaskItem, TaskResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("o")))
                .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.UserName : null));

            CreateMap<CreateTaskDto, TaskItem>();

            CreateMap<UpdateTaskDto, TaskItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<TaskChangeLog, ChangeLogResponseDto>()
                .ForMember(dest => dest.TaskItemTitle, opt => opt.MapFrom(src => src.TaskItem != null ? src.TaskItem.Title : "N/A"))
                .ForMember(dest => dest.TaskAssignedUserName, opt => opt.MapFrom(src =>
                     src.TaskItem != null && src.TaskItem.AssignedUser != null
                     ? src.TaskItem.AssignedUser.UserName : "N/A")) 
                .ForMember(dest => dest.ChangedByUserName, opt => opt.MapFrom(src => src.ChangedByUser != null ? src.ChangedByUser.UserName : "N/A"))
                .ForMember(dest => dest.ChangeTimestamp, opt => opt.MapFrom(src => src.ChangeTimestamp.ToString("dd.MM.yyyy HH:mm:ss")));
        }
    }
}
