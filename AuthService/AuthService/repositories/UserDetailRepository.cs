using Microsoft.EntityFrameworkCore;
using NewsPage.data;
using NewsPage.helpers;
using NewsPage.Models;
using NewsPage.Models.entities;
using NewsPage.repositories.interfaces;

namespace NewsPage.repositories
{
    public class UserDetailRepository : IUserDetailRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly FileHelper _fileHelper;
        public UserDetailRepository(ApplicationDbContext context, FileHelper fileHelper)
        {
            _context = context;
            _fileHelper = fileHelper;
        }
        public async Task<UserDetails> CreateInfo(string FullName, string sex, DateTime Birthday, Guid userAccountId)
        {
            Guid id = Guid.NewGuid();
            var userInfo = new UserDetails
            {
                Id = id
                ,
                FullName = FullName,
                Sex = sex,
                Birthday = Birthday,
                UserAccountId = userAccountId,
                Avatar = "default_avatar.jpg"
            };
            await _context.UserDetails.AddAsync(userInfo);
            await _context.SaveChangesAsync();
            return userInfo;

        }

        public async Task<UserDetails> GetDetailByAccountID(Guid accountID)
        {
            var userDetail = await _context.UserDetails.FirstOrDefaultAsync(u => u.UserAccountId == accountID);

            return userDetail;
        }

        public async Task UpdateProfile(Guid accountId ,UpdateProfileDTO updateProfileDTO)
        {
            var userDetail = await _context.UserDetails.FirstOrDefaultAsync(u => u.UserAccountId==accountId);

            if (userDetail == null) throw new Exception("Người dùng không tồn tại để chỉnh sửa ");

            userDetail.FullName = updateProfileDTO.FullName;
            userDetail.Birthday = updateProfileDTO.Birthday;
            if (updateProfileDTO.Avatar != null)
            {
                string avatarPath = await _fileHelper.SaveFileAsync(updateProfileDTO.Avatar);
                userDetail.Avatar = avatarPath;
            }
            userDetail.Sex = updateProfileDTO.Sex;
            await _context.SaveChangesAsync();
        }
    }
}
