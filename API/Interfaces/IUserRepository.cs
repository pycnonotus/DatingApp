using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;

namespace API.Interfaces {
    public interface IUserRepository {
        void Update (AppUsers user);
        Task<bool> SaveAllAsync ();
        Task<IEnumerable<AppUsers>> GetUserAsync ();
        Task<AppUsers> GetUserByIdAsync (int id);
        Task<AppUsers> GetUserByUsernameAsync (string username);
        Task<IEnumerable<MemberDto>> GetMembersAsync ();
        Task<MemberDto> GetMemberAsync (string username);

    }
}
