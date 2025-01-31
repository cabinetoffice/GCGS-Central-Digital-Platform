using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Ppon;
using CO.CDP.MQ;
using Moq;
using static CO.CDP.EntityVerification.Tests.Events.EventsFactories;

namespace CO.CDP.EntityVerification.Tests.Ppon;

public class OrganisationRegisteredSubscriberTests
{
    [Fact]
    public async Task Action_GeneratesPponIdAndPersistsAndPublishes()
    {
        var pponRepository = new Mock<IPponRepository>();
        var pponService = new Mock<IPponService>();
        var publisher = new Mock<IPublisher>();
        var generatedPpon = "92be415e5985421087bc8fee8c97d338";

        pponService.Setup(x => x.GeneratePponId(OrganisationInformation.OrganisationType.Organisation)).Returns(generatedPpon);

        var handler = new OrganisationRegisteredSubscriber(pponService.Object, pponRepository.Object, publisher.Object);
        var orgId = Guid.NewGuid();
        var @event = GivenOrganisationRegisteredEvent(orgId);

        await handler.Handle(@event);

        pponRepository.Verify(
            s => s.SaveAsync(It.Is<EntityVerification.Persistence.Ppon>(p =>
                p.IdentifierId == generatedPpon), AnyOnSave()),
            Times.Once);

        publisher.Verify(
            s => s.Publish(It.Is<PponGenerated>(e =>
                e.Id == generatedPpon &&
                e.Scheme == "GB-PPON" &&
                e.LegalName == @event.Identifier.LegalName &&
                e.OrganisationId == orgId)),
            Times.Once);
    }

    private static Func<EntityVerification.Persistence.Ppon, Task> AnyOnSave() =>
        It.Is<Func<EntityVerification.Persistence.Ppon, Task>>(f =>
            f(It.IsAny<EntityVerification.Persistence.Ppon>()).ContinueWith(_ => true).Result);
}