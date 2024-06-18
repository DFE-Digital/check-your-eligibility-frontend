
class DoYouHaveNassNumberPage {
  
    doYouHaveNassRadioYesSelector = '[name="IsNassSelected"][value="true"]';  
    doYouHaveNassRadioNoSelector = '[name="IsNassSelected"][value="false"]';
    nassNumberInputField =  'NationalAsylumSeekerServiceNumber'


    public getFieldSelector(fieldName: string): string {
        switch (fieldName) {
            case "NASS Number":
                return this.doYouHaveNassRadioNoSelector;
            default:
                throw new Error(`Field name '${fieldName}' not recognized`);
        }
    }
    public getSelector(): string {
        return '[id="IsNassSelected"]]';
    }
    
    public (isYes: boolean): string {
        return isYes ? this.doYouHaveNassRadioYesSelector : this.doYouHaveNassRadioNoSelector;
      }
    }

    

  
  
  export default DoYouHaveNassNumberPage;
  
