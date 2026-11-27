using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetUserClaimsOrganisationRoleTest
{
    [Theory]
    [InlineData(new[] { "ADMIN", "RESPONDER", "EDITOR" }, "ADMIN")]
    [InlineData(new[] { "EDITOR", "ADMIN" }, "ADMIN")]
    [InlineData(new[] { "RESPONDER", "VIEWER" }, "VIEWER")]
    [InlineData(new[] { "RESPONDER", "EDITOR" }, "EDITOR")]
    [InlineData(new[] { "RESPONDER" }, "RESPONDER")]
    [InlineData(new[] { "admin" }, "ADMIN")]
    [InlineData(new[] { "SOMETHING_NEW" }, "SOMETHING_NEW")]
    [InlineData(new string[0], "")]
    public void ItReportsTheHighestPrivilegeOrganisationScope(string[] scopes, string expected)
    {
        GetUserClaimsUseCase.SelectOrganisationRole(scopes).Should().Be(expected);
    }
}
