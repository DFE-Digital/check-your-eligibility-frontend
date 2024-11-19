describe("Links on not eligible page route to the intended locations", () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B'

    beforeEach(() => {
        cy.session("Session 1", () => {
            cy.SignInSchool();
            cy.wait(1);
        });
    });

    it("Guidance link should route to guidance page", () => {
        cy.visit("/Check/Enter_Details");
            cy.get('#FirstName').type(parentFirstName);
            cy.get('#LastName').type(parentLastName);
            cy.get('#Day').type('01');
            cy.get('#Month').type('01');
            cy.get('#Year').type('1990');
            cy.get('#NinAsrSelection').click();
            cy.get('#NationalInsuranceNumber').type(NIN);
            cy.get('#EmailAddress').clear().type(parentEmailAddress);
            cy.contains('Perform check').click();
            cy.contains('a.govuk-link', 'See a complete list of acceptable evidence', { timeout: 8000 }).then(($link) => {
                
                const url = $link.prop('href');
                cy.visit(url); 
                cy.get('h1.govuk-heading-l').should('contain.text', 'Guidance for reviewing evidence'); 
              });
    });

    it("Support link should route to DfE form", () => {
        cy.visit("/Check/Enter_Details");
            cy.get('#FirstName').type(parentFirstName);
            cy.get('#LastName').type(parentLastName);
            cy.get('#Day').type('01');
            cy.get('#Month').type('01');
            cy.get('#Year').type('1990');
            cy.get('#NinAsrSelection').click();
            cy.get('#NationalInsuranceNumber').type(NIN);
            cy.get('#EmailAddress').clear().type(parentEmailAddress);
            cy.contains('Perform check').click();
            cy.get('a' , {timeout: 80000}).contains("ecs.admin@education.gov.uk");
    });
});
