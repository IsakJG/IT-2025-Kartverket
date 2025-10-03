using Kartverket.Web.Controllers;
using Kartverket.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Kartverket.Web.UnitTests.Controllers;

public class ObstacleControllersTests
{
    private readonly ObstacleController _controller;

    public ObstacleControllersTests()
    {
        _controller = new ObstacleController();
    }

    [Fact]
    public void DataFormViewResult() //Tester at skjema-siden faktisk vises i nettleseren
    {
        var result = _controller.DataForm();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void DataFormReturnOverview_WithModel()//Tester at brukeren blir snedt til "Overview" etter innsending av skjema, at data vises korrekt og at ingen data går taåt underveis.
    {
        var obstacledata = new ObstacleData
        {
            ObstacleName = "Stromledningstolpeting",

            ObstacleHeight = 15,

            ObstacleDescription = "Er ikke såååå farlig da",

            Latitude = 53.141592,

            Longitude = 73.141592
        };

        var result = _controller.DataForm(obstacledata) as ViewResult;

        Assert.NotNull(result);
        Assert.Equal("Overview", result.ViewName);
        Assert.Equal(obstacledata, result.Model);
    }
}