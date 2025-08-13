using ChoSV.Data;
using ChoSV.Models.DTOs.User;
using ChoSV.Models.Entities;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ChoSV.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDBContext _dbContext;
        private readonly SignInManager<User> _signInManager;
        public UserService(UserManager<User> userManager, IEmailService emailService, ITokenService tokenService, ApplicationDBContext dBContext, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _emailService = emailService;
            _tokenService = tokenService;
            _dbContext = dBContext;
            _signInManager = signInManager;
        }

        public async Task<bool> RegisterAsync(RegisterDTO registerDTO)
        {
            var user = new User
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
                CreatedAt = DateTime.UtcNow,
            };

            var createUser = await _userManager.CreateAsync(user, registerDTO.Password);

            if (createUser.Succeeded)
            {
                var userRole = await _userManager.AddToRoleAsync(user, "User");
                if (userRole.Succeeded)
                {
                    var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _emailService.SendConfirmEmailAsync(registerDTO.Email, registerDTO.UserName, emailConfirmToken);
                    return true;
                }
                else throw new ArgumentException("Thêm vai trò thất bại!");
            }
            else throw new ArgumentException("Tạo người dùng thất bại!");
        }
    }
}
