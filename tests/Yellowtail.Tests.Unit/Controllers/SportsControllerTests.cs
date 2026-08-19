using Microsoft.AspNetCore.Mvc;
using Moq;
using Yellowtail.API.Contracts;
using Yellowtail.API.Controllers;
using Yellowtail.Data.Entities;
using Yellowtail.Services.Contracts;

namespace Yellowtail.Tests.Unit.Controllers;

public class SportsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOkWithMappedSportResponses()
    {
        var service = new Mock<ISportService>();
        var sports = new List<Sport>
        {
            new() { Id = Guid.NewGuid(), Name = "Tennis" },
            new() { Id = Guid.NewGuid(), Name = "Football" }
        };
        service.Setup(s => s.GetAllAsync()).ReturnsAsync(sports);

        var sut = new SportsController(service.Object);
        var response = await sut.GetAll();

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var body = Assert.IsType<List<SportResponse>>(ok.Value);
        Assert.Equal(2, body.Count);
        Assert.Contains(body, s => s.Name == "Tennis");
    }
}
