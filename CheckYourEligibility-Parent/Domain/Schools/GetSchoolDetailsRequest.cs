namespace CheckYourEligibility_FrontEnd.Domain.Schools
{
    public class GetSchoolDetailsRequest
    {
        public string Query { get; }
        public GetSchoolDetailsRequest(string query)
        {
            Query = query ?? throw new ArgumentNullException(nameof(query));
        }
    }
}