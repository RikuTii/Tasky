using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Tasky.Data;
using Tasky.Models;

namespace Tasky.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;


        public enum TaskyStatus
        {
            NotCreated,
            NotDone,
            Done
        }

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("CreateOrUpdateTask")]
        public void CreateOrUpdateTask([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();

            Int32.TryParse(idString, out int taskId);
            Int32.TryParse(data["taskListId"]?.ToString(), out int taskListId);
            Int32.TryParse(data["status"]?.ToString(), out int statusOut);

            if (((TaskyStatus)statusOut) == TaskyStatus.NotCreated)
            {
                var newTask = new Tasky.Models.Task();
                newTask.TaskListID = taskListId;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ApplicationUser authUser = _context.Users.Where(e => e.Id == userId).Include(e => e.Account).First();
                if (authUser != null)
                {
                    newTask.CreatorId = authUser.Account.Id;
                }
                newTask.Title = data["title"]?.ToString();
                newTask.CreatedDate = DateTime.Now;
                newTask.Status = TaskyStatus.NotDone;
                _context.Add(newTask);
                _context.SaveChanges();
            }
            else
            {
                Console.WriteLine(taskId);
                var taskQuery = _context.Task.Where(e => e.Id == taskId).ToList();
                var task = taskQuery.ElementAt(0);
                task.Title = data["title"]?.ToString();
                task.Status = (TaskyStatus)statusOut;

                _context.Update(task);
                _context.SaveChanges();
            }

        }
    }
}
