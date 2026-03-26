using Moq;
using ShiftOne.Application.Dtos;
using ShiftOne.Application.Interfaces.Common;
using ShiftOne.Application.Services.Common;
using ShiftOne.Domain.Entities;
using ShiftOne.Domain.Interfaces.Common;

namespace ShiftOne.Tests {
    public class AuthServiceTests {

        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<ITokenService> _mockTokenService;

        private readonly AuthService _authService;
        private readonly User _testUser;

        public AuthServiceTests() {
            _mockRepo = new Mock<IUserRepository>();
            _mockTokenService = new Mock<ITokenService>();

            _authService = new AuthService(_mockRepo.Object, _mockTokenService.Object);
            
            _testUser = new User {
                Id = 1,
                Name = "John",
                Email = "john@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Phone = "1234567890",
                Role = ShiftOne.Domain.Constants.UserRoles.Admin,
                RefreshToken = "old-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };
        }

   
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsTokenResponse() {
            // Arrange
            var dto = new LoginDto { Email = "john@test.com", Password = "password123" };

            _mockRepo.Setup(x => x.GetByEmailAsync(dto.Email))
                     .ReturnsAsync(_testUser);
            _mockTokenService.Setup(x => x.GenerateAccessToken(_testUser))
                             .Returns("access-token");
            _mockTokenService.Setup(x => x.GenerateRefreshToken())
                             .Returns("refresh-token");
            _mockRepo.Setup(x => x.UpdateRefreshTokenAsync(_testUser.Id, "refresh-token", It.IsAny<DateTime>()))
                     .ReturnsAsync(true);

            // Act
            var result = await _authService.LoginAsync(dto);


            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task LoginAsync_NotFoundEmail_ReturnsNull() {
            // Arrange
            var dto = new LoginDto { Email = "johndoe@test.com", Password = "password123" };

            _mockRepo.Setup(x => x.GetByEmailAsync(dto.Email))
                     .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync(dto);


            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_PasswordMismatch_ReturnsNull() {
            // Arrange
            var dto = new LoginDto { Email = "john@test.com", Password = "password1234" };

            _mockRepo.Setup(x => x.GetByEmailAsync(dto.Email))
                     .ReturnsAsync(_testUser);

            // Act
            var result = await _authService.LoginAsync(dto);


            // Assert
            Assert.Null(result);
        }


        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsNullForDBConnectionError() {
            // Arrange
            var dto = new LoginDto { Email = "john@test.com", Password = "password123" };

            _mockRepo.Setup(x => x.GetByEmailAsync(dto.Email))
                     .ReturnsAsync(_testUser);
            _mockTokenService.Setup(x => x.GenerateAccessToken(_testUser))
                             .Returns("access-token");
            _mockTokenService.Setup(x => x.GenerateRefreshToken())
                             .Returns("refresh-token");
            _mockRepo.Setup(x => x.UpdateRefreshTokenAsync(_testUser.Id, "refresh-token", It.IsAny<DateTime>()))
                     .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(dto);


            // Assert
            Assert.Null(result);
        }


    }
}
