using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_DfeSignIn;
using CheckYourEligibility_FrontEnd.Models; // For Constants and related models
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases.Admin
{
    /// <summary>
    /// Represents the result of executing the AdminLoaderUseCase.
    /// </summary>
    public class AdminLoaderResult
    {
        /// <summary>
        /// The name of the view to render.
        /// </summary>
        public string ViewName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the use case encountered an error.
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// Optional updated response JSON (for example, when the status is queued).
        /// </summary>
        public string? UpdatedResponseJson { get; set; }

        /// <summary>
        /// The parsed check eligibility status.
        /// </summary>
        public CheckEligibilityStatus Status { get; set; }

        /// <summary>
        /// The status data model from the check service.
        /// </summary>
        public StatusValue? Data { get; set; }

        public static AdminLoaderResult Success(string viewName, CheckEligibilityStatus status, StatusValue? data, string? updatedResponseJson = null) =>
            new AdminLoaderResult { IsError = false, ViewName = viewName, Status = status, Data = data, UpdatedResponseJson = updatedResponseJson };

        public static AdminLoaderResult Error(string viewName) =>
            new AdminLoaderResult { IsError = true, ViewName = viewName };

        /// <summary>
        /// Enables tuple deconstruction for tests.
        /// </summary>
        public void Deconstruct(out string viewName, out StatusValue? model, out CheckEligibilityStatus status)
        {
            viewName = ViewName;
            model = Data;
            status = Status;
        }
    }

    /// <summary>
    /// Interface for the AdminLoader use case.
    /// </summary>
    public interface IAdminLoaderUseCase
    {
        /// <summary>
        /// Processes the eligibility response and determines which view to render.
        /// </summary>
        /// <param name="responseJson">The JSON string from TempData.</param>
        /// <param name="user">The current HttpContext.User.</param>
        /// <returns>An AdminLoaderResult indicating the outcome.</returns>
        Task<AdminLoaderResult> ExecuteAsync(string? responseJson, ClaimsPrincipal user);
    }

    /// <summary>
    /// Implementation of the AdminLoader use case.
    /// </summary>
    public class AdminLoaderUseCase : IAdminLoaderUseCase
    {
        private readonly ILogger<AdminLoaderUseCase> _logger;
        private readonly IEcsCheckService _checkService;

        public AdminLoaderUseCase(ILogger<AdminLoaderUseCase> logger, IEcsCheckService checkService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _checkService = checkService ?? throw new ArgumentNullException(nameof(checkService));
        }

        public async Task<AdminLoaderResult> ExecuteAsync(string? responseJson, ClaimsPrincipal user)
        {
            if (string.IsNullOrEmpty(responseJson))
            {
                _logger.LogWarning("No response data found.");
                return AdminLoaderResult.Error("Outcome/Technical_Error");
            }

            var response = JsonConvert.DeserializeObject<CheckEligibilityResponse>(responseJson);
            _logger.LogInformation($"Check status processed: {response?.Data?.Status}");

            // Call the service to check the current status.
            var checkStatus = await _checkService.GetStatus(response);
            if (checkStatus == null || checkStatus.Data == null)
            {
                _logger.LogWarning("Null response received from GetStatus.");
                return AdminLoaderResult.Error("Outcome/Technical_Error");
            }

            _logger.LogInformation($"Received status: {checkStatus.Data.Status}");
            Enum.TryParse(checkStatus.Data.Status, out CheckEligibilityStatus status);

            // Determine if the user belongs to a Local Authority (LA) based on claims.
            bool isLA = user?.FindFirst("OrganisationCategoryName")?.Value == Constants.CategoryTypeLA;

            switch (status)
            {
                case CheckEligibilityStatus.eligible:
                    return AdminLoaderResult.Success(isLA ? "Outcome/Eligible_LA" : "Outcome/Eligible", CheckEligibilityStatus.eligible, checkStatus.Data);
                case CheckEligibilityStatus.notEligible:
                    return AdminLoaderResult.Success(isLA ? "Outcome/Not_Eligible_LA" : "Outcome/Not_Eligible", CheckEligibilityStatus.notEligible, checkStatus.Data);
                case CheckEligibilityStatus.parentNotFound:
                    return AdminLoaderResult.Success("Outcome/Not_Found", CheckEligibilityStatus.parentNotFound, checkStatus.Data);
                case CheckEligibilityStatus.DwpError:
                    return AdminLoaderResult.Success("Outcome/Technical_Error", CheckEligibilityStatus.DwpError, checkStatus.Data);
                case CheckEligibilityStatus.queuedForProcessing:
                    // If still processing, return the same JSON to be saved back for the next poll.
                    return AdminLoaderResult.Success("Loader", CheckEligibilityStatus.queuedForProcessing, checkStatus.Data, responseJson);
                default:
                    _logger.LogError($"Unknown Status {status}");
                    return AdminLoaderResult.Error("Outcome/Technical_Error");
            }
        }
    }
}
