namespace CO.CDP.DataSharing.WebApi.Tests.AutoMapper;

public class DataSharingProfileTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    [Fact]
    public void ConfigurationIsValid()
    {
        mapperFixture.Configuration?.AssertConfigurationIsValid();
    }
}