using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUsers> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
