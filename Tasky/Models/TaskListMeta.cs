using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tasky.Models
{
    public class TaskListMeta
    {
        public int? Id { get; set; }
        [Key]
        [ForeignKey("TaskList")]
        public int TaskListID { get; set; }
        public int UserAccountID { get; set; }
        public virtual UserAccount UserAccount { get; set; }
        public virtual TaskList TaskList { get; set; }
    }
}
