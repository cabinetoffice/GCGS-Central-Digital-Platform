namespace CO.CDP.EntityVerification.Model;

public record LookupIdentifierQuery(string Identifier)
{
    public bool TryGetIdentifier(out string scheme, out string id)
    {
        if (!string.IsNullOrEmpty(Identifier))
        {
            var parts = Identifier.Split(':');
            if (parts.Length == 2)
            {
                scheme = parts[0];
                id = parts[1];
                return true;
            }
        }

        scheme = string.Empty;
        id = string.Empty;
        return false;
    }
}
