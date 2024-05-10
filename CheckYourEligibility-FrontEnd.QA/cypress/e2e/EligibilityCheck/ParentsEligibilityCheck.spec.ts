import {StartNowPage}  from '../../support/PageObjects';

describe('Parent with NI number can submit successful application - without login', () => {
    it('Complete Parent Eligibility Check using NI', () => {
      const startNow = new StartNowPage();   
        cy.visit('/')
        cy.clickButtonByText('Start Now');
        cy.typeTextByLabel("Parent's first name", 'John');
        cy.typeTextByLabel("Parent's last name", 'Doe');
        //Enter Date
        cy.typeTextByLabel("Day", '01');
        cy.typeTextByLabel("Month", '01');
        cy.wait(5000);
        cy.typeTextByLabel("Year", '2010');
    });
  });