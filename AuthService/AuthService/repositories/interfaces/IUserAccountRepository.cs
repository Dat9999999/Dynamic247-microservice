using NewsPage.Models.entities;

namespace NewsPage.repositories.interfaces
{
    public interface IUserAccountRepository
    {
        Task<UserAccounts?> GetByEmail(string email);
        Task<UserAccounts?> GetById(Guid id);
        Task<Guid> CreateAccount(string email, string passwordHash,string role);
        void VerifyEmail(string email, UserAccounts user);

        Task<UserAccounts> ResetPassword(string email, string newPassword);
        IQueryable<UserAccounts> GetAll();

        Task LockAccount(Guid id);
        Task UnLockAccount(Guid id);
    }
}
