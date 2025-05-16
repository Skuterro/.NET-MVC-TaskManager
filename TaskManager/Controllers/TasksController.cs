using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TaskManager.Dtos;
using AutoMapper;
using static TaskManager.Dtos.ChangeLogDtos;


namespace TaskManager.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        //private readonly ILogger<TasksController> _logger;

        public TasksController(AppDbContext context, UserManager<User> userManager, IMapper mapper/*, ILogger<TasksController> logger*/)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            //_logger = logger;
        }

        // GET: Tasks/Index
        public async Task<IActionResult> Index()
            {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<TaskItem> tasksEntities;
            bool isAdmin = User.IsInRole("Admin");
            ViewBag.IsAdmin = isAdmin;

            if (isAdmin)
            {
                tasksEntities = await _context.Tasks
                    .Include(t => t.AssignedUser)
                    .OrderByDescending(t => t.UpdatedAt)
                    .ToListAsync();
                    ViewBag.Users = await _userManager.Users.Select(u => new { u.Id, u.UserName }).ToListAsync();
            }
            else
            {
                tasksEntities = await _context.Tasks
                    .Where(t => t.AssignedUserId == currentUserId)
                    .Include(t => t.AssignedUser)
                    .OrderByDescending(t => t.UpdatedAt)
                    .ToListAsync();
            }

            var tasksDtos = _mapper.Map<List<TaskResponseDto>>(tasksEntities);

            string statusToDoString = TaskManager.Models.Status.ToDo.ToString(); 
            string statusInProgressString = TaskManager.Models.Status.InProgress.ToString();
            string statusDoneString = TaskManager.Models.Status.Done.ToString();

            ViewBag.ToDoTasks = tasksDtos.Where(t => t.Status == statusToDoString).ToList();
            ViewBag.InProgressTasks = tasksDtos.Where(t => t.Status == statusInProgressString).ToList();
            ViewBag.DoneTasks = tasksDtos.Where(t => t.Status == statusDoneString).ToList();

            return View(tasksDtos);
        }

        // GET: Tasks/Details/5
        [HttpGet("/Tasks/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            var taskEntity = await _context.Tasks
                                .Include(t => t.AssignedUser)
                                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskEntity == null)
            {
                return NotFound(new { success = false, message = "Task not found." });
            }

            var taskResponseDto = _mapper.Map<TaskResponseDto>(taskEntity);
            return Json(taskResponseDto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeLogHistory(string sortBy = "timestamp_desc", string searchTaskTitle = null, string searchUser = null, int page = 1)
        {
            const int pageSize = 15;

            IQueryable<TaskChangeLog> logsQuery = _context.TaskChangeLogs
                .Include(log => log.TaskItem)
                    .ThenInclude(task => task.AssignedUser)
                .Include(log => log.ChangedByUser);

            if (!string.IsNullOrEmpty(searchTaskTitle))
            {
                logsQuery = logsQuery.Where(log => log.TaskItem != null && log.TaskItem.Title.ToLower().Contains(searchTaskTitle.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchUser))
            {
                logsQuery = logsQuery.Where(log =>
                    (log.TaskItem != null && log.TaskItem.AssignedUser != null && log.TaskItem.AssignedUser.UserName.ToLower().Contains(searchUser.ToLower())) ||
                    (log.ChangedByUser != null && log.ChangedByUser.UserName.ToLower().Contains(searchUser.ToLower()))
                );
            }

            switch (sortBy)
            {
                case "tasktitle_asc":
                    logsQuery = logsQuery.OrderBy(log => log.TaskItem != null ? log.TaskItem.Title.ToLower() : "");
                    break;
                case "tasktitle_desc":
                    logsQuery = logsQuery.OrderByDescending(log => log.TaskItem != null ? log.TaskItem.Title.ToLower() : "");
                    break;
                case "assigneduser_asc":
                    logsQuery = logsQuery.OrderBy(log => log.TaskItem != null && log.TaskItem.AssignedUser != null ? log.TaskItem.AssignedUser.UserName.ToLower() : "");
                    break;
                case "assigneduser_desc":
                    logsQuery = logsQuery.OrderByDescending(log => log.TaskItem != null && log.TaskItem.AssignedUser != null ? log.TaskItem.AssignedUser.UserName.ToLower() : "");
                    break;
                case "timestamp_asc":
                    logsQuery = logsQuery.OrderBy(log => log.ChangeTimestamp);
                    break;
                default:
                    logsQuery = logsQuery.OrderByDescending(log => log.ChangeTimestamp);
                    break;
            }

            var totalItems = await logsQuery.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = page;

            var logsEntities = await logsQuery
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            var logsDtos = _mapper.Map<List<ChangeLogResponseDto>>(logsEntities);

            ViewBag.CurrentSortBy = sortBy;
            ViewBag.TaskTitleSort = sortBy == "tasktitle_asc" ? "tasktitle_desc" : "tasktitle_asc";
            ViewBag.AssignedUserSort = sortBy == "assigneduser_asc" ? "assigneduser_desc" : "assigneduser_asc";
            ViewBag.TimestampSort = sortBy == "timestamp_asc" ? "timestamp_desc" : "timestamp_asc";
            ViewBag.SearchTaskTitle = searchTaskTitle;
            ViewBag.SearchUser = searchUser;

            return View(logsDtos);
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/Tasks/Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto createTaskDto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, errors = new[] { "User cannot be identified." } });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            if (!string.IsNullOrEmpty(createTaskDto.AssignedUserId))
            {
                var assignedUserExists = await _userManager.FindByIdAsync(createTaskDto.AssignedUserId) != null;
                if (!assignedUserExists)
                {
                    return Json(new { success = false, errors = new[] { "User does not exist." } });
                }
            }

            var taskEntity = _mapper.Map<TaskItem>(createTaskDto);
            taskEntity.Status = Status.ToDo;
            taskEntity.CreatedAt = DateTime.UtcNow;
            taskEntity.UpdatedAt = DateTime.UtcNow;

            _context.Add(taskEntity);
            try
            {
                await _context.SaveChangesAsync();

                var changeLog = new TaskChangeLog
                {
                    TaskItemId = taskEntity.Id,
                    ChangedByUserId = currentUserId,
                    ChangeTimestamp = DateTime.UtcNow,
                    ChangeType = "Created task.",
                    FieldName = "Title",
                    OldValue = null,
                    NewValue = taskEntity.Title,
                };

                _context.TaskChangeLogs.Add(changeLog);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred.", details = ex.Message });
            }

            var createdTaskWithUser = await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == taskEntity.Id);

            var responseDto = _mapper.Map<TaskResponseDto>(createdTaskWithUser);
            return Json(new { success = true, task = responseDto });
        }

        // GET: Tasks/Edit/5
        [HttpPost("/Tasks/Edit")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit([FromBody] UpdateTaskDto updateTaskDto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, errors = new[] { "User cannot be identified." } });
            }

            var taskEntity = await _context.Tasks.FindAsync(updateTaskDto.Id);
            if (taskEntity == null)
            {
                return Json(new { success = false, message = "Task not found." });
            }

            var originalTitle = taskEntity.Title;
            var originalDescription = taskEntity.Description;
            var originalAssignedUserId = taskEntity.AssignedUserId;
            var originalAssignedUserName = taskEntity.AssignedUser?.UserName;

            _mapper.Map(updateTaskDto, taskEntity);
            taskEntity.UpdatedAt = DateTime.UtcNow;

            var logs = new List<TaskChangeLog>();
            bool hasChanges = false;

            if (originalTitle != taskEntity.Title)
            {
                logs.Add(new TaskChangeLog
                {
                    TaskItemId = taskEntity.Id,
                    ChangedByUserId = currentUserId,
                    ChangeTimestamp = taskEntity.UpdatedAt,
                    ChangeType = "Field updated",
                    FieldName = "Title",
                    OldValue = originalTitle,
                    NewValue = taskEntity.Title,
                });
                hasChanges = true;
            }
            if (originalDescription != taskEntity.Description)
            {
                logs.Add(new TaskChangeLog
                {
                    TaskItemId = taskEntity.Id,
                    ChangedByUserId = currentUserId,
                    ChangeTimestamp = taskEntity.UpdatedAt,
                    ChangeType = "Field updated",
                    FieldName = "Description",
                    OldValue = originalDescription,
                    NewValue = taskEntity.Description,
                });
                hasChanges = true;
            }
            if (originalAssignedUserId != taskEntity.AssignedUserId)
            {
                var newAssignedUser = !string.IsNullOrEmpty(taskEntity.AssignedUserId) ? await _userManager.FindByIdAsync(taskEntity.AssignedUserId) : null;
                string newAssignedUserName = newAssignedUser?.UserName ?? "N/A";
                logs.Add(new TaskChangeLog
                {
                    TaskItemId = taskEntity.Id,
                    ChangedByUserId = currentUserId,
                    ChangeTimestamp = taskEntity.UpdatedAt,
                    ChangeType = "Assigned user changed",
                    FieldName = "AssignedUserId",
                    OldValue = originalAssignedUserName,
                    NewValue = newAssignedUserName,
                });
                hasChanges = true;
            }

            try
            {
                if (hasChanges)
                {
                    _context.Update(taskEntity);
                }
                if (logs.Any())
                {
                    _context.TaskChangeLogs.AddRange(logs);
                }
                if(hasChanges || logs.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred.", details = ex.Message });
            }

            var updatedTaskWithUser = await _context.Tasks
                            .Include(t => t.AssignedUser)
                            .FirstOrDefaultAsync(t => t.Id == taskEntity.Id);

            var responseDto = _mapper.Map<TaskResponseDto>(updatedTaskWithUser);
            return Json(new { success = true, task = responseDto, message = "Task updated successfully!" });
        }

        // POST: /Tasks/UpdateStatus/{taskId}
        [HttpPost("/Tasks/UpdateStatus/{taskId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int taskId, [FromBody] UpdateTaskStatusDto statusDto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, errors = new[] { "User cannot be identified." } });
            }

            TaskItem taskEntity;

            if (User.IsInRole("Admin"))
            {
                taskEntity = await _context.Tasks.FindAsync(taskId);
            }
            else
            {
                taskEntity = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedUserId == currentUserId);
            }

            if (taskEntity == null)
            {
                return Json(new { success = false, message = "Task not found." });
            }

            if (taskEntity.Status != statusDto.NewStatus)
            {
                var oldStatus = taskEntity.Status.ToString();
                taskEntity.Status = statusDto.NewStatus;
                taskEntity.UpdatedAt = DateTime.UtcNow;

                var changeLog = new TaskChangeLog
                {
                    TaskItemId = taskEntity.Id,
                    ChangedByUserId = currentUserId,
                    ChangeTimestamp = taskEntity.UpdatedAt,
                    ChangeType = "Status changed",
                    FieldName = "Status",
                    OldValue = oldStatus,
                    NewValue = taskEntity.Status.ToString(),
                };

                _context.TaskChangeLogs.Add(changeLog);
                _context.Update(taskEntity);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An unexpected error occurred.", details = ex.Message });
                }

            }
            return Json(new { success = true, message = "Task status has been updated." });
        }

        // POST: Tasks/Delete/5
        [HttpDelete("/Tasks/Delete/{taskId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int taskId)
        {
            var taskEntity = await _context.Tasks.FindAsync(taskId);
            if (taskEntity == null)
            {
                return Json(new { success = false, message = "Task not found." });
            }

            _context.Tasks.Remove(taskEntity);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Task has been deleted" });
            
        }
    }
}
