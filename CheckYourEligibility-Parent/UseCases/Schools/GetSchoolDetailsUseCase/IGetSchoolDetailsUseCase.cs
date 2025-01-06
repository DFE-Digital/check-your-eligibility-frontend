namespace CheckYourEligibility_FrontEnd.UseCases.Schools.GetSchoolDetailsUseCase
{
    
    public interface IGetSchoolDetailsUseCase
    {
        Task<GetSchoolDetailsResponse> ExecuteAsync(GetSchoolDetailsRequest request);
    }
}
