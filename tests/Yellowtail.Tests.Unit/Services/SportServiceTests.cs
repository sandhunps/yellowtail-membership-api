using Moq;
using Yellowtail.Data.Entities;
using Yellowtail.Data.Repositories;
using Yellowtail.Services.Implementation;

namespace Yellowtail.Tests.Unit.Services;

public class SportServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsRepositoryResult()
    {
        var repository = new Mock<ISportRepository>();
        var sports = new List<Sport> { new() { Id = Guid.NewGuid(), Name = "Tennis" } };
        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(sports);

        var sut = new SportService(repository.Object);
        var result = await sut.GetAllAsync();

        Assert.Same(sports, result);
    }
}
