// cypress/support/commands.ts
declare namespace Cypress {
    interface Chainable 
    {
        clickButton(buttonText: string): Chainable<Element>
        clickButtonByText(buttonText: string): Chainable<Element>
        typeTextByLabel(labelText: string, text: string): Chainable<Element>
    }
}

Cypress.Commands.add('clickButton', (buttonText: string) => {
    cy.contains('button', buttonText).click();
});


Cypress.Commands.add('clickButtonByText', (buttonText: string) => {
    cy.contains('a[role="button"]', buttonText.trim()).click();
});

// Cypress.Commands.add('typeTextByLabel', (labelText: string, text: string) => {
//     cy.contains('label', labelText).invoke('attr', 'for').then((id) => {
//         cy.get(`#${id}`).clear().type(text);
//     });
// });

Cypress.Commands.add('typeTextByLabel', (labelText: string, text: string) => {
    cy.contains('label', labelText)
      .parent() // Move to the parent element of the label
      .find('input') // Find the input element within that parent
      //.clear() // Clear any existing text
      .type(text); // Type the new text
});