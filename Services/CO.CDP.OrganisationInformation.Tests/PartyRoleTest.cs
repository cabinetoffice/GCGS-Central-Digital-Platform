using FluentAssertions;

namespace CO.CDP.OrganisationInformation.Tests;

public class PartyRoleTest
{
    [Fact]
    public void ThePartyRoleCodeStartsWithALowercaseLetter()
    {
        PartyRole.Buyer.AsCode().Should().Be("buyer");
        PartyRole.InterestedParty.AsCode().Should().Be("interestedParty");
    }
}