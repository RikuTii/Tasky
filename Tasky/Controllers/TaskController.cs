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
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost("CreateOrUpdateTask")]
        public void CreateOrUpdateTask([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();
            if (!idString.IsNullOrEmpty())
            {
                Int32.TryParse(idString, out int taskId);
                if (_context.Task.Find(taskId) == null)
                {
                    var newTask = new Tasky.Models.Task();
                    if (Int32.TryParse(data["taskListId"]?.ToString(), out int taskListId))
                    {
                        newTask.TaskListID = taskListId;
                    }
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
}
