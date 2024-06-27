class CheckYourAnswersPage {
    parentName = ':nth-child(2) > .govuk-summary-card__content > .govuk-summary-list > :nth-child(3) > .govuk-summary-list__value';
    parentDOB = '[id="ChildList[0].School.Name"]';
    parentNI = '[id="ChildList[0].Day"]';

    public getFieldSelector(fieldName: string): string {
        switch (fieldName) {
            case "parent Name":
                return this.parentName;
            case "Parent DOB":
                return this.parentDOB;
            case "Parent NI":
                return this.parentNI;
            default:
                throw new Error(`Field name '${fieldName}' not recognized`);
        }
    }

    public getRadioSelector(): string {
        return '[name="IsNassSelected"]';
    }
}



export default CheckYourAnswersPage;
