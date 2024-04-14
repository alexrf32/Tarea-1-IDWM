using System.Security.Cryptography;
using System.Text;
using courses_dotnet_api.Src.DTOs.Account;
using courses_dotnet_api.Src.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace courses_dotnet_api.Src.Controllers;

public class AccountController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;

    public AccountController(IUserRepository userRepository, IAccountRepository accountRepository)
    {
        _userRepository = userRepository;
        _accountRepository = accountRepository;
    }

    [HttpPost("register")]
    public async Task<IResult> Register(RegisterDto registerDto)
    {
        if (
            await _userRepository.UserExistsByEmailAsync(registerDto.Email)
            || await _userRepository.UserExistsByRutAsync(registerDto.Rut)
        )
        {
            return TypedResults.BadRequest("User already exists");
        }

        await _accountRepository.AddAccountAsync(registerDto);

        if (!await _accountRepository.SaveChangesAsync())
        {
            return TypedResults.BadRequest("Failed to save user");
        }

        AccountDto? accountDto = await _accountRepository.GetAccountAsync(registerDto.Email);

        return TypedResults.Ok(accountDto);
    }
    private bool ComparePassword(string password, byte[] storedHash, byte[] storedSalt){
    using var hmac = new HMACSHA512(storedSalt);
    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

    return computedHash.SequenceEqual(storedHash);
    }

    [HttpPost("login")]
    public async Task<IResult> Login(LoginDto loginDto)
    {
        var Email = await _userRepository.UserExistsByEmailAsync(loginDto.Email);

        if (!Email)
        {
            return TypedResults.BadRequest("Credentials are invalid");
        }

        var password = await _userRepository.GetPasswordDtoAsync(loginDto.Email);

        if(password == null)
        {
            return TypedResults.BadRequest("Credentials are invalid");
        }

        if(!ComparePassword(loginDto.Password, password.PasswordHash, password.PasswordSalt))
        {
            return TypedResults.BadRequest("Credentials are invalid");
        }

        AccountDto? account = await _accountRepository.GetAccountAsync(loginDto.Email);

        return TypedResults.Ok(account); 
    }
}
