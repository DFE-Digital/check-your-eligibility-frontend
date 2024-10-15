using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.Services
{
    public interface IEcsCheckService
    {
        // bulk
        Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string batchCheckUrl);
        Task<CheckEligibilityBulkResponse> GetBulkCheckResults(string resultsUrl);
        Task<CheckEligibilityResponseBulk> PostBulkCheck(CheckEligibilityRequestBulk_Fsm requestBody);
        // single
        Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest_Fsm requestBody);
        Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody);  
    }
}