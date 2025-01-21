using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Shared
{
    public class PaginationPartialModel
    {
        [Required]
        public required int CurrentPage { get; set; }

        [Required]
        public required int TotalItems { get; set; }

        [Required]
        public required int PageSize { get; set; }

        [Required]
        public required string Url { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public Func<int, string> GetPageUrl => (page) =>
            $"{Url}?pageNumber={page}&pageSize={PageSize}";
    }
}