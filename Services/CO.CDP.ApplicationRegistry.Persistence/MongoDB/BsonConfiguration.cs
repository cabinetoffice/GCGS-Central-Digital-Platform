using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace CO.CDP.ApplicationRegistry.Persistence.MongoDB;

/// <summary>
/// All MongoDB-specific BSON serialization configuration.
/// Scoped exclusively to the CO.CDP.ApplicationRegistry namespace — entity POCOs carry no BSON attributes.
/// Call <see cref="Register"/> once at application startup (idempotent).
/// </summary>
public static class BsonConfiguration
{
    private static bool _registered;
    private static readonly object _lock = new();

    public static void Register()
    {
        if (_registered) return;

        lock (_lock)
        {
            if (_registered) return;

            // Represent Guid as string ("xxxxxxxx-xxxx-...") rather than the legacy binary sub-type 3/4.
            // Must be registered before any BSON serialization occurs.
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

            // Apply camelCase element names and ignore extra elements (forward-compatible)
            // only to types in the ApplicationRegistry persistence namespace.
            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(ignoreExtraElements: true),
                new EnumRepresentationConvention(BsonType.String)
            };

            ConventionRegistry.Register(
                "AppRegistry",
                pack,
                t => t.Namespace?.StartsWith("CO.CDP.ApplicationRegistry") == true);

            _registered = true;
        }
    }
}
