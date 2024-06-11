// cypress/support/commands.ts
declare namespace Cypress {
    interface Chainable {
        clickButtonByText(buttonText: string): Chainable<Element>;
        typeTextByLabel(labelText: string, text: string): Chainable<Element>


        typeIntoInput(selector: string, text: string): Chainable<void>;
        verifyFieldVisibility(selector: string, isVisible: boolean): Chainable<void>;
        enterDate(daySelector: string, monthSelector: string, yearSelector: string, day: string, month: string, year: string): Chainable<void>;
        clickButtonByRole(role: string): Chainable<void>;
        clickButton(text: string): Chainable<void>;
        verifyH1Text(expectedText: string): Chainable<void>;
        selectYesNoOption(selector: string, isYes: boolean): Chainable<Element>;
        retainAuthOnRedirect(initialUrl: string, authHeader: string, alias: string): Chainable<void>;
    
    }
}

Cypress.Commands.add('typeTextByLabel', (labelText: string, text: string) => {
    cy.contains('label', labelText)
        .parent() // Move to the parent element of the label
        .find('input') // Find the input element within that parent
        //.clear() // Clear any existing text
        .type(text); // Type the new text
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

Cypress.Commands.add('enterDate', (daySelector: string, monthSelector: string, yearSelector: string, day: string, month: string, year: string) => {
    cy.get(daySelector).clear().type(day);
    cy.get(monthSelector).clear().type(month);
    cy.get(yearSelector).clear().type(year);
});


Cypress.Commands.add('clickButtonByRole', (role: string) => {
    cy.contains(role).click();
});

Cypress.Commands.add('clickButton', (text: string) => {
    cy.contains('button', text).click();
});

Cypress.Commands.add('verifyH1Text', (expectedText: string) => {
    cy.get('h1').invoke('text').then((actualText: string) => {
        expect(actualText.trim()).to.eq(expectedText);
    });
});

Cypress.Commands.add('selectYesNoOption', (baseSelector: string, isYes: boolean) => {
    const finalSelector = isYes ? `${baseSelector}[value="false"]` : `${baseSelector}[value="true"]`;
    cy.get(finalSelector).click();
});


Cypress.Commands.add('retainAuthOnRedirect', (initialUrl, authHeader, alias) => {
    let redirectUrl: string;
  
    // Intercept the initial request
    cy.intercept(initialUrl, (req) => {
      req.continue((res) => {
        // Capture the redirect location and ensure it is a string
        const locationHeader = res.headers['location'];
        if (Array.isArray(locationHeader)) {
          redirectUrl = locationHeader[0];
        } else {
          redirectUrl = locationHeader;
        }
      });
    }).as('initialRequest');
  
    // Make the initial request
    cy.request({
      url: initialUrl,
      headers: {
        'Authorization': authHeader,
      },
      followRedirect: false,
    }).then(() => {
      // Ensure the redirect URL is captured and is a string
      expect(redirectUrl).to.exist;
  
      // Make the redirected request with the authorization header
      cy.request({
        url: redirectUrl,
        headers: {
          'Authorization': authHeader,
        }
      }).as(alias);
    });
  });


  