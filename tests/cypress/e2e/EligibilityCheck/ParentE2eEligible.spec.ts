import DoYouHaveNassNumberPage from '../../support/PageObjects/DoYouHaveNassNumberPage';
import EnterDetailsPage from '../../support/PageObjects/EnterDetailsPage'
import  StartNowPage from '../../support/PageObjects/StartNowPage';
import EnterChildDetailsPage from '../../support/PageObjects/EnterChildDetailsPage'
import { authenticator } from 'otplib';

// const startNowButton = new StartNowPage();
// const doYouHaveNassNumPage = new DoYouHaveNassNumberPage();
// const enterDetailsPage = new EnterDetailsPage();


describe('Parent with valid details can complete full Eligibility check and application', () => {
    const startNowPage = new StartNowPage();

    it('Can begin the journey from the start now page', () => {
        cy.visit('/')
        cy.verifyH1Text('Check if your children can get free school meals')
        startNowPage.clickStartNowBtn();
        cy.url().should('include', '/Check/Enter_Details')
        cy.verifyH1Text('Enter your details')
    })
})