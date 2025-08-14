using ChoSV.Models.DTOs.User;

namespace ChoSV.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(RegisterDTO registerDTO);
        Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO);
        Task ConfirmEmailAsync(string email, string token);
    }
}
