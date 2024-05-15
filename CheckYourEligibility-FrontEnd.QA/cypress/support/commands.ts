// cypress/support/commands.ts
declare namespace Cypress {
    interface Chainable {
        clickButton(buttonText: string): Chainable<Element>;
        clickButtonByRole(buttonText: string): Chainable<Element>;
        clickButtonByText(buttonText: string): Chainable<Element>;
        typeTextByLabel(labelText: string, text: string): Chainable<Element>
        typeIntoInput(selector: string, text: string): Chainable<any>;
        enterDate(daySelector: string, monthSelector: string, yearSelector: string, day: string, month: string, year: string): Chainable<any>;
        selectRadioButton(selector: string): Chainable<Element>;
    }
}

Cypress.Commands.add('clickButtonByRole', (buttonText: string) => {
    cy.get('[role="button"]').contains(buttonText).click()
})

Cypress.Commands.add('clickButton', (buttonText: string) => {
    cy.get('button').contains(buttonText).click()
});



Cypress.Commands.add('typeTextByLabel', (labelText: string, text: string) => {
    cy.contains('label', labelText)
        .parent() // Move to the parent element of the label
        .find('input') // Find the input element within that parent
        //.clear() // Clear any existing text
        .type(text); // Type the new text
});

Cypress.Commands.add('typeIntoInput', (selector: string, text: string) => {
    cy.get(selector).clear().type(text);
});

Cypress.Commands.add('enterDate', (daySelector: string, monthSelector: string, yearSelector: string, day: string, month: string, year: string) => {
    cy.get(daySelector).clear().type(day);
    cy.get(monthSelector).clear().type(month);
    cy.get(yearSelector).clear().type(year);
});

Cypress.Commands.add('selectRadioButton', (selector: string) => {
    cy.get(selector).click();
  });