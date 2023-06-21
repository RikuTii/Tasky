using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tasky.Data;
using Tasky.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Net.Http;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Validation;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Build.Framework;

namespace Tasky.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(AuthenticationSchemes = "IdentityServerJwtBearer")]
    [ApiController]
    [Route("[controller]")]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("Index")]
        // GET: Tasks
        public string Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            JwtSecurityToken token = null;

            if (Request.Headers.Keys.Contains("Authorization"))
            {
                StringValues values;

                if (Request.Headers.TryGetValue("Authorization", out values))
                {
                    var jwt = values.ToString();

                    if (jwt.Contains("Bearer"))
                    {
                        jwt = jwt.Replace("Bearer", "").Trim();
                    }

                    var handler = new JwtSecurityTokenHandler();

                    token = handler.ReadJwtToken(jwt);

                    if (token == null)
                        return "";

                    userId = token.Subject.ToString();
                }
            }

            ApplicationUser user = _context.Users.Where(e => e.Id == userId).Include(e => e.Account).First();
            if (user != null)
            {
                var tasklist = _context.TaskList.
                    Where(e => e.CreatorID == user.Account.Id).
                    Include(e => e.Tasks!.OrderBy(e => e.Ordering)).
                    ThenInclude(e => e.Creator).
                    Include(e => e.Creator).
                    Include(e => e.TaskListMetas!).
                    ThenInclude(e => e.UserAccount).
                    ToList();
                

                //clean list owner sensitive data
                var cleanTaskLists = new List<TaskList>();
                foreach (var task in tasklist)
                {
                    var list = new TaskList();

                    list = task;
                    if (list.TaskListMetas != null && !list.TaskListMetas.IsNullOrEmpty())
                    {
                        foreach (var meta in list.TaskListMetas)
                        {
                            meta.UserAccount.Avatar = "";
                            meta.UserAccount.Locale = "";
                            meta.UserAccount.UserID = "";
                        }
                    }

                    cleanTaskLists.Add(list);
                }

                var extraLists = _context.TaskListMeta.
                    Where(e => e.UserAccountID == user.UserAccountID).
                    Include(e => e.TaskList).
                    ThenInclude(e => e.Creator).
                    Include(e => e.TaskList).
                    ThenInclude(e => e.Tasks!).
                    ThenInclude(e => e.Creator).
                    ToList();
                var extraTasks = new List<TaskList>();
                if (!extraLists.IsNullOrEmpty())
                {
                    foreach (var v in extraLists)
                    {
                        extraTasks.Add(v.TaskList);
                    }
                }

                var tasks = new List<TaskList>();

                tasks.AddRange(cleanTaskLists);
                tasks.AddRange(extraTasks);


                return JsonSerializer.Serialize(tasks, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });
            }

            return "";
        }

        bool IsAuthorizedToTaskList(TaskList list)
        {
            if (list != null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                ApplicationUser user = _context.Users.Where(e => e.Id == userId).Include(e => e.Account).First();
                if (user != null)
                {
                    if(list.CreatorID == user.Account.Id)
                    {
                        return true;
                    }

                    var metas = _context.TaskListMeta.Where(e => e.TaskListID == list.Id).ToList();
                    foreach(var item in metas)
                    {
                        if(item.UserAccountID == user.Account.Id)
                        {
                            return true;
                        }
                    }


                }
            }
            return false;
        }

        [HttpGet("TaskList")]
        public IResult TaskList([FromQuery] int taskListId)
        {
            Console.WriteLine(taskListId) ;
            if (taskListId > 0)
            {
                var tasklist = _context.TaskList.Where(e => e.Id == taskListId).Include(e => e.Tasks!.OrderBy(e => e.Ordering)).First();
                if(tasklist != null)
                {
                    if(IsAuthorizedToTaskList(tasklist))
                    {
                        return Results.Ok(tasklist);
                    }
                }
            }

            return Results.BadRequest();
        }

        [HttpPost("CreateTaskList")]
        public void CreateTaskList([Bind("Name,Description")] Tasky.Models.TaskList task)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                ApplicationUser user = _context.Users.Where(e => e.Id == userId).Include(e => e.Account).First();
                if (user != null)
                {
                    task.CreatedDate = DateTime.Now;
                    task.CreatorID = user.Account.Id;
                    task.Creator = user.Account;
                    _context.Add(task);
                    _context.SaveChangesAsync();
                }
            }

        }
        [HttpPost("ShareTaskList")]
        public void ShareTaskList([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();
            if (Int32.TryParse(idString, out int id))
            {
                var email = data["email"]?.ToString();
                UserAccount account = _context.UserAccount.Where(e => e.Email == email).First();
                if (account != null)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    ApplicationUser authUser = _context.Users.Where(e => e.Id == userId).Include(e => e.Account).First();
                    if (authUser != null)
                    {
                        //make sure caller owns the tasklist
                        TaskList tasklist = _context.TaskList.Where(e => e.Id == id).First();
                        if (tasklist.CreatorID == authUser.Account.Id)
                        {
                            TaskListMeta meta = new()
                            {
                                TaskListID = id,
                                UserAccountID = account.Id,
                                Id = null
                            };
                            _context.Add(meta);
                            _context.SaveChanges();
                        }
                    }
                }
            }
        }
        [HttpPost("RemoveShareTaskList")]
        public void RemoveShareTaskList([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var idString = data["id"]?.ToString();
            if (Int32.TryParse(idString, out int id))
            {
                var email = data["email"]?.ToString();
                UserAccount account = _context.UserAccount.Where(e => e.Email == email).First();
                if (account != null)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    ApplicationUser authUser = _context.Users.Where(e => e.Id == userId).Include(e => e.Account).First();
                    if (authUser != null)
                    {
                        //make sure caller owns the tasklist
                        TaskList tasklist = _context.TaskList.Where(e => e.Id == id).First();
                        if (tasklist.CreatorID == authUser.Account.Id)
                        {    
                            _context.Remove(_context.TaskListMeta.Where(e => e.TaskListID == id).Where(e => e.UserAccountID == account.Id).First());
                            _context.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
