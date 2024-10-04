
// Custom commands

Cypress.Commands.add('SignInLA', () => {
  cy.visit('/');
  cy.get('#username').type(Cypress.env('DFE_ADMIN_EMAIL_ADDRESS'));
  cy.get('button[type="submit"]').click()

  cy.get('#password').type(Cypress.env('DFE_ADMIN_PASSWORD'));
  cy.get('button[type="submit"]').click()

  cy.contains('Telford and Wrekin Council')
  .parent()
  .find('input[type="radio"]')
  .check();


  cy.contains('Continue',{ timeout: 15000 }).click();

  cy.get('h1').should('include.text', 'Telford and Wrekin Council');
});

Cypress.Commands.add('SignInSchool', () => {
  cy.visit('/');
  cy.get('#username').type(Cypress.env('DFE_ADMIN_EMAIL_ADDRESS'));
  cy.get('button[type="submit"]').click()

  cy.get('#password').type(Cypress.env('DFE_ADMIN_PASSWORD'));
  cy.get('button[type="submit"]').click()

  cy.contains('The Telford Park School')
    .parent()
    .find('input[type="radio"]')
    .check();

  cy.contains('Continue').click();

  cy.get('h1').should('include.text', 'The Telford Park School');
});

Cypress.Commands.add('CheckValuesInSummaryCard', (key: string, expectedValue: string) => {
  cy.get('.govuk-summary-list__row').contains('.govuk-summary-list__key', key )
    .siblings('.govuk-summary-list__value')
    .should('include.text', expectedValue)
});

Cypress.Commands.add('scanPagesForValue', (value: string) => {
  cy.get('body').then((body) => {
    if (body.find(`td:contains("${value}")`).length > 0) {
      cy.get(`td:contains("${value}")`).click();
    }
    else {
      cy.contains('.govuk-link', 'Next').click();
      cy.scanPagesForValue(value);
    }
  });
});


Cypress.Commands.add('findApplicationFinalise', (value: string) => {
  let referenceFound = false;
  function searchOnPage() {
    cy.get('.govuk-table tbody tr').each(($row) => {
      cy.wrap($row).find('td').eq(1).invoke('text').then((text) => {
          if (text.trim() === value) {
              referenceFound = true;
              cy.wrap($row).find('td').eq(0).find('input[type="checkbox"]').click();
              cy.log('found it!');
              return false;
          }
      });
    }).then(() => {
      if (!referenceFound){
        cy.get('body').then((body) => {
          if(body.find('.govuk-link').length > 0) {
            cy.contains('.govuk-link', 'Next').click();
            cy.findApplicationFinalise(value);
          }
          else{
            cy.log('Reference number not found')
          }
        });
      }
    });
  }
  searchOnPage();
})


Cypress.Commands.add('verifyFieldVisibility', (selector: string, isVisible: boolean) => {
  if (isVisible) {
    cy.get(selector).should('be.visible');
  } else {
    cy.get(selector).should('not.be.visible');
  }
});


Cypress.Commands.add('verifyH1Text', (expectedText: string) => {
  cy.contains('h1', expectedText).should('be.visible');
  cy.get('h1').invoke('text').then((actualText: string) => {
    expect(actualText.trim()).to.eq(expectedText); 
  });
});

Cypress.Commands.add('selectYesNoOption', (baseSelector: string, isYes: boolean) => {
  const finalSelector = isYes ? `${baseSelector}[value="true"]` : `${baseSelector}[value="false"]`;
  cy.log(`selector being used: ${finalSelector}`)
  cy.get(finalSelector).click();
});

Cypress.Commands.add('retainAuthOnRedirect', (initialUrl, authHeader, alias) => {
  let redirectUrl: string;

  cy.intercept(initialUrl, (req) => {
    req.continue((res) => {
      const locationHeader = res.headers['location'];
      if (Array.isArray(locationHeader)) {
        redirectUrl = locationHeader[0];
      } else {
        redirectUrl = locationHeader;
      }
    });
  }).as('initialRequest');

  cy.request({
    url: initialUrl,
    headers: {
      'Authorization': authHeader,
    },
    followRedirect: false,
  }).then(() => {
    expect(redirectUrl).to.exist;

    cy.request({
      url: redirectUrl,
      headers: {
        'Authorization': authHeader,
      }
    }).as(alias);
  });
});


