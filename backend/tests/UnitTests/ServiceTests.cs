using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Application.Services;
using Application.DTOs;
using Domain.Interfaces;
using Domain.Entities;

namespace UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockUserRepository = new Mock<IUserRepository>();

            _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
            _mockConfiguration.Setup(x => x["Jwt:Secret"]).Returns("test-secret-key-12345678901234567890");

            _authService = new AuthService(_mockUnitOfWork.Object, _mockMapper.Object, _mockConfiguration.Object);
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

            var userDto = new UserDto { Id = 1, Email = "test@test.com" };

            _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(x => x.GetUserWithRolesAsync(user.Id))
                .ReturnsAsync(user);
            _mockMapper.Setup(x => x.Map<UserDto>(user))
                .Returns(userDto);

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