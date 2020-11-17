using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext :
    IdentityDbContext
    <
        AppUsers, AppRole, int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>,
        IdentityUserToken<int>
    >
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUsers>().
            HasMany(
                ur => ur.UserRoles
            ).WithOne(u => u.User).HasForeignKey(u => u.UserId).IsRequired();

            modelBuilder.Entity<AppRole>().
            HasMany(
                ur => ur.UserRoles
            ).WithOne(u => u.Role).HasForeignKey(u => u.RoleId).IsRequired();

            modelBuilder.Entity<UserLike>().HasKey(k => new
            {
                k.SourceUserId,
                k.LikedUserId
            });
            modelBuilder.Entity<UserLike>()
            .HasOne(u => u.SourceUser)
            .WithMany(u => u.LikedUsers).HasForeignKey(u => u.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserLike>()
            .HasOne(u => u.LikedUser)
            .WithMany(u => u.LikedByUsers).HasForeignKey(u => u.LikedUserId)
            .OnDelete(DeleteBehavior.Cascade); // SQL SERVER change this to DeleteBehavior.NoAction



            modelBuilder.Entity<Message>().HasOne(u => u.Recipient)
            .WithMany(u => u.MessageReceived).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>().HasOne(u => u.Sender)
            .WithMany(u => u.MessageSend).OnDelete(DeleteBehavior.Restrict);
        }


    }
}
