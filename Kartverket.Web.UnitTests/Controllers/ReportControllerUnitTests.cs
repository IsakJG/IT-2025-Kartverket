using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Controllers;
using Kartverket.Web.Data;
using Kartverket.Web.Models.Entities;
using Kartverket.Web.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace Kartverket.Web.UnitTests.Controllers
{
    public class ReportControllerUnitTests
    {
        private KartverketDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<KartverketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new KartverketDbContext(options);
        }

        // tester at ActiveReports returnerer en ViewResult
        [Fact]
        public async Task ActiveReports_ReturnsViewResult()
        {
            var context = GetDbContext();
            var controller = new ReportController(context);

            var result = await controller.ActiveReports();

            Assert.IsType<ViewResult>(result);
        }

        // tester at Archive returnerer en ViewResult
        [Fact]
        public async Task Archive_ReturnsViewResult()
        {
            var context = GetDbContext();
            var controller = new ReportController(context);

            var result = await controller.Archive();

            Assert.IsType<ViewResult>(result);
        }

        // tester at Details med gyldig id returnerer View med modell
        [Fact]
        public async Task Details_ValidId_ReturnsViewWithModel()
        {
            var context = GetDbContext();

            var report = new Report
            {
                ReportId = 1,
                Title = "Test",
                HeightInFeet = 100,
                User = new User { Username = "Pilot" },
                Status = new Status { StatusName = "Pending" },
                Category = new Category { CategoryName = "Tower" },
                TimestampEntry = new TimestampEntry { DateCreated = DateTime.UtcNow }
            };

            context.Reports.Add(report);
            await context.SaveChangesAsync();

            var controller = new ReportController(context);

            var result = await controller.Details(1);

            Assert.IsType<ViewResult>(result);

            var view = result as ViewResult;
            Assert.IsType<ArchiveRow>(view.Model);
        }

        // tester at Details med ugyldig id returnerer NotFound
        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = new ReportController(context);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        // tester at ValidateReport gir view når id finnes
        [Fact]
        public async Task ValidateReport_ValidId_ReturnsView()
        {
            var context = GetDbContext();

            context.Reports.Add(new Report
            {
                ReportId = 10,
                Title = "ReportX"
            });

            await context.SaveChangesAsync();

            var controller = new ReportController(context);

            var result = await controller.ValidateReport(10);

            Assert.IsType<ViewResult>(result);
        }

        // tester at ValidateReport returnerer NotFound på ugyldig id
        [Fact]
        public async Task ValidateReport_InvalidId_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = new ReportController(context);

            var result = await controller.ValidateReport(12345);

            Assert.IsType<NotFoundResult>(result);
        }

        // tester at ApproveReport endrer status og redirecter
        [Fact]
        public async Task ApproveReport_ValidId_ChangesStatusAndRedirects()
        {
            var context = GetDbContext();

            context.Reports.Add(new Report
            {
                ReportId = 50,
                StatusId = 1
            });

            await context.SaveChangesAsync();

            var controller = new ReportController(context);

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            var result = await controller.ApproveReport(50);

            var updated = await context.Reports.FindAsync(50);

            Assert.Equal(3, updated.StatusId);
            Assert.IsType<RedirectToActionResult>(result);
        }

        // tester at RejectReport endrer status og redirecter
        [Fact]
        public async Task RejectReport_ValidId_ChangesStatusAndRedirects()
        {
            var context = GetDbContext();

            context.Reports.Add(new Report
            {
                ReportId = 70,
                StatusId = 1
            });

            await context.SaveChangesAsync();

            var controller = new ReportController(context);

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            var result = await controller.RejectReport(70);

            var updated = await context.Reports.FindAsync(70);

            Assert.Equal(2, updated.StatusId);
            Assert.IsType<RedirectToActionResult>(result);
        }
    }
}
