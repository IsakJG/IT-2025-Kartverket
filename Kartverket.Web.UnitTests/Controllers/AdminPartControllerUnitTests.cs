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
using Microsoft.Extensions.Logging;

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
        public async Task Index_ReturnsViewWithModel()
        {
            var context = GetDbContext();

            context.Users.Add(new Models.Entities.User { UserId = 1, Username = "TestUser" });
            context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin" });
            context.UserRoles.Add(new UserRole { UserId = 1, RoleId = 1 });
            context.SaveChanges();

            var logger = new Mock<ILogger<AdminPartController>>().Object;
            var controller = new AdminPartController(context, logger);

            var result = await controller.Index();
            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<List<UserAdminViewModel>>(view.Model);
        }

        // Tester at Create (GET) returnerer riktig view
        [Fact]
        public async Task Create_Get_ReturnsView()
        {
            var context = GetDbContext();
            var logger = new Mock<ILogger<AdminPartController>>().Object;
            var controller = new AdminPartController(context, logger);

            var result = await controller.Create();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("CreateNewUser", view.ViewName);
        }

        // Tester at Create (POST) med gyldig input oppretter bruker og redirecter
        [Fact]
        public async Task Create_Post_Valid_Redirects()
        {
            var context = GetDbContext();
            context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin" });
            context.Organization.Add(new Organization { OrgId = 1, OrgName = "TestOrg" });
            context.SaveChanges();

            var logger = new Mock<ILogger<AdminPartController>>().Object;
            var controller = new AdminPartController(context, logger);
            MockTempData(controller);

            var model = new UserAdminViewModel
            {
                UserName = "NewUser",
                Email = "test@example.com",
                Password = "SecretPass",
                RoleId = 1,
                OrgId = 1
            };

            var result = await controller.Create(model);

            Assert.IsType<RedirectToActionResult>(result);
        }

        // Tester at Create (POST) med ugyldig input returnerer samme view
        [Fact]
        public async Task Create_Post_Invalid_ReturnsView()
        {
            var context = GetDbContext();

            // Legg til Roles i databasen slik at SelectList ikke feiler
            context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin" });
            context.Roles.Add(new Role { RoleId = 2, RoleName = "User" });
            context.Organization.Add(new Organization { OrgId = 1, OrgName = "TestOrg" });
            context.SaveChanges();

            var logger = new Mock<ILogger<AdminPartController>>().Object;
            var controller = new AdminPartController(context, logger);
            MockTempData(controller); // Legg til TempData for å unngå null-exception

            controller.ModelState.AddModelError("Email", "Required");

            // OBS: Controlleren har en bug - den kjører koden selv om ModelState er invalid
            // Derfor MÅ vi gi gyldige verdier for å unngå NullReferenceException
            var model = new UserAdminViewModel
            {
                UserName = "TestUser",
                Email = "invalid-email", // Ugyldig for å simulere validerings-feil
                Password = "ValidPassword123", // MÅ ha verdi siden controlleren ikke stopper ved invalid ModelState
                RoleId = 1, // MÅ ha verdi for å unngå .Value exception
                OrgId = 1 // MÅ ha verdi for å unngå .Value exception
            };

            var result = await controller.Create(model);

            // Siden controlleren ikke returnerer ved invalid ModelState, vil den faktisk opprette brukeren
            // og redirecte. Dette er en bug i controlleren, men vi kan ikke fikse det i testen.
            // Testen vil derfor feile med mindre controlleren fikses.

            // Hvis du BARE skal fikse testen uten å endre controlleren, 
            // kan du enten:
            // 1. Endre testen til å forvente Redirect (ikke riktig oppførsel)
            // 2. Eller kommentere ut denne testen siden den eksponerer en bug i controlleren

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("CreateNewUser", view.ViewName);
            Assert.IsType<UserAdminViewModel>(view.Model);
        }

        // Tester at Edit (GET) returnerer view når bruker finnes
        [Fact]
        public async Task Edit_Get_Valid_ReturnsView()
        {
            var context = GetDbContext();

            context.Users.Add(new Models.Entities.User
            {
                UserId = 5,
                Username = "John",
                Email = "john@test.com"
            });

            context.SaveChanges();

            var logger = new Mock<ILogger<AdminPartController>>().Object;
            var controller = new AdminPartController(context, logger);

            var result = await controller.Edit(5);
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditUser", view.ViewName);
        }

        // Tester at Edit (GET) returnerer NotFound når bruker ikke finnes
        [Fact]
        public async Task Edit_Get_Invalid_ReturnsNotFound()
        {
            var context = GetDbContext();
            var logger = new Mock<ILogger<AdminPartController>>().Object;
            var controller = new AdminPartController(context, logger);

            var result = await controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
