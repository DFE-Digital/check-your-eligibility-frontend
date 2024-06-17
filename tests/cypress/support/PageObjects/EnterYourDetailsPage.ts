
class EnterYourDetailsPage {
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

  // Method to type into a field given the field name and text
  public enterParentsInfo(fieldName: string, text: string): void {
    let selector = this.getFieldSelector(fieldName);  // Get the correct selector
    cy.typeIntoInput(selector, text)
  }

  public verifyFieldVisibility(fieldName: string, IsVisible: boolean): void {
    let selector = this.getFieldSelector(fieldName);  // Get the correct selector
    cy.verifyFieldVisibility(selector, IsVisible)
  }

  public enterDateOfBirth(day: string, month: string, year: string): void {
    cy.enterDate(this.daySelector, this.monthSelector, this.yearSelector, day, month, year);
  }

  // Method to select a 'Yes' or 'No' radio button option based on the choice
  public selectYesNoOption(choice: 'Yes' | 'No'): void {
    const selector = choice === 'Yes' ? this.doYouHaveNiRadioYesSelector : this.doYouHaveNiRadioNoSelector;
   // cy.selectRadioButton(selector);
  }
}

export default EnterYourDetailsPage;
