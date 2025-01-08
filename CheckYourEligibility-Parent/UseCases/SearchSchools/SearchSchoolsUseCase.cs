﻿using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility_FrontEnd.Services;

namespace CheckYourEligibility_FrontEnd.UseCases.SearchSchools
{
    public interface ISearchSchoolsUseCase
    {
        Task<IEnumerable<Establishment>> ExecuteAsync(string query);
    }

    public class SearchSchoolsUseCase : ISearchSchoolsUseCase
    {
        private readonly IEcsServiceParent _parentService;

        public SearchSchoolsUseCase(IEcsServiceParent parentService)
        {
            _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<IEnumerable<Establishment>> ExecuteAsync(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 3)
            {
                throw new ArgumentException("Query must be at least 3 characters long.", nameof(query));
            }

            var results = await _parentService.GetSchool(query);
            return results?.Data ?? new List<Establishment>();
        }
    }
}