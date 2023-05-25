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

            if (statusOut == 4)
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
                _context.Add(newTask);
                _context.SaveChanges();
            }
            else
            {
                var taskQuery = _context.Task.Where(e => e.Id == taskId).ToList();
                var task = taskQuery.ElementAt(0);
                task.Title = data["title"]?.ToString();
                _context.Update(task);
                _context.SaveChanges();
            }

        }
    }
}
