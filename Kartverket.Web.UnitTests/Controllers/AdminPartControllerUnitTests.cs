using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Kartverket.Web.Controllers;
using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Models.Entities;

namespace Kartverket.Web.UnitTests.Controllers
{
    public class AdminPartControllerUnitTests
    {
        private KartverketDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<KartverketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new KartverketDbContext(options);
        }

        private void MockTempData(Controller controller)
        {
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );
        }

        // Tester at Index() returnerer korrekt view og en liste med brukere
        [Fact]
        public void Index_ReturnsViewWithModel()
        {
            var context = GetDbContext();

            context.Users.Add(new User { UserId = 1, Username = "TestUser" });
            context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin" });
            context.UserRoles.Add(new UserRole { UserId = 1, RoleId = 1 });
            context.SaveChanges();

            var controller = new AdminPartController(context);

            var result = controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<List<UserAdminViewModel>>(view.Model);
        }

        // Tester at Create (GET) returnerer riktig view
        [Fact]
        public void Create_Get_ReturnsView()
        {
            var context = GetDbContext();
            var controller = new AdminPartController(context);

            var result = controller.Create();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("CreateNewUser", view.ViewName);
        }

        // Tester at Create (POST) med gyldig input oppretter bruker og redirecter
        [Fact]
        public void Create_Post_Valid_Redirects()
        {
            var context = GetDbContext();
            context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin" });
            context.SaveChanges();

            var controller = new AdminPartController(context);
            MockTempData(controller);

            var model = new UserAdminViewModel
            {
                UserName = "NewUser",
                Email = "test@example.com",
                Password = "SecretPass",
                RoleId = 1
            };

            var result = controller.Create(model);

            Assert.IsType<RedirectToActionResult>(result);
        }

        // Tester at Create (POST) med ugyldig input returnerer samme view
        [Fact]
        public void Create_Post_Invalid_ReturnsView()
        {
            var context = GetDbContext();
            var controller = new AdminPartController(context);

            controller.ModelState.AddModelError("Email", "Required");

            var model = new UserAdminViewModel();

            var result = controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("CreateNewUser", view.ViewName);
        }

        // Tester at Edit (GET) returnerer view når bruker finnes
        [Fact]
        public void Edit_Get_Valid_ReturnsView()
        {
            var context = GetDbContext();

            context.Users.Add(new User
            {
                UserId = 5,
                Username = "John",
                Email = "john@test.com"
            });

            context.SaveChanges();

            var controller = new AdminPartController(context);

            var result = controller.Edit(5);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditUser", view.ViewName);
        }

        // Tester at Edit (GET) returnerer NotFound når bruker ikke finnes
        [Fact]
        public void Edit_Get_Invalid_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = new AdminPartController(context);

            var result = controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
