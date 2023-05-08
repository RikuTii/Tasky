using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;
using Tasky.Models;


namespace Tasky.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions)
            : base(options, operationalStoreOptions)
        {

        }
        public DbSet<Tasky.Models.Task> Task { get; set; } = default!;
        public DbSet<UserAccount> UserAccount { get; set; } = default!;
        public DbSet<TaskList> TaskList { get; set; } = default!;
        public DbSet<TaskListMeta> TaskListMeta { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            Console.WriteLine("created db");

            modelBuilder.Entity<ApplicationUser>()
              .HasOne(e => e.Account)
              .WithOne(e => e.User)
              .HasForeignKey<ApplicationUser>(e => e.UserAccountID);

            modelBuilder.Entity<TaskList>()
            .HasMany(e => e.Tasks)
            .WithOne(e => e.TaskList)
            .HasForeignKey(e => e.TaskListID)
            .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<TaskList>()
           .HasMany(e => e.Users)
           .WithOne()
           .HasForeignKey(e => e.Id);

/*           modelBuilder.Entity<TaskListMeta>()
            .HasOne(e => e.TaskList)
            .WithOne()
            .HasForeignKey<TaskList>(e => e.Id);
            /*
            modelBuilder.Entity<TaskList>()
            .HasOne(e => e.Creator)
            .WithOne()
            .HasForeignKey<TaskList>(e => e.CreatorID);*/

            modelBuilder.Entity<Tasky.Models.Task>()
            .HasOne(e => e.Creator)
            .WithOne()
            .HasForeignKey<Tasky.Models.Task> (e => e.CreatorId);
            /* modelBuilder.Entity<TaskList>().HasMany<TaskListMeta>().WithOne("TaskListID");
             modelBuilder.Entity<TaskList>().HasMany<Tasky.Models.Task>().WithOne();
             modelBuilder.Entity<ApplicationUser>().HasOne<UserAccount>().WithOne("UserAccountID");
             modelBuilder.Entity<UserAccount>().HasOne<ApplicationUser>().WithOne("ApplicationUserID");*/


        }
    }
}