using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.Domain.Schools
{
    public class GetSchoolDetailsResponse
    {
        public IEnumerable<Establishment> Schools { get; }
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }
        private GetSchoolDetailsResponse(IEnumerable<Establishment> schools, bool isSuccess, string? errorMessage = null)
        {
            Schools = schools;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
        public static GetSchoolDetailsResponse Success(IEnumerable<Establishment> schools)
            => new(schools, true);
        public static GetSchoolDetailsResponse Failure(string error)
            => new(Enumerable.Empty<Establishment>(), false, error);
    }
}