using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Services;

namespace CO.CDP.EntityVerification.Events;

public class OrganisationRegisteredEventHandler : IEventHandler
{
    private readonly IPponService _pponService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceProviderWrapper _spWrapper;
    private readonly IPponRepository _pponRepository;

    public OrganisationRegisteredEventHandler(IPponService pponService,
        IServiceProvider serviceProvider,
        IServiceProviderWrapper spWrapper,
        IPponRepository pponRepository)
    {
        _pponService = pponService;
        _serviceProvider = serviceProvider;
        _spWrapper = spWrapper;
        _pponRepository = pponRepository;
    }

    public void Action(EvMessage msg)
    {
        OrganisationRegisteredMessage newOrg = (OrganisationRegisteredMessage)msg;
        var pponid = _pponService.GeneratePponId();

        Ppon newIdentifier = new() { PponId  = pponid };

        using (var scope = _serviceProvider.CreateScope())
        {
            using (var dbContext = _spWrapper.GetRequiredService(scope.ServiceProvider))
            {
                _pponRepository.Save(dbContext, newIdentifier);
            }
        }
    }
}
