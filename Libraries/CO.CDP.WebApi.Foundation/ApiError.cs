using System.Net;

namespace CO.CDP.WebApi.Foundation;

public abstract record ApiError(string Message);

public sealed record ServerError(string Message, HttpStatusCode StatusCode) : ApiError(Message);

public sealed record ClientError(string Message, HttpStatusCode StatusCode) : ApiError(Message);

public sealed record NetworkError(string Message, Exception? InnerException = null) : ApiError(Message);

public sealed record DeserialisationError(string Message, Exception? InnerException = null) : ApiError(Message);
