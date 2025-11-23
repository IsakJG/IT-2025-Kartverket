using Kartverket.Web.Controllers;
using Kartverket.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;


namespace Kartverket.Web.UnitTests.Controllers;

public class ObstacleControllersTests
{
    private readonly ObstacleController _controller;

    public ObstacleControllersTests()
    {
        var options = new DbContextOptionsBuilder<KartverketDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        var context = new KartverketDbContext(options);

        _controller = new ObstacleController(context);
    }
    [Fact]
    public void DataFormViewResult() //Tester at skjema-siden faktisk vises i nettleseren
    {
        var result = _controller.DataForm();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task DataFormReturnOverview_WithModel()//Tester at brukeren blir sendt til "Overview" etter innsending av skjema, at data vises korrekt og at ingen data går taåt underveis.
    {
        var obstacledata = new ObstacleData
        {
            ObstacleName = "Stromledningstolpeting",

            ObstacleHeight = 15,

            ObstacleDescription = "Er ikke såååå farlig da",

            Latitude = 53.141592,

            Longitude = 73.141592
        };

        var result = await _controller.DataForm(obstacledata);

        // Konverter retur til ViewResult
         var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(result);
        Assert.Equal("Overview", view.ViewName);
        Assert.Equal(obstacledata, view.Model);
    }
}