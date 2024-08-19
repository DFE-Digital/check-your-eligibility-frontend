
// Custom commands

Cypress.Commands.add('SignIn', () => {
  cy.visit('/');
  cy.get('h1').should('include.text', 'Department for Education Sign-in');
  cy.get('#username').type(Cypress.env('DFE_ADMIN_EMAIL_ADDRESS'));
  cy.contains('Continue').click();

  cy.get('#password').type(Cypress.env('DFE_ADMIN_PASSWORD'));
  cy.contains('Sign in').click();

  cy.get('h1').should('include.text', 'Telford and Wrekin Council');
})

Cypress.Commands.add('typeTextByLabel', (labelText: string, text: string) => {
  cy.contains('label', labelText)
    .parent()
    .find('input')
    .type(text);
});


Cypress.Commands.add('typeIntoInput', (selector: string, text: string) => {
  cy.get(selector).type(text);
});

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

Cypress.Commands.add('waitForElementToDisappear', (selector: string) => {
  cy.get('body').then($body => {
    if ($body.find(selector).length > 0) {
      cy.get(selector, { timeout: 30000 }).should('not.exist');
    }
  });
});

