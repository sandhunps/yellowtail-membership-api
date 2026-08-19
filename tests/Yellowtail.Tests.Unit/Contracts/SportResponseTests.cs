using Yellowtail.API.Contracts;
using Yellowtail.Data.Entities;

namespace Yellowtail.Tests.Unit.Contracts;

public class SportResponseTests
{
    [Fact]
    public void FromEntity_MapsIdAndName()
    {
        var sport = new Sport { Id = Guid.NewGuid(), Name = "Tennis" };

        var response = SportResponse.FromEntity(sport);

        Assert.Equal(sport.Id, response.Id);
        Assert.Equal("Tennis", response.Name);
    }
}
