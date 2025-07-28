using CO.CDP.RegisterOfCommercialTools.WebApi.Models;
using System.Text.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class SearchService : ISearchService
{
    private readonly ICommercialToolsQueryBuilder _queryBuilder;
    private readonly ICommercialToolsRepository _repository;
    private readonly string _odataBaseUrl;

    public SearchService(ICommercialToolsQueryBuilder queryBuilder, ICommercialToolsRepository repository, IConfiguration configuration)
    {
        _queryBuilder = queryBuilder;
        _repository = repository;
        _odataBaseUrl = configuration.GetSection("ODataApi:BaseUrl").Value ?? throw new InvalidOperationException("ODataApi:BaseUrl configuration is required");
    }

    public async Task<SearchResponse> Search(SearchRequestDto request)
    {
        var queryBuilder = _queryBuilder
            .WithKeywords(request.Keyword ?? string.Empty)
            .WithStatus(request.Status ?? string.Empty)
            .FeeFrom(request.MinFees ?? 0)
            .FeeTo(request.MaxFees ?? decimal.MaxValue)
            .WithPageSize(request.PageSize)
            .WithPageNumber(request.PageNumber);


        // Add submission deadline filters if provided
        if (request.SubmissionDeadlineFrom.HasValue)
            queryBuilder = queryBuilder.SubmissionDeadlineFrom(request.SubmissionDeadlineFrom.Value);

        if (request.SubmissionDeadlineTo.HasValue)
            queryBuilder = queryBuilder.SubmissionDeadlineTo(request.SubmissionDeadlineTo.Value);

        if (request.ContractStartDateFrom.HasValue)
            queryBuilder = queryBuilder.ContractStartDateFrom(request.ContractStartDateFrom.Value);

        if (request.ContractStartDateTo.HasValue)
            queryBuilder = queryBuilder.ContractStartDateTo(request.ContractStartDateTo.Value);

        if (request.ContractEndDateFrom.HasValue)
            queryBuilder = queryBuilder.ContractEndDateFrom(request.ContractEndDateFrom.Value);

        if (request.ContractEndDateTo.HasValue)
            queryBuilder = queryBuilder.ContractEndDateTo(request.ContractEndDateTo.Value);

        var queryUrl = queryBuilder.Build(_odataBaseUrl);

        var results = await _repository.SearchCommercialTools(queryUrl);
        var totalCount = await _repository.GetCommercialToolsCount(queryUrl);

        return new SearchResponse
        {
            Results = results,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<SearchResultDto?> GetById(string id)
    {
        return await _repository.GetCommercialToolById(id);
    }
}