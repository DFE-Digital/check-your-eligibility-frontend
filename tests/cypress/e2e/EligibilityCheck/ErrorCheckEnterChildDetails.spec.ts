describe('Ensure correct error messages are shown on the Enter Child Details page', () => {

    it('will show the correct error message if fields are left blank', () => {

        cy.visit('/Check/Enter_Child_Details');
        cy.get('h1').should('contain.text', 'Provide details of your children');

        cy.contains('Save and continue').click();

        cy.get('h2').should('contain.text', 'There is a problem');

        cy.get('li').should('contain.text', 'First name is required');
        cy.get('li').should('contain.text', 'Last name is required');
        cy.get('li').should('contain.text', 'School is required');
        cy.get('li').should('contain.text', 'Day is required');
        cy.get('li').should('contain.text', 'Month is required');
        cy.get('li').should('contain.text', 'Year is required');

    });
});

describe('Ensure correct error messages are shown when individual details are incorrect or invalid', () => {
    beforeEach(() => {
        cy.visit('/Check/Enter_Child_Details');
        cy.get('h1').should('include.text', 'Provide details of your children');


        cy.get('[id="ChildList[0].FirstName"]').type('Tim');
        cy.get('[id="ChildList[0].LastName"]').type('Smith');
        cy.get('[id="ChildList[0].School.Name"]').type('Hinde House 2-16 Academy');

        cy.get('#schoolList0')
            .should('be.visible')
            .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
            .click({ force: true})

        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
    });

    it('returns the correct error message when invalid charaters are used in the first name field', () => {

        cy.get('[id="ChildList[0].FirstName"]').clear().type('123456');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'First Name field contains an invalid character');
    });

    it('returns the correct error message when invalid charaters are used in the last name field', () => {

        cy.get('[id="ChildList[0].LastName"]').type('123456');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Last Name field contains an invalid character');
    });

    it('returns the correct error message when invalid dates are added to the date fields', () => {

        cy.get('[id="ChildList[0].Day"]').clear().type('32');
        cy.get('[id="ChildList[0].Month"]').clear().type('13');
        cy.get('[id="ChildList[0].Year"]').clear().type('2050');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Invalid date entered');
        cy.get('li').should('contain.text', 'Invalid Day');
        cy.get('li').should('contain.text', 'Invalid Month');
        cy.get('li').should('contain.text', 'Invalid Year');

    });



})