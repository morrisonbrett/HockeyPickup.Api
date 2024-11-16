using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using HockeyPickup.Api.Controllers;
using HockeyPickup.Api.Data.Repositories;
using HockeyPickup.Api.Models.Responses;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace HockeyPickup.Api.Tests.ControllerTests;

public class UsersControllerTest
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly UsersController _controller;

    public UsersControllerTest()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockUserRepository.Object, _mockLogger.Object);
    }

    private void SetupUserRole(string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "test@example.com"),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private static List<UserDetailedResponse> CreateDetailedUsersList()
    {
        return new List<UserDetailedResponse>
        {
            new() {
                Id = "1",
                UserName = "user1",
                Email = "user1@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsPreferred = true,
                IsPreferredPlus = false,
                Rating = 4.5m,
                Active = true
            },
            new() {
                Id = "2",
                UserName = "user2",
                Email = "user2@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                IsPreferred = true,
                IsPreferredPlus = true,
                Rating = 4.8m,
                Active = true
            }
        };
    }

    private static List<UserBasicResponse> CreateBasicUsersList()
    {
        return new List<UserBasicResponse>
        {
            new() {
                Id = "1",
                UserName = "user1",
                Email = "user1@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsPreferred = true,
                IsPreferredPlus = false,
                Active = true
            },
            new() {
                Id = "2",
                UserName = "user2",
                Email = "user2@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                IsPreferred = true,
                IsPreferredPlus = true,
                Active = true
            }
        };
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsDetailedUsers()
    {
        // Arrange
        SetupUserRole("Admin");
        var detailedUsers = CreateDetailedUsersList();

        _mockUserRepository
            .Setup(x => x.GetDetailedUsersAsync())
            .ReturnsAsync(detailedUsers);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDetailedResponse>>().Subject;
        returnedUsers.Should().BeEquivalentTo(detailedUsers);
    }

    [Fact]
    public async Task GetUsers_AsRegularUser_ReturnsBasicUsers()
    {
        // Arrange
        SetupUserRole("User");
        var basicUsers = CreateBasicUsersList();

        _mockUserRepository
            .Setup(x => x.GetBasicUsersAsync())
            .ReturnsAsync(basicUsers);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserBasicResponse>>().Subject;
        returnedUsers.Should().BeEquivalentTo(basicUsers);
    }

    [Fact]
    public async Task GetUsers_RepositoryThrowsException_Returns500()
    {
        // Arrange
        SetupUserRole("User");
        _mockUserRepository
            .Setup(x => x.GetBasicUsersAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);

        var response = JsonSerializer.Deserialize<Dictionary<string, string>>(
            JsonSerializer.Serialize(statusCodeResult.Value)
        );
        response!["message"].Should().Be("An error occurred while retrieving users");

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task GetUsers_NoRoleClaim_ReturnsBasicUsers()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "test@example.com")
            // No role claim
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var basicUsers = CreateBasicUsersList();

        _mockUserRepository
            .Setup(x => x.GetBasicUsersAsync())
            .ReturnsAsync(basicUsers);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserBasicResponse>>().Subject;
        returnedUsers.Should().BeEquivalentTo(basicUsers);
    }
}
