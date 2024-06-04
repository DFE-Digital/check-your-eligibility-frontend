// cypress/support/PageObjects/EnterYourDetailsPage.ts

class EnterDetailsPage {
    firstNameInputField = "#FirstName";
    lastNameInputField = "#LastName";
    daySelector = '#Day';
    monthSelector = '#Month';
    yearSelector = '#Year';
    doYouHaveNiRadioYesSelector = '[name="IsNassSelected"][value="false"]';
    doYouHaveNiRadioNoSelector = '[name="IsNassSelected"][value="true"]';
    nationalInsuranceFieldSelector = '#NationalInsuranceNumber';

    public getFieldSelector(fieldName: string): string {
        switch (fieldName) {
            case "Parent's first name":
                return this.firstNameInputField;
            case "Parent's last name":
                return this.lastNameInputField;
            case "Parent's National Insurance number":
                return this.nationalInsuranceFieldSelector;
            default:
                throw new Error(`Field name '${fieldName}' not recognized`);
        }
    }

    public getRadioSelector(): string {
        return '[name="IsNassSelected"]';
    }
}



export default EnterDetailsPage;
