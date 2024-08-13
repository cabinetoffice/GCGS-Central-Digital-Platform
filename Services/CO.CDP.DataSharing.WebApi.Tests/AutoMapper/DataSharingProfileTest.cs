namespace CO.CDP.DataSharing.Tests.AutoMapper;
public class DataSharingProfileTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    [Fact]
    public void ConfigurationIsValid()
    {
        mapperFixture.Configuration.AssertConfigurationIsValid();
    }
}