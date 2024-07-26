using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Ppon;
using FluentAssertions;
using Moq;
using static CO.CDP.EntityVerification.Ppon.OrganisationUpdatedSubscriber.OrganisationUpdatedException;

namespace CO.CDP.EntityVerification.Tests.Ppon;
public class OrganisationUpdatedSubscriberTests
{
    [Fact]
    public async Task Action_InsertsNewIdentifiers()
    {
        var mockPponRepository = new Mock<IPponRepository>();
        var subscriber = new OrganisationUpdatedSubscriber(mockPponRepository.Object);
        var mockEvent = GivenOrganisationUpdatedEvent();
        var testPpon = PponFactories.GivenPpon();

        mockPponRepository
            .Setup(repo => repo.FindPponByPponIdAsync(mockEvent.PponId))
            .ReturnsAsync(testPpon);

        testPpon.Identifiers.Should().BeEmpty();

        await subscriber.Handle(mockEvent);

        mockPponRepository.Verify(
            s => s.Save(It.Is<EntityVerification.Persistence.Ppon>(p =>
                (p.IdentifierId == testPpon.IdentifierId))),
            Times.Once);
        testPpon.Identifiers.Count.Should().Be(2);
    }

    [Fact]
    public async Task Action_GeneratesExceptionWhenPponIdNotFound()
    {
        var mockPponRepository = new Mock<IPponRepository>();
        var subscriber = new OrganisationUpdatedSubscriber(mockPponRepository.Object);
        var mockEvent = GivenOrganisationUpdatedEvent();

        mockPponRepository
            .Setup(repo => repo.FindPponByPponIdAsync(mockEvent.PponId))
            .ReturnsAsync((EntityVerification.Persistence.Ppon)null!);

        await subscriber.Invoking(async r => await r.Handle(mockEvent))
            .Should().ThrowAsync<NotFoundPponException>();
    }

    private static OrganisationUpdated GivenOrganisationUpdatedEvent()
    {
        return new OrganisationUpdated
        {
            PponId = "072fa7fb29b142d480b64ac62850a305",
            Identifier = new EntityVerification.Events.Identifier
            {
                Id = "93433423432",
                LegalName = "Acme Ltd",
                Scheme = "GB-COH",
                Uri = null
            },
            AdditionalIdentifiers =
            [
                new EntityVerification.Events.Identifier
                {
                    Id = "GB123123123",
                    LegalName = "Acme Ltd",
                    Scheme = "VAT",
                    Uri = null
                }
            ],
            Roles = ["supplier"]
        };
    }
}
