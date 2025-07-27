using SDCafe.DataAccess;
using SDCafe.Entities;
using BCrypt.Net;

namespace SDCafe.Business
{
    public class UserService : Service<User>, IUserService
    {
        public UserService(IRepository<User> repository) : base(repository)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var users = await _repository.FindAsync(u => u.Email == email && !u.IsDeleted);
            return users.FirstOrDefault();
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user != null && VerifyPassword(password, user.Password))
            {
                return user;
            }
            return null;
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return !await _repository.ExistsAsync(u => u.Email == email && u.Id != excludeId.Value && !u.IsDeleted);
            }
            else
            {
                return !await _repository.ExistsAsync(u => u.Email == email && !u.IsDeleted);
            }
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            return await _repository.FindAsync(u => u.Role == role && !u.IsDeleted);
        }

        public override async Task<User> AddAsync(User entity)
        {
            if (!string.IsNullOrEmpty(entity.Password))
            {
                entity.Password = HashPassword(entity.Password);
            }
            return await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(User entity)
        {
            try
            {
                var existingUser = await _repository.GetByIdAsync(entity.Id);
                if (existingUser != null)
                {
                    _repository.DetachEntity(existingUser);
                    
                    if (!string.IsNullOrEmpty(entity.Password) && entity.Password != existingUser.Password)
                    {
                        if (entity.Password.Length < 44)
                        {
                            entity.Password = HashPassword(entity.Password);
                        }
                    }
                    else
                    {
                        entity.Password = existingUser.Password;
                    }
                }
                
                _repository.DetachEntity(entity);
                
                await base.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Kullanıcı güncellenirken hata oluştu: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            try
            {
                var hashedPassword = HashPassword(newPassword);
                
                var userExists = await _repository.ExistsAsync(u => u.Id == userId);
                if (!userExists)
                    return false;

                var userToUpdate = new User
                {
                    Id = userId,
                    Password = hashedPassword,
                    UpdatedDate = DateTime.Now,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Email = string.Empty,
                    Role = UserRole.Waiter
                };

                await _repository.UpdateSpecificFieldsAsync(userToUpdate, 
                    u => u.Password, 
                    u => u.UpdatedDate);
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
} 