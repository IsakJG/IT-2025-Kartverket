
using Kartverket.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kartverket.Web.UnitTests.Controllers
{
    public class HomeControllerUnitTests
    {
        [Fact]
        public void Index_HasNullViewName()
        {
            // Arrange
            var controller = GetUnitUnderTest();
            // Act
            var result = controller.Index();
            var viewResult = result as ViewResult;
            // Assert
            Assert.Null(viewResult.ViewName);
        }

        private HomeController GetUnitUnderTest()
        {
            var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<HomeController>>();
            var config = Substitute.For<IConfiguration>();
            
            return new HomeController(logger,config);
        }
    }

}
