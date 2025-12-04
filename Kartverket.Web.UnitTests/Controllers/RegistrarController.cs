using Xunit;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Controllers;
using Kartverket.Web.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;

namespace Kartverket.Web.UnitTests.Controllers
{
    public class RegistrarControllerUnitTests
    {
       // tester at RegisterMetode returnerer riktig view
        [Fact]
        public void RegisterMetode_ReturnsCorrectView()
        {
            // Arrange
            var dbContextMock = new Mock<KartverketDbContext>(new DbContextOptions<KartverketDbContext>());
var loggerMock = new Mock<ILogger<RegistrarController>>();
var controller = new RegistrarController(dbContextMock.Object, loggerMock.Object);

            // Act
            var result = controller.RegisterMetode();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("MainPageReg", viewResult.ViewName);
        }
    }
}
