using Microsoft.EntityFrameworkCore;
using NewsPage.data;
using NewsPage.Models.entities;
using NewsPage.repositories.interfaces;

namespace NewsPage.repositories
{
    public class UserAccountsRepository : IUserAccountRepository
    {
        private readonly ApplicationDbContext _context;
        public UserAccountsRepository(ApplicationDbContext context) {
            _context = context;
        
        }

        public async Task<Guid> CreateAccount(string email, string passwordHash, string role)
        {
            Guid Id = Guid.NewGuid();
            DateTime CreateAt = DateTime.Now;
            //Hash password 
            var newAccount = new UserAccounts
            {
                Id = Id,
                Email = email,
                Password = passwordHash, // Lấy từ appsettings.json
                Role = role,
                CreatedAt = CreateAt,
                Status = "Enable",
                IsVerified = false
            };
            await _context.UserAccounts.AddAsync(newAccount);
            await _context.SaveChangesAsync();
            return Id;

        }

        public IQueryable<UserAccounts> GetAll()
        {
            return _context.UserAccounts.AsQueryable();
        }

        public async Task<UserAccounts?> GetByEmail(string email)
        {
           return await _context.UserAccounts.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<UserAccounts?> GetById(Guid id)
        {
            return await _context.UserAccounts.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task LockAccount(Guid id)
        {
            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == id);
            if (userAccount == null)
            {
                throw new Exception("Người dùng không tồn tại để thực hiện khóa");
            }
            userAccount.Status = "Disable";
            await _context.SaveChangesAsync();
        }

        public async Task<UserAccounts> ResetPassword(string email, string newPassword)
        {
            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == email);
            if (userAccount != null)
            {
                userAccount.Password = newPassword;
            }
            await _context.SaveChangesAsync();
            return userAccount;
        }

        public async Task UnLockAccount(Guid id)
        {
            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == id);
            if (userAccount == null)
            {
                throw new Exception("Người dùng không tồn tại để thực hiện mở khóa");
            }
            userAccount.Status = "Enable";
            await _context.SaveChangesAsync();
        }

        public async void VerifyEmail(string email, UserAccounts user)
        {
            try
            {
                user.IsVerified = true;
                await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }


    }
}
