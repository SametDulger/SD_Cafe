using FluentAssertions;
using Moq;
using SDCafe.Business;
using SDCafe.DataAccess;
using SDCafe.Entities;
using System.Linq.Expressions;
using Xunit;

namespace SDCafe.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IRepository<User>> _mockRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockRepository = new Mock<IRepository<User>>();
            _userService = new UserService(_mockRepository.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnUser()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            
            var user = new User
            {
                Id = 1,
                Email = email,
                Password = hashedPassword,
                FirstName = "Test",
                LastName = "User",
                Role = UserRole.Waiter,
                IsDeleted = false
            };

            _mockRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { user });

            // Act
            var result = await _userService.AuthenticateAsync(email, password);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidCredentials_ShouldReturnNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongpassword";
            
            var user = new User
            {
                Id = 1,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                FirstName = "Test",
                LastName = "User",
                Role = UserRole.Waiter,
                IsDeleted = false
            };

            _mockRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { user });

            // Act
            var result = await _userService.AuthenticateAsync(email, password);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                Id = 1,
                Email = email,
                FirstName = "Test",
                LastName = "User",
                Role = UserRole.Waiter,
                IsDeleted = false
            };

            _mockRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { user });

            // Act
            var result = await _userService.GetByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
        }

        [Fact]
        public async Task GetByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _mockRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _userService.GetByEmailAsync(email);

            // Assert
            result.Should().BeNull();
        }
    }
} 