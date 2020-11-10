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
        }


    }
}
