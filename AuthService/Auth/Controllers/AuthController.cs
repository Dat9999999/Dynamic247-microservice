
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NewsPage.helpers;
using NewsPage.Models;
using NewsPage.Models.entities;
using NewsPage.repositories.interfaces;

namespace NewsPage.Controllers.Auth
{
    [Route("/api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserDetailRepository _userDetailRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly PasswordHelper _passwordHelper;
        private readonly MailHelper _mailHelper;
        private readonly OtpHelper _otpHelper;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IUserAccountRepository userAccountsRepository, IUserDetailRepository userDetailRepository
            , JwtHelper jwtHelper, PasswordHelper passwordHelper, MailHelper mailHelper, OtpHelper otpHelper
            , ILogger<AuthController> logger
            )
        {
            _userAccountRepository = userAccountsRepository;
            _jwtHelper = jwtHelper;
            _passwordHelper = passwordHelper;
            _userDetailRepository = userDetailRepository;
            _mailHelper = mailHelper;
            _otpHelper = otpHelper;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AccountDTO accountDTO)
        {
            try{
                var email = accountDTO.Email;
                if (email == null)
                {
                    return BadRequest();
                }
                var userAccount = await _userAccountRepository.GetByEmail(email);

                if (userAccount == null || !_passwordHelper.VerifyPassword(accountDTO.Password, userAccount.Password))
                {
                    return NotFound(new { message = "Sai tên đăng nhập hoặc mật khẩu" });
                }
                if (!userAccount.IsVerified)
                {
                    return BadRequest(new { message = "Email của bạn chưa được xác thực, hãy xác thực email trước khi dăng nhập" });
                }

                if (userAccount.Status != "Enable")
                {
                    return BadRequest(new { message = "Tài khoản của bạn đã bị khóa, hãy liên hệ hỗ trợ nếu có sai sót" });

                }
                var token = _jwtHelper.GenerateJwtToken(userAccount.Email, userAccount.Role);
                _logger.LogInformation($"user {userAccount.Id} login successfully");
                return Ok(new { token });

            }
            catch(Exception e){
                _logger.LogError(e,"Get error at login");
                return NoContent();
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); // ❌ return error if invalid input
                }
                // block all request register invalid which contains unverify email
                //using cache to check if that email is verified by key {mail}_to_verify 
                
                
                if (registerDTO.Role == "Editor")
                {
                    return BadRequest(new { message = "Yêu cầu không hợp lệ" });
                }

                //Hash password
                string passwordHash = _passwordHelper.HashPassword(registerDTO.Password);

                //create account 
                Guid accountId = await _userAccountRepository.CreateAccount(registerDTO.Email, passwordHash, registerDTO.Role);

                //create user info 
                UserDetails userDetails = await _userDetailRepository.CreateInfo(registerDTO.FullName, registerDTO.Sex,
                    registerDTO.Birthday, accountId);

                return Ok(registerDTO);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { message = "Có lỗi khi tạo tài khoản" });
            }
        }

        [HttpPost("registerEditor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterEditorAccount([FromBody] RegisterDTO registerDTO)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); // ❌ return error if invalid input
                }
                if (await _userAccountRepository.GetByEmail(registerDTO.Email) is not null)
                {
                    return BadRequest(new { message = "Email đã tồn tại" });
                }
                //Hash password
                string passwordHash = _passwordHelper.HashPassword(registerDTO.Password);

                //create account 
                Guid accountId = await _userAccountRepository.CreateAccount(registerDTO.Email, passwordHash, registerDTO.Role);

                //create user info 
                UserDetails userDetails = await _userDetailRepository.CreateInfo(registerDTO.FullName, registerDTO.Sex,
                    registerDTO.Birthday, accountId);

                return Ok(registerDTO);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { message = "Có lỗi khi tạo tài khoản biên tập viên" });
            }
        }


        [HttpGet("verifyEmail")]

        //Send an otp mail to new user account 
        public async Task<IActionResult> VerifyEmail(string email)
        {
            try
            {
                //create subject 
                string subject = "Xác thực Email";
                //create otp 
                var otp = _otpHelper.GenerateOtp(email).Otp;

                //send email contains OTP code
                await _mailHelper.SendOtpEmailAsync(subject, email, otp);
                return Ok(new { message = "Email sent successfully!" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = "Có lỗi khi gửi email hoặc tạo mã" });
            }
        }

        [HttpPost("verifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtplDTO verifyOtpDTO)
        {
            try
            {
                var userAccount = await _userAccountRepository.GetByEmail(verifyOtpDTO.Email);
                if (userAccount == null)
                {
                    return BadRequest(new { message = "Email chưa đăng ký hoặc không tồn tại" });

                }
                if (!_otpHelper.ValidateOtp(verifyOtpDTO.Email, verifyOtpDTO.Otp))
                {
                    return BadRequest(new { message = "Mã OTP không chính xác hoặc hết hiệu lực" });
                }
                _userAccountRepository.VerifyEmail(verifyOtpDTO.Email, userAccount);
                return Ok(new { message = "Email xác thực thành công" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = "Có lỗi khi xác thực mã otp" });
            }
        }

        [HttpGet("resetPassword")]
        public async Task<IActionResult> RequestResetPassword(string email)
        {
            try
            {
                //create key to store in redis 
                string key = $"otp_resetPassword_{email}";

                //Create subject to send an email 
                string subject = "Xác nhận đổi mật khẩu";

                //create otp 
                string otp = _otpHelper.GenerateOtp(key).Otp;

                //send email
                await _mailHelper.SendOtpEmailAsync(subject, email, otp);
                return Ok(new { message = "Gửi mail otp xác thực thành công" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = "Gửi Otp thay đổi mật khẩu thất bại" });
            }
        }

        [HttpPatch("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                var email = changePasswordDTO.Email;


                //Hash password before store in to database
                var newPassword = _passwordHelper.HashPassword(changePasswordDTO.NewPassword);
                //create key to store in redis 
                string key = $"otp_resetPassword_{changePasswordDTO.Email}";

                bool isValid = _otpHelper.ValidateOtp(key, changePasswordDTO.Otp);
                if (!isValid)
                {
                    return BadRequest(new { message = "Mã OTP không hợp lệ hoặc đã hết hạn" });
                }

                //reset password 
                await _userAccountRepository.ResetPassword(email, newPassword);
                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = "Gửi Otp thay đổi mật khẩu thất bại" });
            }
        }
    }
}
