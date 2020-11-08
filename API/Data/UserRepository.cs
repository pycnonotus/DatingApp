using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await this.context.Users.Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider).SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParam)
        {
            var query = this.context.Users.AsQueryable();

            query = query.Where(q => q.UserName != userParam.CurrentUsername);
            query = query.Where(q => q.Gender == userParam.Gender);
            var minDob = DateTime.Today.AddYears(-userParam.MaxAge - 1); // max age
            var maxDob = DateTime.Today.AddYears(-userParam.MinAge); // right
            query = query.Where(q => q.DateOfBirth >= minDob && q.DateOfBirth <= maxDob);



            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(this.mapper.ConfigurationProvider).AsNoTracking()
            , userParam.PageNumber, userParam.PageSize);
        }

        public async Task<IEnumerable<AppUsers>> GetUserAsync() => await context.Users.Include(p => p.Photos).ToListAsync();
        public async Task<AppUsers> GetUserByIdAsync(int id)
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<AppUsers> GetUserByUsernameAsync(string username)
        {
            return await context.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.UserName == username);
        }
        public async Task<bool> SaveAllAsync() => await context.SaveChangesAsync() > 0;

        public void Update(AppUsers user)
        {
            this.context.Entry(user).State = EntityState.Modified;
        }
    }
}
