
namespace CheckYourEligibility_FrontEnd.ViewModels
{
    public class PaginationPartialViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalRecords { get; set; }
        public string ControllerName { get; set; }
    }
}
