
describe('Parents can enter childs details successfully', () => {

    it('Allows a parent to enter their childs details', () => {
        cy.visit('/Check/Enter_Child_Details');
        cy.get('h1').should('include.text', 'Provide details of your children');

        // cy.typeIntoInput(enterChildDetailsPage.getFieldSelector("Child One last name"), "Smith");
        cy.get('[id="ChildList[0].FirstName"]').type('Tim');
        cy.get('[id="ChildList[0].LastName"]').type('Smith');
        cy.get('[id="ChildList[0].School.Name"]').type('Hinde House 2-16 Academy');
        // cy.contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield').click();
        cy.get('#schoolList0')
            .should('be.visible')
            .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
            .click({ force: true})
            // .then(($el) => {
            //     cy.wrap($el).scrollIntoView().should('be.visible');
            //     cy.wrap($el).click({ force: true});
            // });
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');

        cy.contains('Save and continue').click();

        cy.contains('Tim Smith');
        cy.contains('Hinde House 2-16 Academy');
        cy.contains('01/01/2007')

        cy.get('h1').should('contain.text', 'Check your answers before registering');
        cy.contains('Register details').click();
    });

});