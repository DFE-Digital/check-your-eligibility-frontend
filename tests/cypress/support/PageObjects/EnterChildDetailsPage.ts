class EnterChildDetailsPage {
    childOnefirstNameInputField = '[id="ChildList[0].FirstName"]';
    childOnelastNameInputField = '[id="ChildList[0].LastName"]';
    childOneSchoolNameField = '[id="ChildList[0].School.Name"]';
    childOnedaySelector = '[id="ChildList[0].Day"]';
    childOnemonthSelector = '[id="ChildList[0].Month"]';
    childOneyearSelector = '[id="ChildList[0].Year"]';


    public getFieldSelector(fieldName: string): string {
        switch (fieldName) {
            case "Child One first name":
                return this.childOnefirstNameInputField;
            case "Child One last name":
                return this.childOnelastNameInputField;
            case "Child One school name":
                return this.childOneSchoolNameField;
            case "Child One School":
                return this.childOneSchoolNameField;
            default:
                throw new Error(`Field name '${fieldName}' not recognized`);
        }
    }

    public getRadioSelector(): string {
        return '[name="IsNassSelected"]';
    }
}



export default EnterChildDetailsPage;
