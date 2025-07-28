namespace CO.CDP.RegisterOfCommercialTools.WebApi.Models;

public class AwsSettings
{
    public string? Region { get; set; }
    public AwsCredentials? Credentials { get; set; }
}

public class AwsCredentials
{
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
}

public class CognitoAuthenticationSettings
{
    public string? Domain { get; set; }
    public string? UserPoolId { get; set; }
    public string? UserPoolClientId { get; set; }
    public string? UserPoolClientSecret { get; set; }
}