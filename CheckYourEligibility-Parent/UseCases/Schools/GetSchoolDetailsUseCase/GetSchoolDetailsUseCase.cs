using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;

namespace CheckYourEligibility_FrontEnd.UseCases.Schools.GetSchoolDetailsUseCase
{
    // GetSchoolDetailsUseCase.cs
    public class GetSchoolDetailsUseCase : IGetSchoolDetailsUseCase
    {
        private readonly IEcsServiceParent _parentService;
        private readonly ILogger<GetSchoolDetailsUseCase> _logger;

        public GetSchoolDetailsUseCase(
            IEcsServiceParent parentService,
            ILogger<GetSchoolDetailsUseCase> logger)
        {
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GetSchoolDetailsResponse> ExecuteAsync(GetSchoolDetailsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Query) || request.Query.Length < 3)
                {
                    return GetSchoolDetailsResponse.Failure("Query must be at least 3 characters long.");
                }

                var results = await _parentService.GetSchool(request.Query);
                if (results != null)
                {
                    return GetSchoolDetailsResponse.Success(results.Data);
                }

                return GetSchoolDetailsResponse.Success(new List<Establishment>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving school details for query: {Query}", request.Query);
                return GetSchoolDetailsResponse.Failure("An error occurred while retrieving school details.");
            }
        }
    }
}
