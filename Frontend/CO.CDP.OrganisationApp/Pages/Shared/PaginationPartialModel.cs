using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Shared
{
    public class PaginationPartialModel
    {
        [Required]
        public int CurrentPage { get; set; }

        [Required]
        public int TotalItems { get; set; }

        [Required]
        public int PageSize { get; set; }

        [Required]
        public string OrganisationType { get; set; } = "buyer";

        [Required]
        public string Url { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public Func<int, string> GetPageUrl => (page) =>
            $"{Url}?pageNumber={page}&pageSize={PageSize}";
    }
}