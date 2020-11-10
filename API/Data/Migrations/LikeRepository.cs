using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Migrations
{
    public class LikeRepository : ILikeRepository
    {
        private readonly DataContext context;
        public LikeRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedId)
        {
            return await context.Likes.FindAsync(sourceUserId, likedId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = context.Likes.AsQueryable();
            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(l => l.SourceUserId == likesParams.UserId);
                users = likes.Select(l => l.LikedUser);
            }
            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(l => l.LikedUserId == likesParams.UserId);
                users = likes.Select(l => l.SourceUser);
            }
            var likedUsers = users.Select(
                usr => new LikeDto
                {
                    Username = usr.UserName,
                    KnownAs = usr.KnownAs,
                    Age = usr.DateOfBirth.CalculateAge(),
                    PhotoUrl = usr.Photos.FirstOrDefault(p => p.IsMain).Url,
                    City = usr.City,
                    Id = usr.Id,

                }
            );
            return await PagedList<LikeDto>.CreateAsync(likedUsers,
            likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUsers> GetUserWithLikes(int userId)
        {
            return await this.context.Users.Include(u => u.LikedUsers).FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
