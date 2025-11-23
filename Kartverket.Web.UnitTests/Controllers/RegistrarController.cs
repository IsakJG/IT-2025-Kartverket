using Xunit;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Controllers;

namespace Kartverket.Web.UnitTests.Controllers
{
    public class RegistrarControllerUnitTests
    {
       // tester at RegisterMetode returnerer riktig view
        [Fact]
        public void RegisterMetode_ReturnsCorrectView()
        {
            // Arrange
            var controller = new RegistrarController();

            // Act
            var result = controller.RegisterMetode();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("MainPageReg", viewResult.ViewName);
        }
    }
}
