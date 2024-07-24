using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Ppon;
using CO.CDP.MQ;
using Moq;

namespace CO.CDP.EntityVerification.Tests.Ppon;

public class OrganisationRegisteredEventHandlerTests
{
    [Fact]
    public async Task Action_GeneratesPponIdAndPersistsAndPublishes()
    {
        var pponRepository = new Mock<IPponRepository>();
        var pponService = new Mock<IPponService>();
        var publisher = new Mock<IPublisher>();
        var generatedPpon = "92be415e5985421087bc8fee8c97d338";

        pponService.Setup(x => x.GeneratePponId()).Returns(generatedPpon);

        var handler = new OrganisationRegisteredEventHandler(pponService.Object, pponRepository.Object, publisher.Object);
        var @event = GivenOrganisationRegisteredEvent();

        await handler.Handle(@event);

        pponRepository.Verify(
            s => s.Save(It.Is<EntityVerification.Persistence.Ppon>(p => p.IdentifierId == generatedPpon)), Times.Once);
        publisher.Verify(
               s => s.Publish(It.IsAny<String>()), Times.Once);
    }

    private static OrganisationRegistered GivenOrganisationRegisteredEvent(
        Guid? id = null,
        string name = "Acme Ltd"
    )
    {
        return new OrganisationRegistered
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Identifier = new EntityVerification.Events.Identifier
            {
                Id = "93433423432",
                LegalName = name,
                Scheme = "GB-COH",
                Uri = null
            },
            AdditionalIdentifiers =
            [
                new EntityVerification.Events.Identifier
                {
                    Id = "GB123123123",
                    LegalName = name,
                    Scheme = "VAT",
                    Uri = null
                }
            ],
            Roles = ["supplier"]
        };
    }
}
