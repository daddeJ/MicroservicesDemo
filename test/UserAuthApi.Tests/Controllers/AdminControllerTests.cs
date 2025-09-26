using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserAuthApi.Controllers;
using UserAuthApi.Data;
using UserAuthApi.Helpers;
using UserAuthApi.Services;
using UserAuthApi.Tests.Helpers;
using UserAuthApi.Tests.TestData;

namespace UserAuthApi.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUserQueryService> _mockUserQueryService;
    private readonly AdminController _adminController;
    
    public AdminControllerTests()
    {
        _mockUserManager = MockHelpers.MockUserManager();
        _mockUserQueryService = new Mock<IUserQueryService>();
        _adminController = new AdminController(_mockUserManager.Object, _mockUserQueryService.Object);
    }

    [Fact]
    public void AdminController_HssCorrectRouteAuthorization()
    {
        var controllerType = typeof(AdminController);

        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;
        Assert.NotNull(routeAttribute);
        Assert.Equal("api/admin", routeAttribute.Template);

        var apiControllerAttribute = controllerType.GetCustomAttributes(typeof(ApiControllerAttribute), false)
            .FirstOrDefault();
        Assert.NotNull(apiControllerAttribute);
        
        var authorizeAttribute = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .FirstOrDefault() as AuthorizeAttribute;
        Assert.NotNull(authorizeAttribute);
        Assert.Equal("SuperAdminOnly", authorizeAttribute.Policy);
    }

    [Fact]
    public void AdminController_InheritsFromControllerBase()
    {
        Assert.IsAssignableFrom<ControllerBase>(_adminController);
    }
    
    [Fact]
    public async Task GetAllUsers_ReturnsOk_WhenAdminQueryHasMultipleRolesAndTiers()
    {
        _mockUserManager.Setup(u => u.Users).Returns(FakeUsers.GetTestUsers());
        _mockUserQueryService.Setup(s => s.GetUsersAsync(
                It.IsAny<IQueryable<ApplicationUser>>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<int>>(),
                1, 10))
            .ReturnsAsync(new PageResultDto<UserDto>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 5,
                TotalPages = 1,
                Items = FakeUsers.GetMockAdminAccessUserDto()
            });

        var roles = "Executive,HR,Manager,Leader,Regular"; 
        var tiers = "1,2,3,4,5";                       
        
        var result = await _adminController.GetAllUsers(roles, tiers, 1, 10);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var pageResult = Assert.IsType<PageResultDto<UserDto>>(okResult.Value);

        Assert.Equal(5, pageResult.TotalItems);
        Assert.All(pageResult.Items, item =>
        {
            Assert.Contains(item.Role, DataSeeder.AdminRoleAccess);
            Assert.Contains(int.Parse(item.Tier), new[] { 1, 2, 3, 4, 5 });
        });
    }

    [Fact]
    public async Task GetAllUsers_ReturnsBadRequest_WhenTierIsOutOfRange()
    {
        var roles = "HR"; 
        var tiers = "0";

        var result = await _adminController.GetAllUsers(roles, tiers, 1, 10);
        
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(-1, 1)]
    public async Task GetAllUsers_WithInvalidPageNumber_CorrectsToOne(int invalidPageNumber, int expectedPageNumber)
    {
        var expectedResult = new PageResultDto<UserDto>()
        {
            PageNumber = expectedPageNumber,
            PageSize = 10,
            TotalItems = 0,
            TotalPages = 1,
            Items = []
        };
        _mockUserQueryService.Setup(x => x.GetUsersAsync(
                It.IsAny<IQueryable<IdentityUser>>(), 
                It.IsAny<List<string>>(), 
                It.IsAny<List<int>>(), 
                It.IsAny<int>(), 
                It.IsAny<int>()))
            .ReturnsAsync(expectedResult);
        
        var result = await _adminController.GetAllUsers(null, null, invalidPageNumber, 10);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockUserQueryService.Verify(x => x.GetUsersAsync(
            It.IsAny<IQueryable<IdentityUser>>(), 
            It.IsAny<List<string>>(), 
            It.IsAny<List<int>>(), 
            expectedPageNumber, 
            It.IsAny<int>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-5, 10)]
    [InlineData(-1, 10)]
    public async Task GetAllUsers_WithInvalidPageSize_CorrectsToTen(int invalidPageSize, int expectedPageSize)
    {
        var expectedResult = new PageResultDto<UserDto>()
        {
            PageNumber = 1,
            PageSize = expectedPageSize,
            TotalItems = 0,
            TotalPages = 1,
            Items = []
        };
        _mockUserQueryService.Setup(x => x.GetUsersAsync(
                It.IsAny<IQueryable<IdentityUser>>(), 
                It.IsAny<List<string>>(), 
                It.IsAny<List<int>>(), 
                It.IsAny<int>(), 
                It.IsAny<int>()))
            .ReturnsAsync(expectedResult);
        
        var result = await _adminController.GetAllUsers(null, null, 1, invalidPageSize);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockUserQueryService.Verify(x => x.GetUsersAsync(
            It.IsAny<IQueryable<IdentityUser>>(), 
            It.IsAny<List<string>>(), 
            It.IsAny<List<int>>(), 
            It.IsAny<int>(), 
            expectedPageSize), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_WithPageSizeOver100_CorrectsTo100()
    {
        var expectedResult = new PageResultDto<UserDto>()
        {
            PageNumber = 1,
            PageSize = 100,
            TotalItems = 0,
            TotalPages = 1,
            Items = []
        };
        _mockUserQueryService.Setup(x => x.GetUsersAsync(
                It.IsAny<IQueryable<IdentityUser>>(), 
                It.IsAny<List<string>>(), 
                It.IsAny<List<int>>(), 
                It.IsAny<int>(), 
                It.IsAny<int>()))
            .ReturnsAsync(expectedResult);
        
        var result = await _adminController.GetAllUsers(null, null, 1, 200);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockUserQueryService.Verify(x => x.GetUsersAsync(
            It.IsAny<IQueryable<IdentityUser>>(), 
            It.IsAny<List<string>>(), 
            It.IsAny<List<int>>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetUserById_WithNonExixtent_ReturnsNotFound()
    {
        var userId = "non-existent-id";
        _mockUserManager.Setup(u => u.Users).Returns(FakeUsers.GetTestUsers);
        
        var result = await _adminController.GetUserById(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetUserById_WithValidId_ReturnsOk()
    {
        var userId = "1";
        var user = FakeUsers.GetTestUsers().FirstOrDefault();
        var roles = new List<string> {"Admin"};
        var claims = new List<Claim> { new Claim("Tier", "0") };
        
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);
        _mockUserManager.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(claims);
        
        var result = await _adminController.GetUserById(userId);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var userDto = Assert.IsType<UserDto>(okResult.Value);
        
        Assert.Equal(userId, userDto.Id);
        Assert.Equal("adminUser", userDto.UserName);
        Assert.Equal("test@admin.com", userDto.Email);
        Assert.Equal("Admin", userDto.Role);
        Assert.Equal("0", userDto.Tier);
    }

    [Fact]
    public async Task GetUserById_WithEmptyRoleAndClaims_ReturnUserWithEmptyString()
    {
        var userId = "1";
        var user = FakeUsers.GetTestUsers().FirstOrDefault();
        var roles = new List<string>();
        var claims = new List<Claim>();
        
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);
        _mockUserManager.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(claims);
        
        var result = await _adminController.GetUserById(userId);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var userDto = Assert.IsType<UserDto>(okResult.Value);
        
        Assert.Equal(string.Empty, userDto.Role);
        Assert.Equal(string.Empty, userDto.Tier);
    }

    [Fact]
    public async Task GetUserById_WithMixedClaimTypes_OnlyIncudesTierValues()
    {
        var userId = "1";
        var user = FakeUsers.GetTestUsers().FirstOrDefault();
        var roles = new List<string>
        {
            "Admin"
        };
        var claims = new List<Claim>
        {
            new Claim("Tier", "0"),
            new Claim("Tier", "1"),
            new Claim("Tier", "5")
        };
        
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);
        _mockUserManager.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(claims);
        
        var result = await _adminController.GetUserById(userId);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var userDto = Assert.IsType<UserDto>(okResult.Value);
        
        Assert.Equal("Admin", userDto.Role);
        Assert.Equal("0, 1, 5", userDto.Tier);
    }

    [Fact]
    public async Task UpdateUserRoleTier_WithValidData_ReturnsOkWithSuccessMessage()
    {
        var userId = "test-user-id";
        var updateModel = new UpdateUserDto { Role = "Admin", Tier = 0};
        
        _mockUserQueryService.Setup(x => x.UpdateUserAsync(userId, updateModel))
            .ReturnsAsync((true, string.Empty));

        var result = await _adminController.UpdateUserRoleTier(userId, updateModel);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        var messageProperty = response.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal("User role and tier updated successfully", messageProperty.GetValue(response));
    }

    [Fact]
    public async Task UpdateUserRoleTier_WithInvalidData_ReturnBadRequestWithError()
    {
        var userId = "test-user-id";
        var updateModel = new UpdateUserDto { Role = "InvalidRole", Tier = 999 };
        var expectedErrors = $"Tier '{updateModel.Tier}' is not valid for role '{updateModel.Role}'.";
        
        _mockUserQueryService.Setup(x => x.UpdateUserAsync(userId, updateModel))
            .ReturnsAsync((false, expectedErrors));
        
        var result = await _adminController.UpdateUserRoleTier(userId, updateModel);
        
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequestResult.Value;
        Assert.NotNull(response);

        var messageProperty = response.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal(expectedErrors, messageProperty.GetValue(response));
    }

    [Fact]
    public async Task UpdateUserRoleTier_WithNonExistentUser_ReturnsBadRequestWithError()
    {
        var userId = "non-existent-id";
        var updateModel = new UpdateUserDto { Role = "Admin", Tier = 0 };
        var expectedError = "User not found";
        
        _mockUserQueryService.Setup(x => x.UpdateUserAsync(userId, updateModel))
            .ReturnsAsync((false, expectedError));
        
        var result = await _adminController.UpdateUserRoleTier(userId, updateModel);
        
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequestResult.Value;
        Assert.NotNull(response);

        var messageProperty = response.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal(expectedError, messageProperty.GetValue(response));
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ReturnOkWithSuccessMessage()
    {
        var userId = "test-user-id";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "TestUser",
            LockoutEnabled = false,
            LockoutEnd = null
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _adminController.DeleteUser(userId);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);

        var messageProperty = response.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal("User has been deleted successfully", messageProperty.GetValue(response));
        
        Assert.True(user.LockoutEnabled);
        Assert.Equal(DateTimeOffset.MaxValue, user.LockoutEnd);
        
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteUser_WithNonExistentId_ReturnsNotFound()
    {
        var userId = "non-existent-id";
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

        var result = await _adminController.DeleteUser(userId);

        Assert.IsType<NotFoundResult>(result);
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
    
    [Fact] public async Task DeleteUser_WithNullId_ReturnsNotFound()
    {
        _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

        var result = await _adminController.DeleteUser(null);

        Assert.IsType<NotFoundResult>(result);
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact] public async Task DeleteUser_AlreadyDeletedUser_StillReturnsOkAndUpdatesLockout()
    {
        var userId = "test-user-id";
        var user = new ApplicationUser 
        { 
            Id = userId, UserName = "testuser", LockoutEnabled = true, LockoutEnd = DateTimeOffset.MaxValue
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _adminController.DeleteUser(userId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
            
        var messageProperty = response.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal("User has been deleted successfully", messageProperty.GetValue(response));

        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact] public async Task DeleteUser_UpdateFails_StillReturnsOk()
    {
        var userId = "test-user-id";
        var user = new ApplicationUser 
        { 
            Id = userId, UserName = "testuser", LockoutEnabled = false, LockoutEnd = null
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        var result = await _adminController.DeleteUser(userId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}