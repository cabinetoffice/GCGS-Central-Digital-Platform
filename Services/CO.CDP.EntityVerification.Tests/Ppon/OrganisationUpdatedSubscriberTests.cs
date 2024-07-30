using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Ppon;
using FluentAssertions;
using Moq;

using static CO.CDP.EntityVerification.Tests.Events.EventsFactories;
using static CO.CDP.EntityVerification.Tests.Events.PponFactories;

namespace CO.CDP.EntityVerification.Tests.Events;
public class OrganisationUpdatedSubscriberTests
{
    [Fact]
    public async Task Handle_InsertsNewIdentifiersThatDoNotExist_WhenPponExists()
    {
        var mockPponRepository = new Mock<IPponRepository>();
        var subscriber = new OrganisationUpdatedSubscriber(mockPponRepository.Object);
        var mockEvent = GivenOrganisationUpdatedEvent();
        var testPpon = GivenPpon();
        var pponIdentifier = CreatePponIdentifier();

        mockEvent.AdditionalIdentifiers.Add(pponIdentifier);

        mockPponRepository
            .Setup(repo => repo.FindPponByPponIdAsync(pponIdentifier!.Id))
            .ReturnsAsync(testPpon);

        testPpon.Identifiers.Should().BeEmpty();

        await subscriber.Handle(mockEvent);

        mockPponRepository.Verify(
            s => s.Save(It.Is<EntityVerification.Persistence.Ppon>(p =>
                (p.IdentifierId == testPpon.IdentifierId))),
            Times.Once);
        testPpon.Identifiers.Count.Should().Be(mockEvent.AllIdentifiers().Count());
    }

    [Fact]
    public async Task Handle_DoesNotInsertExistingIdentifiers_WhenPponExists()
    {
        var mockPponRepository = new Mock<IPponRepository>();
        var subscriber = new OrganisationUpdatedSubscriber(mockPponRepository.Object);
        var mockEvent = GivenOrganisationUpdatedEvent();
        var testPpon = GivenPpon();
        var pponIdentifier = CreatePponIdentifier();

        mockEvent.AdditionalIdentifiers.Add(pponIdentifier);

        mockPponRepository
            .Setup(repo => repo.FindPponByPponIdAsync(pponIdentifier!.Id))
            .ReturnsAsync(testPpon);
        testPpon.Identifiers = Identifier.GetPersistenceIdentifiers(mockEvent.AllIdentifiers());

        await subscriber.Handle(mockEvent);

        testPpon.Identifiers.Count.Should().Be(mockEvent.AllIdentifiers().Count());
    }

    [Fact]
    public async Task Handle_DoesNotInsertNewIdentifiers_WhenInboundEventDoesNotContainPponIdentifier()
    {
        var mockPponRepository = new Mock<IPponRepository>();
        var subscriber = new OrganisationUpdatedSubscriber(mockPponRepository.Object);
        var mockEvent = GivenOrganisationUpdatedEvent();
        var testPpon = GivenPpon();

        testPpon.Identifiers.Should().BeEmpty();

        await subscriber.Handle(mockEvent);

        testPpon.Identifiers.Should().BeEmpty();
        mockPponRepository.Verify(s => s.Save(It.IsAny<EntityVerification.Persistence.Ppon>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DoesNotInsertNewIdentifiers_WhenInboundEventContainsPponIdentifierButPponDoesNotExistInRepo()
    {
        var mockPponRepository = new Mock<IPponRepository>();
        var subscriber = new OrganisationUpdatedSubscriber(mockPponRepository.Object);
        var mockEvent = GivenOrganisationUpdatedEvent();
        var testPpon = GivenPpon();
        var pponIdentifier = CreatePponIdentifier();

        mockEvent.AdditionalIdentifiers.Add(pponIdentifier);
        testPpon.Identifiers.Should().BeEmpty();

        await subscriber.Handle(mockEvent);

        testPpon.Identifiers.Should().BeEmpty();
        mockPponRepository.Verify(s => s.Save(It.IsAny<EntityVerification.Persistence.Ppon>()), Times.Never);
    }
}
