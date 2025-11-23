using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Kartverket.Web.Controllers;
using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Models.Entities;
using Kartverket.Web.Services;

namespace Kartverket.Web.UnitTests.Controllers
{
    public class AuthControllerUnitTests
    {
        // Opprett in-memory database
        private KartverketDbContext GetDb()
        {
            var options = new DbContextOptionsBuilder<KartverketDbContext>()
                .UseInMemoryDatabase(databaseName: $"AuthTests_{System.Guid.NewGuid()}")
                .Options;

            return new KartverketDbContext(options);
        }

        // Opprett AuthController + HttpContext m/session
        private AuthController GetAuthController(KartverketDbContext db)
        {
            var logger = new Mock<ILogger<AuthController>>();

            var controller = new AuthController(db, logger.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Mock TempData
            controller.TempData = new TempDataDictionary(
                controller.ControllerContext.HttpContext,
                Mock.Of<ITempDataProvider>());

            // Mock session
            controller.HttpContext.Session = new DummySession();

            return controller;
        }

        // enkel session-klasse som kan slettes riktig
        private class DummySession : ISession
        {
            private Dictionary<string, byte[]> _data = new();

            public IEnumerable<string> Keys => _data.Keys;

            public string Id => "dummy";

            public bool IsAvailable => true;

            public void Clear() => _data.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public void Remove(string key) => _data.Remove(key);

            public void Set(string key, byte[] value) => _data[key] = value;

            public bool TryGetValue(string key, out byte[] value)
                => _data.TryGetValue(key, out value);
        }

        // tester Login GET
        [Fact]
        public void Login_Get_ReturnsView()
        {
            var ctx = GetDb();
            var controller = GetAuthController(ctx);

            var result = controller.Login();

            Assert.IsType<ViewResult>(result);
        }

        // tester Logout sletter session + redirect
        [Fact]
        public void Logout_ClearsSession_AndRedirects()
        {
            var ctx = GetDb();
            var controller = GetAuthController(ctx);

            // sett session data
            controller.HttpContext.Session.SetString("Username", "pilot");

            var result = controller.Logout();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);

            // sjekk om session ble tømt
            Assert.Null(controller.HttpContext.Session.GetString("Username"));
        }
    }
}
