// cypress/e2e/enterYourDetails.cy.ts

import EnterYourDetailsPage from '../../support/PageObjects/EnterYourDetailsPage';
import DoYouHaveNassNumberPage from '../../support/PageObjects/DoYouHaveNassNumberPage';
import EnterDetailsPage from '../../support/PageObjects/EnterDetailsPage'

describe('Parent with valid details can carry out Eligibility Check', () => {
  const enterYourDetailsPage = new EnterYourDetailsPage();
  const doYouHaveNassNumPage = new DoYouHaveNassNumberPage();
  const enterDetailsPage = new EnterDetailsPage();


  it.only('Complete Parent Eligibility Check using NI number', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals');
    cy.clickButtonByRole('Start Now');
    cy.get('h1').should('have.text', 'Enter your details');
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), true);
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB123456C");
    cy.clickButton('Save and continue');
    cy.get('h1').should('have.text', 'Your children are entitled to free school meals');

    const authorizationHeader = 'Basic aW50ZWdyYXRpb24tdXNlcjp3aW50ZXIyMDIx';

    // Function to handle redirect requests
    function handleRedirect(url: string, headers: { Authorization: string }): Cypress.Chainable<Cypress.Response<any>> {
      return cy.request({
        url,
        followRedirect: false,
        headers
      }).then((response) => {
        const redirectUrl = response.redirectedToUrl;
        if (redirectUrl) {
          return handleRedirect(redirectUrl, headers);
        }
        return cy.wrap(response);
      });
    }

    cy.intercept('GET', "https://signin.integration.account.gov.uk/**", (req) => {
      req.headers['Authorization'] = authorizationHeader
    }).as('intercept for GET');
    
    cy.contains("Go to OneGov").click();

    cy.origin('https://signin.integration.account.gov.uk/*',
        () => {
          cy.wait(2000);
          
          cy.visit('https://signin.integration.account.gov.uk/sign-in-or-create', {
            auth: {
              username: 'integration-user',
              password: 'winter2021',
            },
          })
      
          cy.contains("Sign in").click();
          
          cy.get("input[name=email]").type("marten.wetterberg@madetech.com");
          
          cy.contains("Continue").click();
        }
    );
    /*
    
        .invoke('attr', 'href')
        .then(href => {
          cy.request({
            url: href,
            followRedirect: false,
            headers: {
              Authorization: authorizationHeader
            }
          }).then((initialResponse) => {
            const redirectUrl = initialResponse.redirectedToUrl;
            if (redirectUrl) {
              
              
              cy.visit(redirectUrl);
              
              cy.origin('https://signin.integration.account.gov.uk/sign-in-or-create', {
                args: {
                  redirectUrl,
                  authorizationHeader
                }
              }, ({redirectUrl, authorizationHeader}) => {
                cy.contains("Sign in").click();
              });
            }
          });
        });*/

    /*cy.request({
      url: 'https://oidc.integration.account.gov.uk/authorize?ui_locales=en&response_type=code&scope=openid,email&client_id=pBvq8IcdKosgOKzyQ6szmnS0_Yw&state=dolkfkfkfkflooh&nonce=qwsrkiseyullllio&redirect_uri=https://ecs-test-as-frontend.azurewebsites.net/Check/Enter_Child_Details',
      followRedirect: false,
      headers: {
        Authorization: authorizationHeader
      }
    }).then((initialResponse) => {
      const redirectUrl = initialResponse.redirectedToUrl;
      if (redirectUrl) {
        handleRedirect(redirectUrl, { Authorization: authorizationHeader }).then((finalResponse) => {
          expect(finalResponse.status).to.eq(200);
          const finalUrl = finalResponse.redirectedToUrl;
          if (finalUrl) {
            cy.visit(finalUrl);
          }
        });
      } else {
        expect(initialResponse.status).to.eq(200);
        const initialUrl = initialResponse.redirectedToUrl;
        if (initialUrl) {
          cy.visit(initialUrl);
        }
      }
    });

    //cy.wait(4000);
    cy.visit('https://signin.integration.account.gov.uk/sign-in-or-create', { auth: { username: 'integration-user', password: 'winter2021' } });

    cy.origin('https://signin.integration.account.gov.uk', () => {
      
      cy.get('#sign-in-button').click();
    })*/

  });




  it('Complete Parent Eligibility Check using Nass number', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals')
    cy.clickButtonByRole('Start Now');
    cy.get('h1'.trim()).should('have.text', 'Enter your details')
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Johnson");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), false);
    cy.clickButton('Save and continue');
    // cy.get('h1'.trim()).should('have.text', 'Your children are entitled to free school meals')
    cy.selectYesNoOption(doYouHaveNassNumPage.getSelector(), false);
    //  cy.typeIntoInput(doYouHaveNassNumPage.getFieldSelector("NASS Number"), "123456789")
    cy.clickButton('Save and continue');
    // cy.get('h1').should('have.text', 'Your children are entitled to free school meals')
    cy.get('.govuk-button').click

  });

});


