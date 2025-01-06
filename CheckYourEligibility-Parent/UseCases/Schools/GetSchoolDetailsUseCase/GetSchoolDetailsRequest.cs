namespace CheckYourEligibility_FrontEnd.UseCases.Schools.GetSchoolDetailsUseCase
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
