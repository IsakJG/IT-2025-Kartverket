using Kartverket.Web.Controllers;
using Kartverket.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Kartverket.Web.UnitTests.Controllers;

public class ObstacleControllersTests
{
    private KartverketDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<KartverketDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new KartverketDbContext(options);
    }

    private ObstacleController GetControllerWithMockedSession(KartverketDbContext context)
    {
        var controller = new ObstacleController(context);

        // Mock HttpContext og Session
        var httpContext = new DefaultHttpContext();
        var mockSession = new MockHttpSession();
        mockSession.SetInt32("UserId", 1); // Sett en test UserId
        httpContext.Session = mockSession;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Mock TempData
        controller.TempData = new TempDataDictionary(
            httpContext,
            Mock.Of<ITempDataProvider>()
        );

        return controller;
    }

    // Mock Session-klasse
    private class MockHttpSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[] value)
            => _sessionStorage.TryGetValue(key, out value);
    }

    [Fact]
    public void DataFormViewResult() //Tester at skjema-siden faktisk vises i nettleseren
    {
        var context = GetDbContext();
        var controller = new ObstacleController(context);

        var result = controller.DataForm();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task DataFormReturnOverview_WithModel()//Tester at brukeren blir sendt til "Overview" etter innsending av skjema, at data vises korrekt og at ingen data går taåt underveis.
    {
        var context = GetDbContext();
        var controller = GetControllerWithMockedSession(context);

        var obstacledata = new ObstacleData
        {
            ObstacleName = "Stromledningstolpeting",
            ObstacleHeight = 15,
            ObstacleDescription = "Er ikke såååå farlig da",
            Latitude = 53.141592,
            Longitude = 73.141592
        };

        var result = await controller.DataForm(obstacledata);

        // Konverter retur til ViewResult
        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(result);
        Assert.Equal("Overview", view.ViewName);
        Assert.Equal(obstacledata, view.Model);
    }
}