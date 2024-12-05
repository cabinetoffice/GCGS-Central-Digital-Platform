using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace healthCheck.HealthChecks;

public class ElastiCacheHealthCheck : IHealthCheck
{
    private readonly string _host;
    private readonly string _password;
    private readonly int _port;

    public ElastiCacheHealthCheck(string host, int port, string password)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _port = port;
        _password = password;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var details = new Dictionary<string, object>();

        try
        {
            // Stage 1: DNS Resolution
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(_host);
                details["DNS Resolution"] = addresses.Any() ? "Success" : "Failed";
            }
            catch (Exception ex)
            {
                details["DNS Resolution"] = $"Failed: {_host}, {ex.Message}";
                return HealthCheckResult.Unhealthy("Redis is unavailable", null, details);
            }

            // Stage 2: Port Connectivity
            try
            {
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(_host, _port);
                details["Port Connectivity"] = "Success";
            }
            catch (Exception ex)
            {
                details["Port Connectivity"] = $"Failed: {_port}, {ex.Message}";
                return HealthCheckResult.Unhealthy("Redis is unavailable", null, details);
            }

            // Stage 3: Redis Connection Test
            try
            {
                var options = new ConfigurationOptions
                {
                    EndPoints = { $"{_host}:{_port}" },
                    // Password = _password,
                    // Ssl = true,
                    // SslProtocols = SslProtocols.Tls12,
                    AbortOnConnectFail = false,
                    ConnectTimeout = 5000,
                    // AllowAdmin = true,
                    KeepAlive = 10
                };

                Console.WriteLine("Redis Configuration Options:");
                Console.WriteLine(ConfigurationOptionsToString(options));

                try
                {
                    var connection = await ConnectionMultiplexer.ConnectAsync(options);
                    if (connection.IsConnected)
                    {
                        details["Redis Connection"] = "Success";

                        var db = connection.GetDatabase();
                        var ping = db.Ping();
                        details["Redis PING Response"] = ping.ToString();

                        var server = connection.GetServer(_host, _port);
                        details["Redis Server Role"] = server.IsReplica ? "Replica" : "Primary";

                        var lastSave = server.LastSave();
                        details["Redis Last Save"] = lastSave.ToString("o");

                        return HealthCheckResult.Healthy("Redis is available", details);
                    }

                    details["Redis Connection"] = "Failed: Unable to establish connection";
                }
                catch (RedisConnectionException ex)
                {
                    details["Redis Connection"] = $"Failed with RedisConnectionException: {ex.Message}";
                }

                catch (Exception ex)
                {
                    details["Redis Connection"] = $"Failed with Exception: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                details["Redis Connection"] = $"Failed: {ex.Message}";
            }

            return HealthCheckResult.Unhealthy("Redis is unavailable", null, details);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Unexpected error during health check", ex, details);
        }
    }

    private string ConfigurationOptionsToString(ConfigurationOptions options)
    {
        return $@"
    AbortOnConnectFail: {options.AbortOnConnectFail}
    AllowAdmin: {options.AllowAdmin}
    ConnectTimeout: {options.ConnectTimeout}
    EndPoints: {string.Join(", ", options.EndPoints.Select(e => e.ToString()))}
    KeepAlive: {options.KeepAlive}
    Password: {options.Password ?? "null"}
    Ssl: {options.Ssl}
    SslProtocols: {options.SslProtocols}";
    }
}
