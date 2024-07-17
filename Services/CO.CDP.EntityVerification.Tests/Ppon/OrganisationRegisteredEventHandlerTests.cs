using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Ppon;
using Moq;

namespace CO.CDP.EntityVerification.Tests.Ppon;

public class OrganisationRegisteredEventHandlerTests
{
    [Fact]
    public async Task Action_GeneratesPponIdAndPersists()
    {
        var pponRepository = new Mock<IPponRepository>();
        var pponService = new Mock<IPponService>();
        var generatedPpon = "92be415e5985421087bc8fee8c97d338";

        pponService.Setup(x => x.GeneratePponId()).Returns(generatedPpon);

        var handler = new OrganisationRegisteredEventHandler(pponService.Object, pponRepository.Object);
        var @event = new OrganisationRegistered
        {
            Name = "MyOrg",
        };

        await handler.Handle(@event);

        pponRepository.Verify(
            s => s.Save(It.Is<EntityVerification.Persistence.Ppon>(p => p.PponId == generatedPpon)), Times.Once);
    }
}