using CO.CDP.Functional;

namespace CO.CDP.WebApi.Foundation;

public class ApiResult<TValue> : Result<ApiError, TValue>
{
    private ApiResult(Either<ApiError, TValue> either) : base(either)
    {
    }

    public new static ApiResult<TValue> Success(TValue value) => new(Right(value));
    public new static ApiResult<TValue> Failure(ApiError error) => new(Left(error));
}
