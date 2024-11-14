using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authentication;
using System.Text;

namespace CO.CDP.OrganisationApp.Pages;

public interface IDiagnosticPage
{
    Task<string> GetContent();
}

public class DiagnosticPage(
    ISession session,
    IHttpContextAccessor httpContextAccessor,
    IAuthorityClient authorityClient) : IDiagnosticPage
{
    public async Task<string> GetContent()
    {
        string? oneLoginToken = null;
        string? oneLoginTokenExpiry = null;

        if (httpContextAccessor.HttpContext != null)
        {
            oneLoginToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            oneLoginTokenExpiry = await httpContextAccessor.HttpContext.GetTokenAsync("expires_at");
        }

        static string tokenHtml(string text, string value, string id) =>
            $"<p><strong>{text} (<a target='_blank' href='https://jwt.io/#debugger-io?token={value}'>Decode</a>) (<a style='text-decoration: none;' href=\"javascript:copyContent('{id}');\">📄 Copy</a>):</strong><br/><span id='{id}' style='overflow-wrap: break-word;'>{value}</span></p>";

        StringBuilder sb = new("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Diagnostic Page</title></head><body><h1>Diagnostic Page</h1>");

        if (!string.IsNullOrWhiteSpace(oneLoginToken))
        {
            sb.Append(tokenHtml("OneLogin Access Token", oneLoginToken, "oat"));
        }

        if (!string.IsNullOrWhiteSpace(oneLoginTokenExpiry))
        {
            sb.Append($"<p><strong>OneLogin Access Token Expiry: </strong><br/>{DateTimeOffset.Parse(oneLoginTokenExpiry).ToLocalTime()}</p>");
        }

        var userUrn = session.Get<UserDetails>(Session.UserDetailsKey)?.UserUrn;
        if (userUrn != null)
        {
            var tokens = await authorityClient.GetAuthTokens(userUrn);
            if (tokens != null)
            {
                sb.Append(tokenHtml("Authority Access Token", tokens.AccessToken, "aat"));
                sb.Append($"<p><strong>Authority Access Token Expiry: </strong><br/>{tokens.AccessTokenExpiry}</p>");

                sb.Append($"<p><strong>Authority Refresh Token: </strong><br/><span style='overflow-wrap: break-word;'>{tokens.RefreshToken}</span></p>");
                sb.Append($"<p><strong>Authority Refresh Token Expiry: </strong><br/>{tokens.RefreshTokenExpiry}</p>");
            }
        }

        sb.Append("<script>const copyContent = async (id) => {try {const text = document.getElementById(id).innerHTML;await navigator.clipboard.writeText(text);} catch (e) {}}</script>");

        sb.Append("</body></html>");

        return sb.ToString();
    }
}