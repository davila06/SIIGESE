using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Application.Services;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.Entities;

namespace UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IPasswordResetTokenRepository> _mockTokenRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly Mock<ITokenBlacklistService> _mockTokenBlacklist;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockTokenRepository = new Mock<IPasswordResetTokenRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _mockTokenBlacklist = new Mock<ITokenBlacklistService>();

            _mockConfiguration.Setup(x => x["Jwt:Secret"]).Returns("test-secret-key-for-unit-testing-min32chars!!");
            _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");
            _mockConfiguration.Setup(x => x["Jwt:ExpirationHours"]).Returns("8");

            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockRoleRepository.Object,
                _mockTokenRepository.Object,
                _mockEmailService.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockTokenBlacklist.Object);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ShouldReturnLoginResponse()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "test@test.com", Password = "password123" };
            var user = new User 
            { 
                Id = 1, 
                Email = "test@test.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(x => x.GetUserWithRolesAsync(user.Id))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be(request.Email);
        }
    }
}