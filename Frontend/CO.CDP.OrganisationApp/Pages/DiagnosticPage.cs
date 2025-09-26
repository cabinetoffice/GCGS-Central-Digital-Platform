using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using CO.CDP.UI.Foundation.Pages;
using System.Text;

namespace CO.CDP.OrganisationApp.Pages;

public class DiagnosticPage(
    ISession session,
    IHttpContextAccessor httpContextAccessor,
    IAuthorityClient authorityClient,
    IConfiguration configuration) : DiagnosticPageBase(httpContextAccessor, configuration)
{
    protected override async Task AddAdditionalContent(StringBuilder sb)
    {
        var userUrn = session.Get<UserDetails>(Session.UserDetailsKey)?.UserUrn;
        if (userUrn != null)
        {
            var tokens = await authorityClient.GetAuthTokens(userUrn);
            if (tokens != null)
            {
                sb.Append("<h2 class=\"heading-l\">Authority Tokens</h2>");
                sb.Append("<dl class=\"summary-list\">");

                sb.Append("<div class=\"summary-list__row\">");
                sb.Append("<dt class=\"summary-list__key\">Authority Access Token</dt>");
                sb.Append("<dd class=\"summary-list__value\">");
                sb.Append($"<div class=\"token\" id=\"aat\">{tokens.AccessToken}</div>");
                sb.Append($"<p class=\"body-s\">");
                sb.Append($"<a href=\"https://jwt.io/#debugger-io?token={tokens.AccessToken}\" target=\"_blank\" class=\"link\">Decode token</a>");
                sb.Append("</p>");
                sb.Append("</dd>");
                sb.Append("</div>");

                sb.Append("<div class=\"summary-list__row\">");
                sb.Append("<dt class=\"summary-list__key\">Access Token Expiry</dt>");
                sb.Append($"<dd class=\"summary-list__value\">{tokens.AccessTokenExpiry}</dd>");
                sb.Append("</div>");

                sb.Append("<div class=\"summary-list__row\">");
                sb.Append("<dt class=\"summary-list__key\">Authority Refresh Token</dt>");
                sb.Append($"<dd class=\"summary-list__value\"><div class=\"token\">{tokens.RefreshToken}</div></dd>");
                sb.Append("</div>");

                sb.Append("<div class=\"summary-list__row\">");
                sb.Append("<dt class=\"summary-list__key\">Refresh Token Expiry</dt>");
                sb.Append($"<dd class=\"summary-list__value\">{tokens.RefreshTokenExpiry}</dd>");
                sb.Append("</div>");

                sb.Append("</dl>");
            }
        }
    }
}