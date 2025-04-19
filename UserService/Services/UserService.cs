using UserService.Models;

namespace UserService.Services;

public class UserManager
{
    private readonly List<User> _users = new();

    public User? GetUserByEmail(string email)
    {
        return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public User? GetUserById(Guid id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public User Register(RegisterDto registerDto)
    {
        if (GetUserByEmail(registerDto.Email) != null)
        {
            throw new Exception("User with this email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = registerDto.Name,
            Email = registerDto.Email,
            Password = registerDto.Password // Note: In production, password should be hashed
        };

        _users.Add(user);
        return user;
    }
} 