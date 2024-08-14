describe('Check free school meals - single student elligible', () => {
    beforeEach(() => {
        cy.SignIn();
    })

    it('Allows the LA user to check free school meal for a single student successfully using NI number ', () => {
        cy.contains('Check free school meals - single student').click();

        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Smith');
        cy.get('#EmailAddress').type('Tim@Smith.com');
        cy.get('#NationalInsuranceNumber').type('AB123456C');

        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.contains('Perform check').click();
        cy.url().should('include', '/Check/Loader');

        cy.get('h1').should('contain.text', 'The children of this parent or guardian are entitled to free school meals');

    });

    it('Allows the LA user to check free school meal for a single student successfully using Nass number ', () => {
        cy.contains('Check free school meals - single student').click();

        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Smith');
        cy.get('#EmailAddress').type('Tim@Smith.com');
        cy.get('#NationalAsylumSeekerServiceNumber').type('240767899');

        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.contains('Perform check').click();
        cy.url().should('include', '/Check/Loader');

        cy.get('h1').should('contain.text', 'The children of this parent or guardian are entitled to free school meals',{ timeout: 10000 });

    });
})