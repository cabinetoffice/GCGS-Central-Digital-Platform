using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Ppon;
using FluentAssertions;
using Moq;
using static CO.CDP.EntityVerification.Events.Identifier;
using static CO.CDP.EntityVerification.Persistence.Tests.PponFactories;
using static CO.CDP.EntityVerification.Tests.Events.EventsFactories;

namespace CO.CDP.EntityVerification.Tests.Ppon;
public class OrganisationUpdatedSubscriberTests
{
    [Fact]
    public async Task Handle_InsertsNewIdentifiersThatDoNotExist_WhenPponExists()
    {
        var mockPponRepository = new Mock<IPponRepository>();
        var subscriber = new OrganisationUpdatedSubscriber(mockPponRepository.Object);
        var mockEvent = GivenOrganisationUpdatedEvent();
        var testPpon = GivenPpon(mockEvent.Identifier.Id);
        var totalAdditionalIdentifiersBeforeUpdate = mockEvent.AdditionalIdentifiers.Count();

        mockPponRepository
            .Setup(repo => repo.FindPponByPponIdAsync(mockEvent.Identifier.Id))
            .ReturnsAsync(testPpon);

        await subscriber.Handle(mockEvent);

        mockPponRepository.Verify(
            s => s.Save(It.Is<EntityVerification.Persistence.Ppon>(p =>
                (p.IdentifierId == testPpon.IdentifierId))),
            Times.Once);
        testPpon.Identifiers.Count.Should().Be(totalAdditionalIdentifiersBeforeUpdate);
        testPpon.Identifiers.ToList().ForEach(i => i.endsOn.Should().BeNull());
    }

    [Fact]
    public async Task Handle_SetsEndsOn_ForPersistedIdentifiersThatDoNotExistInUpdate()
    {
        var mockPponRepository = new Mock<IPponRepository>();
        var subscriber = new OrganisationUpdatedSubscriber(mockPponRepository.Object);
        var mockEvent = GivenOrganisationUpdatedEvent();
        var testPpon = GivenPponWithIdentifier(mockEvent.Identifier.Id);
        var totalAdditionalIdentifiersInUpdate = mockEvent.AdditionalIdentifiers.Count();
        var totalPersistedIdentifiersBeforeHandle = testPpon.Identifiers.Count();

        mockPponRepository
            .Setup(repo => repo.FindPponByPponIdAsync(mockEvent.Identifier.Id))
            .ReturnsAsync(testPpon);

        await subscriber.Handle(mockEvent);

        mockPponRepository.Verify(
            s => s.Save(It.Is<EntityVerification.Persistence.Ppon>(p =>
                (p.IdentifierId == testPpon.IdentifierId))),
            Times.Once);
        testPpon.Identifiers.Count.Should().Be(totalAdditionalIdentifiersInUpdate + totalPersistedIdentifiersBeforeHandle);

        testPpon.Identifiers.FirstOrDefault(i => i.endsOn == null).Should().NotBeNull();
        testPpon.Identifiers.FirstOrDefault(i => i.endsOn != null).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_DoesNotInsertExistingIdentifiers_WhenPponExists()
    {
        var mockPponRepository = new Mock<IPponRepository>();
        var subscriber = new OrganisationUpdatedSubscriber(mockPponRepository.Object);
        var mockEvent = GivenOrganisationUpdatedEvent();
        var testPpon = GivenPpon();
        var pponIdentifier = CreatePponIdentifier();

        mockPponRepository
            .Setup(repo => repo.FindPponByPponIdAsync(pponIdentifier.Id))
            .ReturnsAsync(testPpon);
        testPpon.Identifiers = GetPersistenceIdentifiers(mockEvent.AllIdentifiers());

        var totalIdentifiersBeforeUpdate = testPpon.Identifiers.Count();

        mockEvent.AdditionalIdentifiers.Add(pponIdentifier);

        await subscriber.Handle(mockEvent);

        testPpon.Identifiers.Count.Should().Be(totalIdentifiersBeforeUpdate);
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