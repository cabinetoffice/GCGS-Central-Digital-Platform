using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text.Json;

namespace CO.CDP.UI.Foundation.Pages;

public abstract class DiagnosticPageBase(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    : IDiagnosticPage
{
    protected readonly IConfiguration Configuration = configuration;

    public virtual async Task<string> GetContent()
    {
        string? oneLoginToken = null;
        string? oneLoginTokenExpiry = null;
        string? nonce = null;

        if (httpContextAccessor.HttpContext != null)
        {
            oneLoginToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            oneLoginTokenExpiry = await httpContextAccessor.HttpContext.GetTokenAsync("expires_at");

            if (httpContextAccessor.HttpContext.Items["ContentSecurityPolicyNonce"] is string contextNonce)
            {
                nonce = contextNonce;
            }
        }

        StringBuilder sb = new("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Diagnostic Page</title>");

        sb.Append(@$"<style type='text/css' nonce='{nonce}'>
            body {{
                font-family: ""GDS Transport"", arial, sans-serif;
                font-size: 19px;
                line-height: 1.31579;
                margin: 0;
                padding: 20px;
                background-color: #ffffff;
                color: #0b0c0c;
            }}
            .container {{
                max-width: 960px;
                margin: 0 auto;
            }}
            .heading-xl {{
                font-size: 48px;
                line-height: 1.04167;
                font-weight: bold;
                margin: 0 0 30px 0;
                color: #0b0c0c;
            }}
            .heading-l {{
                font-size: 36px;
                line-height: 1.11111;
                font-weight: bold;
                margin: 40px 0 20px 0;
                color: #0b0c0c;
            }}
            .heading-m {{
                font-size: 24px;
                line-height: 1.25;
                font-weight: bold;
                margin: 30px 0 15px 0;
                color: #0b0c0c;
            }}
            .summary-list {{
                border-top: 1px solid #b1b4b6;
                margin: 20px 0;
            }}
            .summary-list__row {{
                border-bottom: 1px solid #b1b4b6;
                display: flex;
                flex-wrap: wrap;
            }}
            .summary-list__key {{
                font-weight: bold;
                margin: 0;
                padding: 10px 20px 10px 0;
                width: 40%;
                min-width: 250px;
                flex-shrink: 0;
            }}
            .summary-list__value {{
                margin: 0;
                padding: 10px 0;
                flex: 1;
                word-wrap: break-word;
                word-break: break-all;
                overflow-wrap: anywhere;
                min-width: 0;
            }}
            .tag {{
                display: inline-block;
                padding: 5px 8px;
                font-size: 14px;
                font-weight: bold;
                text-decoration: none;
                text-transform: uppercase;
                letter-spacing: 1px;
                border-radius: 3px;
            }}
            .tag--green {{
                background-color: #00703c;
                color: #ffffff;
            }}
            .tag--red {{
                background-color: #d4351c;
                color: #ffffff;
            }}
            .tag--yellow {{
                background-color: #ffdd00;
                color: #0b0c0c;
            }}
            .tag--orange {{
                background-color: #f47738;
                color: #ffffff;
            }}
            .tag--grey {{
                background-color: #6f777b;
                color: #ffffff;
            }}
            .token {{
                word-wrap: break-word;
                word-break: break-all;
                overflow-wrap: anywhere;
                white-space: pre-wrap;
                font-family: ""Courier New"", monospace;
                font-size: 14px;
                background-color: #f3f2f1;
                padding: 15px;
                border: 1px solid #b1b4b6;
                border-radius: 3px;
                margin: 5px 0;
                display: block;
                min-height: 80px;
                line-height: 1.4;
                max-width: 100%;
            }}
            .body-s {{
                font-size: 16px;
                margin: 10px 0 0 0;
            }}
            .link {{
                color: #1d70b8;
                text-decoration: underline;
            }}
            .link:hover {{
                color: #003078;
                text-decoration-thickness: 3px;
            }}
            .error-summary {{
                border: 3px solid #d4351c;
                padding: 20px;
                margin: 20px 0;
            }}
            .error-summary__title {{
                margin: 0 0 15px 0;
                font-size: 24px;
                font-weight: bold;
                color: #d4351c;
            }}
            .error-summary__body p {{
                margin: 0;
            }}
            code {{
                font-family: ""Courier New"", monospace;
                background-color: #f3f2f1;
                padding: 2px 4px;
                border-radius: 3px;
            }}
        </style>");

        sb.Append("</head><body>");
        sb.Append("<div class=\"container\">");
        sb.Append("<h1 class=\"heading-xl\">Diagnostic Page</h1>");

        AddAuthenticationTokens(sb, oneLoginToken, oneLoginTokenExpiry);

        AddSystemInformation(sb);

        AddFeatureFlags(sb);

        AddConfigurationInfo(sb, nonce);

        AddSessionInformation(sb);

        await AddAdditionalContent(sb);

        sb.Append("<script" + (string.IsNullOrWhiteSpace(nonce) ? "" : $" nonce=\"{nonce}\"") + @">
            const copyContent = async (id) => {
                try {
                    const text = document.getElementById(id).innerHTML;
                    await navigator.clipboard.writeText(text);
                } catch (e) { }
            }

            Array.from(document.querySelectorAll('[data-token-id]')).forEach(copyButton => {
                copyButton.addEventListener('click', (e) => {
                    e.preventDefault();
                    copyContent(copyButton.dataset.tokenId)
                });
            });
        </script>");

        sb.Append("</div>");
        sb.Append("</body></html>");

        return sb.ToString();
    }

    protected virtual void AddSystemInformation(StringBuilder sb)
    {
        sb.Append("<h2 class=\"heading-l\">System Information</h2>");
        sb.Append("<dl class=\"summary-list\">");

        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null)
        {
            var version = assembly.GetName().Version?.ToString() ?? "Unknown";
            sb.Append("<div class=\"summary-list__row\">");
            sb.Append("<dt class=\"summary-list__key\">Assembly Version</dt>");
            sb.Append($"<dd class=\"summary-list__value\">{version}</dd>");
            sb.Append("</div>");
        }

        sb.Append("<div class=\"summary-list__row\">");
        sb.Append("<dt class=\"summary-list__key\">Machine Name</dt>");
        sb.Append($"<dd class=\"summary-list__value\">{Environment.MachineName}</dd>");
        sb.Append("</div>");

        sb.Append("<div class=\"summary-list__row\">");
        sb.Append("<dt class=\"summary-list__key\">OS Version</dt>");
        sb.Append($"<dd class=\"summary-list__value\">{Environment.OSVersion}</dd>");
        sb.Append("</div>");

        sb.Append("<div class=\"summary-list__row\">");
        sb.Append("<dt class=\"summary-list__key\">.NET Version</dt>");
        sb.Append($"<dd class=\"summary-list__value\">{Environment.Version}</dd>");
        sb.Append("</div>");

        sb.Append("<div class=\"summary-list__row\">");
        sb.Append("<dt class=\"summary-list__key\">Current Directory</dt>");
        sb.Append($"<dd class=\"summary-list__value\">{Environment.CurrentDirectory}</dd>");
        sb.Append("</div>");

        sb.Append("<div class=\"summary-list__row\">");
        sb.Append("<dt class=\"summary-list__key\">UTC Time</dt>");
        sb.Append($"<dd class=\"summary-list__value\">{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</dd>");
        sb.Append("</div>");

        sb.Append("</dl>");
    }

    protected virtual void AddFeatureFlags(StringBuilder sb)
    {
        sb.Append("<h2 class=\"heading-l\">Feature Flags</h2>");

        var featureFlagSection = Configuration.GetSection("FeatureFlags");
        var featuresSection = Configuration.GetSection("Features");

        if (featureFlagSection.Exists() || featuresSection.Exists())
        {
            sb.Append("<dl class=\"summary-list\">");

            if (featureFlagSection.Exists())
            {
                foreach (var child in featureFlagSection.GetChildren())
                {
                    sb.Append("<div class=\"summary-list__row\">");
                    sb.Append($"<dt class=\"summary-list__key\">{child.Key}</dt>");
                    sb.Append("<dd class=\"summary-list__value\">");
                    sb.Append(GetStatusTag(child.Value));
                    sb.Append("</dd>");
                    sb.Append("</div>");
                }
            }

            if (featuresSection.Exists())
            {
                foreach (var child in featuresSection.GetChildren())
                {
                    sb.Append("<div class=\"summary-list__row\">");
                    sb.Append($"<dt class=\"summary-list__key\">{child.Key}</dt>");
                    sb.Append("<dd class=\"summary-list__value\">");
                    sb.Append(GetStatusTag(child.Value));
                    sb.Append("</dd>");
                    sb.Append("</div>");
                }
            }

            sb.Append("</dl>");
        }
        else
        {
            sb.Append("<p class=\"body\">No feature flags configured</p>");
        }
    }

    protected virtual void AddConfigurationInfo(StringBuilder sb, string? nonce)
    {
        sb.Append("<h2 class=\"heading-l\">Configuration Information</h2>");
        sb.Append("<dl class=\"summary-list\">");

        var environment = Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown";
        sb.Append("<div class=\"summary-list__row\">");
        sb.Append("<dt class=\"summary-list__key\">Environment</dt>");
        sb.Append("<dd class=\"summary-list__value\">");
        sb.Append(GetEnvironmentTag(environment));
        sb.Append("</dd>");
        sb.Append("</div>");

        var appName = Configuration["ApplicationName"] ?? Configuration["Application:Name"] ?? "Unknown";
        sb.Append("<div class=\"summary-list__row\">");
        sb.Append("<dt class=\"summary-list__key\">Application</dt>");
        sb.Append($"<dd class=\"summary-list__value\">{appName}</dd>");
        sb.Append("</div>");

        sb.Append("<div class=\"summary-list__row\">");
        sb.Append("<dt class=\"summary-list__key\">Content Security Policy Nonce</dt>");
        sb.Append("<dd class=\"summary-list__value\">");
        if (!string.IsNullOrEmpty(nonce))
        {
            sb.Append($"<code>{nonce}</code>");
        }
        else
        {
            sb.Append("<strong class=\"tag tag--grey\">Not set</strong>");
        }

        sb.Append("</dd>");
        sb.Append("</div>");

        sb.Append("</dl>");

        var connectionStrings = Configuration.GetSection("ConnectionStrings");
        if (connectionStrings.Exists())
        {
            sb.Append("<h3 class=\"heading-m\">Connection Strings</h3>");
            sb.Append("<dl class=\"summary-list\">");
            foreach (var connectionString in connectionStrings.GetChildren())
            {
                var maskedValue = MaskConnectionString(connectionString.Value ?? "");
                sb.Append("<div class=\"summary-list__row\">");
                sb.Append($"<dt class=\"summary-list__key\">{connectionString.Key}</dt>");
                sb.Append($"<dd class=\"summary-list__value\"><code>{maskedValue}</code></dd>");
                sb.Append("</div>");
            }

            sb.Append("</dl>");
        }
    }

    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "Not configured";

        var parts = connectionString.Split(';');
        var maskedParts = new List<string>();

        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();
            if (trimmedPart.StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ||
                trimmedPart.StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
            {
                var keyValuePair = trimmedPart.Split('=', 2);
                maskedParts.Add($"{keyValuePair[0]}=***");
            }
            else
            {
                maskedParts.Add(trimmedPart);
            }
        }

        return string.Join("; ", maskedParts);
    }

    private static string GetStatusTag(string? value)
    {
        return value?.ToLower() switch
        {
            "true" => "<strong class=\"tag tag--green\">Enabled</strong>",
            "false" => "<strong class=\"tag tag--red\">Disabled</strong>",
            _ => $"<strong class=\"tag tag--grey\">{value ?? "Unknown"}</strong>"
        };
    }

    private static string GetEnvironmentTag(string environment)
    {
        return $"<strong class=\"tag tag--yellow\">{environment}</strong>";
    }

    protected virtual void AddAuthenticationTokens(StringBuilder sb, string? oneLoginToken, string? oneLoginTokenExpiry)
    {
        if (!string.IsNullOrWhiteSpace(oneLoginToken) || !string.IsNullOrWhiteSpace(oneLoginTokenExpiry))
        {
            sb.Append("<h2 class=\"heading-l\">Authentication Tokens</h2>");
            sb.Append("<dl class=\"summary-list\">");

            if (!string.IsNullOrWhiteSpace(oneLoginToken))
            {
                sb.Append("<div class=\"summary-list__row\">");
                sb.Append("<dt class=\"summary-list__key\">OneLogin Access Token</dt>");
                sb.Append("<dd class=\"summary-list__value\">");
                sb.Append($"<div class=\"token\">{oneLoginToken}</div>");
                sb.Append($"<p class=\"body-s\">");
                sb.Append(
                    $"<a href=\"https://jwt.io/#debugger-io?token={oneLoginToken}\" target=\"_blank\" class=\"link\">Decode token</a>");
                sb.Append("</p>");
                sb.Append("</dd>");
                sb.Append("</div>");
            }

            if (!string.IsNullOrWhiteSpace(oneLoginTokenExpiry))
            {
                sb.Append("<div class=\"summary-list__row\">");
                sb.Append("<dt class=\"summary-list__key\">Token Expiry</dt>");
                sb.Append(
                    $"<dd class=\"summary-list__value\">{DateTimeOffset.Parse(oneLoginTokenExpiry).ToLocalTime()}</dd>");
                sb.Append("</div>");
            }

            sb.Append("</dl>");
        }
    }

    protected virtual void AddSessionInformation(StringBuilder sb)
    {
        sb.Append("<h2 class=\"heading-l\">Session Information</h2>");

        try
        {
            var session = httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                sb.Append("<p class=\"body\">Session not available</p>");
                return;
            }

            sb.Append("<dl class=\"summary-list\">");

            var userDetails = GetSessionValue<object>(session, "UserDetails");
            sb.Append("<div class=\"summary-list__row\">");
            sb.Append("<dt class=\"summary-list__key\">User Session</dt>");
            sb.Append("<dd class=\"summary-list__value\">");
            if (userDetails != null)
            {
                sb.Append("<strong class=\"tag tag--green\">Active</strong>");
            }
            else
            {
                sb.Append("<strong class=\"tag tag--red\">Inactive</strong>");
            }

            sb.Append("</dd>");
            sb.Append("</div>");

            var authTokens = GetSessionValue<object>(session, "UserAuthTokens");
            sb.Append("<div class=\"summary-list__row\">");
            sb.Append("<dt class=\"summary-list__key\">Auth Tokens</dt>");
            sb.Append("<dd class=\"summary-list__value\">");
            if (authTokens != null)
            {
                sb.Append("<strong class=\"tag tag--green\">Present</strong>");
            }
            else
            {
                sb.Append("<strong class=\"tag tag--red\">Not found</strong>");
            }

            sb.Append("</dd>");
            sb.Append("</div>");

            sb.Append("</dl>");

            if (userDetails is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
            {
                sb.Append("<h3 class=\"heading-m\">Session Data</h3>");
                sb.Append("<dl class=\"summary-list\">");
                foreach (var property in jsonElement.EnumerateObject())
                {
                    sb.Append("<div class=\"summary-list__row\">");
                    sb.Append($"<dt class=\"summary-list__key\">{property.Name}</dt>");
                    sb.Append($"<dd class=\"summary-list__value\">{GetJsonValueDisplay(property.Value)}</dd>");
                    sb.Append("</div>");
                }

                sb.Append("</dl>");
            }
        }
        catch (Exception ex)
        {
            sb.Append("<div class=\"error-summary\">");
            sb.Append("<h2 class=\"error-summary__title\">Session Error</h2>");
            sb.Append("<div class=\"error-summary__body\">");
            sb.Append($"<p class=\"body\">{ex.Message}</p>");
            sb.Append("</div>");
            sb.Append("</div>");
        }
    }

    private T? GetSessionValue<T>(ISession session, string key) where T : class
    {
        try
        {
            var value = session.GetString(key);
            return value == null ? null : JsonSerializer.Deserialize<T>(value);
        }
        catch
        {
            return null;
        }
    }

    private static string GetJsonValueDisplay(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => $"\"{element.GetString()}\"",
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            JsonValueKind.Object => $"Object ({element.EnumerateObject().Count()} properties)",
            JsonValueKind.Array => $"Array ({element.GetArrayLength()} items)",
            _ => element.GetRawText()
        };
    }

    protected virtual Task AddAdditionalContent(StringBuilder sb)
    {
        return Task.CompletedTask;
    }
}