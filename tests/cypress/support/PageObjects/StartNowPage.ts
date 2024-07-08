// cypress/pageObjects/StartNowPage.ts
class StartNowPage {
  elements = {
    startNowButton: () => cy.contains('Start Now')

  }
    clickStartNowBtn() {
      this.elements.startNowButton().click();
    }
  }
  
  export default StartNowPage;
  