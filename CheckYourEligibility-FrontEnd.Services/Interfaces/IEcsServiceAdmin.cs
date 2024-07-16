using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.Services
{
    public interface IEcsServiceAdmin
    {
        Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string batchCheckUrl);
        Task<CheckEligibilityBulkResponse> GetBulkCheckResults(string resultsUrl);
        Task<CheckEligibilityResponseBulk> PostBulkCheck(CheckEligibilityRequestBulk requestBody);
    }
}