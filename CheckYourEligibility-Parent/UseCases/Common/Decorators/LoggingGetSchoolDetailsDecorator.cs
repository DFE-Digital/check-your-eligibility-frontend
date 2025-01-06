using CheckYourEligibility_FrontEnd.Domain.Schools;
using CheckYourEligibility_FrontEnd.UseCases.Schools.GetSchoolDetailsUseCase;

namespace CheckYourEligibility_FrontEnd.UseCases.Common.Decorators
{
    public class LoggingGetSchoolDetailsUseCase : IGetSchoolDetailsUseCase
    {
        private readonly IGetSchoolDetailsUseCase _inner;
        private readonly ILogger<LoggingGetSchoolDetailsUseCase> _logger;

        public LoggingGetSchoolDetailsUseCase(
            IGetSchoolDetailsUseCase inner,
            ILogger<LoggingGetSchoolDetailsUseCase> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GetSchoolDetailsResponse> ExecuteAsync(GetSchoolDetailsRequest request)
        {
            try
            {
                return await _inner.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching schools with query: {Query}", request.Query);
                throw;
            }
        }
    }
}
