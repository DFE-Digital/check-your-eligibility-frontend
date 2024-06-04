// cypress/pageObjects/StartNowPage.ts
class StartNowPage {
  elements = {
    

  }
    clickStartNowBtn() {
      cy.contains('button', 'Start now').click();
    }
  }
  
  export default StartNowPage;
  