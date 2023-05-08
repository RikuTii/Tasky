using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Tasky.Models
{
    public class TaskList
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; }
        public int CreatorID { get; set; }
        public virtual UserAccount? Creator { get; set; }
        public virtual ICollection<TaskListMeta>? TaskListMetas { get; set; }
        public virtual ICollection<Task>? Tasks { get; set; }
        public virtual ICollection<UserAccount>? Users { get; set; }


    }
}
