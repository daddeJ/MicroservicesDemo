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
    public async Task GetAllUsers_ReturnsOk_WhenExecutiveQueryHasMultipleRolesAndTiers()
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
}