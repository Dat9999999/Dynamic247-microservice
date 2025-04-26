using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsPage.Models;
using NewsPage.Models.entities;
using NewsPage.repositories.interfaces;
using System.Security.Claims;

namespace NewsPage.Controllers.User
{
    [Route("/api/v1/[controller]")]
    public class UserController: ControllerBase
    {
        private readonly IUserDetailRepository _userDetailRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        public UserController(IUserDetailRepository userDetailRepository, IUserAccountRepository userAccountRepository)
        {
            _userDetailRepository = userDetailRepository;
            _userAccountRepository = userAccountRepository;
        }


        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> Profile(Guid accountId)
        {
            try
            {
                var userDetails = await _userDetailRepository.GetDetailByAccountID(accountId);
                if (userDetails == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }

                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new {message = "Lỗi hệ thống khi tải thông tin người dùng"});
            }
        }
        [HttpGet("Get-details-by-mail")]
        public async Task<IActionResult> Detail([FromQuery] string mail)
        {
            try
            {
                var userAccount = await _userAccountRepository.GetByEmail(mail);
                var userDetails = await _userDetailRepository.GetDetailByAccountID(userAccount.Id);
                
                if (userDetails == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }

                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new {message = "Lỗi hệ thống khi tải thông tin người dùng"});
            }
        }
        [HttpGet("Get-details-by-account-id")]
        public async Task<IActionResult> DetailByid([FromQuery] Guid id)
        {
            try
            {
                var userAccount = await _userAccountRepository.GetById(id);
                if(userAccount.Role == "Admin") return Ok(new UserDetails{});
                var userDetails = await _userDetailRepository.GetDetailByAccountID(id);
                
                if (userDetails == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }

                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new {message = "Lỗi hệ thống khi tải thông tin người dùng"});
            }
        }
        [HttpGet("Get-mail-by-account-id")]
        public async Task<IActionResult> AccountByMail([FromQuery] Guid id)
        {
            try
            {
                var userAccount = await _userAccountRepository.GetById(id);
                
                if (userAccount == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }
               
                return Ok(userAccount.Email);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new {message = "Lỗi hệ thống khi tải thông tin người dùng"});
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> updatePrfile(Guid AccountId, [FromForm] UpdateProfileDTO updateProfileDTO)
            {

                try
                {
                    var userAccount = await _userAccountRepository.GetById(AccountId);
                    if (userAccount == null)
                    {
                        return NotFound(new {message = "Người dùng không tồn tại"});
                    }
                    var userEmailFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
                    var roleFromToken = User.FindFirst(ClaimTypes.Role)?.Value;
                    Console.WriteLine(userEmailFromToken);
                    Console.WriteLine(userAccount.Email);
                    if (roleFromToken != "Admin" && userEmailFromToken != userAccount.Email)
                    {
                        return BadRequest(new { message = "Bạn không có quyền chỉnh sửa đối với người dùng này" });
                    }

                    await _userDetailRepository.UpdateProfile(userAccount.Id, updateProfileDTO);

                    return Ok(new {success = true, message = "Cập nhật thông tin thành công" });
                }
                catch (Exception e){
                    Console.WriteLine(e);
                    return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống khi thực hiện chỉnh sửa thông tin người dùng. Vui lòng thử lại sau!" });
                }
            }
        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetListUser(int page = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                if(page < 1 ||  pageSize < 1)
                {
                    return BadRequest(new { message = "Thông tin phân trang không hợp lệ" });
                }
                var query = _userAccountRepository.GetAll();
                // Lọc theo tên hoặc email nếu có từ khóa tìm kiếm
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(u => u.Email.Contains(search));
                }
                int totalUsers = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalUsers /(double)pageSize);
                var paginatedUsers = await query
                    .Skip((page -1) * pageSize)
                    .Take(pageSize)
                    .ToArrayAsync();
                return Ok(new
                {
                    success = true,
                    currentPage = page,
                    totalPages = totalPages,
                    totalUsers = totalUsers,
                    pageSize = pageSize,
                    data = paginatedUsers
                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, new { message = "Lỗi hệ thống trong lúc lấy danh sách người dùng" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("lock")]
        public async Task<IActionResult> LockAccount(Guid accountId)
        {
            try
            {
                await _userAccountRepository.LockAccount(accountId);
                return Ok(new { message = "Khóa tài khoản người dùng thành công", accountId});

            }
            catch (Exception e) { 
                Console.WriteLine(e) ;
                return StatusCode(500, new { message = "Lỗi hệ thống khi thực hiện khóa người dùng" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("unlock")]
        public async Task<IActionResult> UnLockAccount(Guid accountId)
        {
            try
            {
                await _userAccountRepository.UnLockAccount(accountId);
                return Ok(new { message = "Mở khóa tài khoản người dùng thành công", accountId });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, new { message = "Lỗi hệ thống khi thực hiện mở khóa người dùng" });
            }
        }


    }
}
