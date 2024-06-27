using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility_FrontEnd.Attributes
{
    public class DobAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance;

            if (value == null || value == "")
            {
                return ValidationResult.Success;
            }
            else
            {
                DateOnly dob;
                var day = model.GetType().GetProperty("Day").GetValue(model);
                var month = model.GetType().GetProperty("Month").GetValue(model);
                var year = model.GetType().GetProperty("Year").GetValue(model);

                var desiredDateFormat = $"{year}-{month}-{day}";
                DateOnly dobAsDate;
                DateOnly.TryParse(desiredDateFormat, out dobAsDate);

                if (dobAsDate > DateOnly.FromDateTime(DateTime.Now))
                {
                    return new ValidationResult("Date of Birth cannot be a future date");
                }

                var dobString = $"{year}-{month}-{day}";
                if (DateOnly.TryParse(dobString, out dob))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Invalid date entered");
                }
            }
        }
    }
}
