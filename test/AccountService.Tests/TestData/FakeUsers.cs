using AccountService.Data;
using AccountService.Models;

namespace AccountService.Tests.TestData;

public static class FakeUsers
{
    public static IQueryable<ApplicationUser> GetTestUsers()
    {
        return new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = "1",
                UserName = "adminUser",
                Email = "test@admin.com"
            },
            new ApplicationUser
            {
                Id = "2",
                UserName = "executiveUser",
                Email = "test@executive.com"
            },
            new ApplicationUser
            {
                Id = "3",
                UserName = "hrUser",
                Email = "test@hr.com"
            },
            new ApplicationUser
            {
                Id = "4",
                UserName = "managerUser",
                Email = "test@manager.com"
            },
            new ApplicationUser
            {
                Id = "5",
                UserName = "leaderUser",
                Email = "test@leader.com"
            },
            new ApplicationUser
            {
                Id = "6",
                UserName = "regularUser",
                Email = "test@regular.com"
            }
        }.AsQueryable();
    }

    public static List<UserDto> GetMockUserDto()
    {
        return new List<UserDto>
        {
            new UserDto
            {
                Id = "1",
                UserName = "adminUser",
                Email = "test@admin.com",
                Role = "Admin",
                Tier = "0"
            },
            new UserDto
            {
                Id = "2",
                UserName = "executiveUser",
                Email = "test@executive.com",
                Role = "Admin",
                Tier = "1"
            },
            new UserDto
            {
                Id = "3",
                UserName = "hrUser",
                Email = "test@hr.com",
                Role = "Admin",
                Tier = "2"
            },
            new UserDto
            {
                Id = "4",
                UserName = "managerUser",
                Email = "test@manager.com",
                Role = "Admin",
                Tier = "3"
            },
            new UserDto
            {
                Id = "5",
                UserName = "leaderUser",
                Email = "test@leader.com",
                Role = "Admin",
                Tier = "4"
            },
            new UserDto
            {
                Id = "6",
                UserName = "regularUser",
                Email = "test@regular.com",
                Role = "Admin",
                Tier = "5"
            },
        };
    }
    public static List<UserDto> GetMockAdminAccessUserDto()
    {
        return new List<UserDto>
        {
            new UserDto
            {
                Id = "2",
                UserName = "executiveUser",
                Email = "test@executive.com",
                Role = "Admin",
                Tier = "1"
            },
            new UserDto
            {
                Id = "3",
                UserName = "hrUser",
                Email = "test@hr.com",
                Role = "Admin",
                Tier = "2"
            },
            new UserDto
            {
                Id = "4",
                UserName = "managerUser",
                Email = "test@manager.com",
                Role = "Admin",
                Tier = "3"
            },
            new UserDto
            {
                Id = "5",
                UserName = "leaderUser",
                Email = "test@leader.com",
                Role = "Admin",
                Tier = "4"
            },
            new UserDto
            {
                Id = "6",
                UserName = "regularUser",
                Email = "test@regular.com",
                Role = "Admin",
                Tier = "5"
            },
        };
    }
}