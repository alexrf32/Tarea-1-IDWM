using courses_dotnet_api.Src.DTOs.Account;

namespace courses_dotnet_api.Src.Interfaces;

public interface IUserRepository
{
    Task<bool> UserExistsByEmailAsync(string email);
    Task<bool> UserExistsByRutAsync(string rut);
    Task<PasswordDto?> GetPasswordDtoAsync(string email);
    Task<bool> SaveChangesAsync();
}
