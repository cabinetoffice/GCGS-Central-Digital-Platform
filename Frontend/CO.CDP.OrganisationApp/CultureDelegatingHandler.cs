using System.Globalization;

namespace CO.CDP.OrganisationApp;

public class CultureDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);
        return base.SendAsync(request, cancellationToken);
    }
}