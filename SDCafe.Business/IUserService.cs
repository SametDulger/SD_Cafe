using SDCafe.Entities;

namespace SDCafe.Business
{
    public interface IUserService : IService<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);
    }
} 