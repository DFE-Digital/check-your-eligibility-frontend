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
    const enterDetailsPage = new EnterDetailsPage();

    it('Parent can begin the journey from the start now page', () => {
        cy.visit('/');
        cy.verifyH1Text('Check if your children can get free school meals');
        startNowPage.clickStartNowBtn();
        cy.url().should('include', '/Check/Enter_Details');
    });

    it('Parent can enter their details successfully', () => {
        cy.visit('/Check/Enter_Details');
        cy.verifyH1Text('Enter your details');
        cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
        cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
        cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
        cy.selectYesNoOption(enterDetailsPage.getRadioSelector(),false); // fix false issue
        cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB123456C");
        cy.clickButton('Save and continue');
        cy.url().should('include', '/Check/Loader');
        cy.verifyH1Text('Your children are entitled to free school meals');
    })

    it('Parents can go to OneGov and sign in', () => {
        cy.visit('/Check/Loader');
        cy.verifyH1Text('Your children are entitled to free school meals');
    })
})