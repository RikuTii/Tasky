﻿using System.ComponentModel.DataAnnotations;
using static Tasky.Controllers.TaskController;
using static Tasky.Controllers.TaskController.TaskyStatus;
namespace Tasky.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }
        public int CreatorId { get; set; }
        public virtual UserAccount Creator { get; set; }
        public int TaskListID { get; set; }
        public virtual TaskList TaskList { get; set; }
        public TaskyStatus? Status { get; set; }
        public int Ordering { get; set; }

    }
}
