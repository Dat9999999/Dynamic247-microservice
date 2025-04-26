using NewsPage.Models;
using NewsPage.Models.entities;

namespace NewsPage.repositories.interfaces
{
    public interface IUserDetailRepository
    {
        Task<UserDetails> CreateInfo(string FullName, string sex, DateTime Birthday, Guid userAccountId);
        Task<UserDetails> GetDetailByAccountID(Guid accountID);

        Task UpdateProfile(Guid accountId ,UpdateProfileDTO updateProfileDTO);
    }
}
