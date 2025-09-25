using Microsoft.AspNetCore.Identity;
using Moq;
using UserAuthApi.Data;

namespace UserAuthApi.Tests.Helpers;

public static class MockHelpers
{
    public static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null
        );
    }
}