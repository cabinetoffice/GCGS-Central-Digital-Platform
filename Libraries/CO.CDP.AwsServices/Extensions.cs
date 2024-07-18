using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using CO.CDP.AwsServices.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.AwsServices;

public static class Extensions
{
    public static IServiceCollection AddAwsCofiguration(
        this IServiceCollection services, IConfiguration configuration)
    {
        var awsSection = configuration.GetSection("Aws");

        var awsConfig = awsSection.Get<AwsConfiguration>()
            ?? throw new Exception("Aws environment configuration missing.");

        if (!string.IsNullOrWhiteSpace(awsConfig.ServiceURL) && !string.IsNullOrWhiteSpace(awsConfig.Region))
            throw new Exception("Either provide aws service url or aws region endpoint configuration.");

        var awsOptions = new AWSOptions
        {
            Credentials = new BasicAWSCredentials(awsConfig.AccessKeyId, awsConfig.SecretAccessKey)
        };

        if (!string.IsNullOrWhiteSpace(awsConfig.ServiceURL))
            awsOptions.DefaultClientConfig.ServiceURL = awsConfig.ServiceURL;

        if (!string.IsNullOrWhiteSpace(awsConfig.Region))
            awsOptions.Region = RegionEndpoint.GetBySystemName(awsConfig.Region);

        return services
            .Configure<AwsConfiguration>(awsSection)
            .AddDefaultAWSOptions(awsOptions);
    }

    public static IServiceCollection AddAwsS3Service(this IServiceCollection services)
    {
        return services
            .AddAWSService<IAmazonS3>()
            .AddSingleton<ITransferUtility, TransferUtilityWrapper>()
            .AddSingleton<IFileHostManager, AwsFileManager>();
    }
}