using Kartverket.Web.Controllers;
using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Diagnostics;

namespace Kartverket.Web.UnitTests.Controllers
    {
        public class HomeControllerUnitTests
        {
            private HomeController GetController()
            {
                var logger = Substitute.For<ILogger<HomeController>>();
                var options = new DbContextOptionsBuilder<KartverketDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
                var dbContext = new KartverketDbContext(options);

            var controller = new HomeController(dbContext, logger);

                // Mock HttpContext for Error action
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                return controller;
            }

            // Tester at Index returnerer riktig view
            [Fact]
            public void Index_ReturnsView()
            {
                // Arrange
                var controller = GetController();

                // Act
                var result = controller.Index();

                // Assert
                Assert.IsType<ViewResult>(result);
            }

            // Tester at MainPage returnerer riktig view
            [Fact]
            public void MainPage_ReturnsView()
            {
                // Arrange
                var controller = GetController();

                // Act
                var result = controller.MainPage();

                // Assert
                Assert.IsType<ViewResult>(result);
            }

            // Tester at Privacy returnerer riktig view
            [Fact]
            public void Privacy_ReturnsView()
            {
                // Arrange
                var controller = GetController();

                // Act
                var result = controller.Privacy();

                // Assert
                Assert.IsType<ViewResult>(result);
            }

            // Tester at Error returnerer view med ErrorViewModel
            [Fact]
            public void Error_ReturnsViewWithErrorViewModel()
            {
                // Arrange
                var controller = GetController();

                // Act
                var result = controller.Error();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
                Assert.NotNull(model.RequestId);
            }

            // Tester at Error setter RequestId fra Activity.Current når tilgjengelig
            [Fact]
            public void Error_UsesActivityCurrentId_WhenAvailable()
            {
                // Arrange
                var controller = GetController();
                var activity = new Activity("TestActivity");
                activity.Start();

                // Act
                var result = controller.Error();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
                Assert.Equal(activity.Id, model.RequestId);

                activity.Stop();
            }

            // Tester at Error bruker TraceIdentifier når Activity.Current er null
            [Fact]
            public void Error_UsesTraceIdentifier_WhenActivityIsNull()
            {
                // Arrange
                var controller = GetController();
                var expectedTraceId = "test-trace-id";
                controller.HttpContext.TraceIdentifier = expectedTraceId;

                // Act
                var result = controller.Error();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
                Assert.Equal(expectedTraceId, model.RequestId);
            }

            // Tester at Error har korrekte ResponseCache attributter
            [Fact]
            public void Error_HasCorrectResponseCacheAttributes()
            {
                // Arrange
                var method = typeof(HomeController).GetMethod("Error");

                // Act
                var attribute = method.GetCustomAttributes(typeof(ResponseCacheAttribute), false)
                    .FirstOrDefault() as ResponseCacheAttribute;

                // Assert
                Assert.NotNull(attribute);
                Assert.Equal(0, attribute.Duration);
                Assert.Equal(ResponseCacheLocation.None, attribute.Location);
                Assert.True(attribute.NoStore);
            }
        }
    }
