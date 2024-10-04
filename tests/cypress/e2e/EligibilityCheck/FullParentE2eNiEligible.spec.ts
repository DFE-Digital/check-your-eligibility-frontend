
describe('Parent with valid details can complete full Eligibility check and application', () => {

    it('Parent can make the full journey', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');

        cy.contains('Start Now').click()
        cy.url().should('include', '/Check/Enter_Details');

        cy.get('h1').should('include.text', 'Enter your details');

        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('Smith');
        cy.get('#Day').should('be.visible').type('01');
        cy.get('#Month').should('be.visible').type('01');
        cy.get('#Year').should('be.visible').type('1990');

        cy.get('#IsNinoSelected').click();

        cy.get('#NationalInsuranceNumber').should('be.visible').type('AB123456C');

        cy.contains('Save and continue').click();
        cy.url().should('include', '/Check/Loader');

        cy.get('h1').should('include.text', 'Your children are entitled to free school meals');



        const authorizationHeader: string= Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', 'https://signin.integration.account.gov.uk/**', (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');

        cy.contains('Continue to GOV.UK One Login',{ timeout: 60000 }).click();

        cy.origin('https://signin.integration.account.gov.uk', () => {
            let currentUrl = "";
            cy.url().then((url) => {
                currentUrl = url;
            });
            cy.wait(2000);

            cy.visit(currentUrl, {
                auth: {
                    username: Cypress.env('AUTH_USERNAME'),
                    password: Cypress.env('AUTH_PASSWORD')
                },
            });

            cy.contains('Sign in').click();

            cy.get('input[name=email]').type(Cypress.env('ONEGOV_EMAIL'));
            cy.contains('Continue').click();

            cy.get('input[name=password]').type(Cypress.env('ONEGOV_PASSWORD'));
            cy.contains('Continue').click();

        });

        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('h1').should('include.text', 'Provide details of your children');


        cy.get('[id="ChildList[0].FirstName"]').type('Timmy');
        cy.get('[id="ChildList[0].LastName"]').type('Smith');
        cy.get('[id="ChildList[0].School.Name"]').type('Hinde House 2-16 Academy');

        cy.get('#schoolList0')
            .should('be.visible')
            .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
            .click({ force: true})

        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');

        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 15000 }).should('contain.text', 'Check your answers before registering');

        cy.get('h2').should('contain.text', 'Parent or guardian details')
        cy.contains('dt', 'Parent or guardian name')
            .next('dd')
            .contains('Tim Smith');

        cy.contains('dt', 'Parent or guardian date of birth')
        .next('dd')
        .contains('01/01/1990');

        cy.contains('dt', 'National insurance number')
        .next('dd')
        .contains('AB123456C');

        cy.contains('dt', 'Email address')
        .next('dd')
        .contains(Cypress.env('ONEGOV_EMAIL'));


        cy.get('h2').should('contain.text', 'Child 1')
        cy.contains('dt', "Child's name")
            .next('dd')
            .contains('Timmy Smith');

        cy.contains('dt', 'School')
        .next('dd')
        .contains('Hinde House 2-16 Academy');

        cy.contains('dt', "Child's date of birth")
        .next('dd')
        .contains('01/01/2007');

        cy.contains('Submit application').click();

        cy.url().should('include', '/Check/Application_Sent');
        cy.get('h1').should('contain.text', 'Application complete');

        cy.get('.govuk-table__header').should('contain.text', 'Timmy Smith');
        
        cy.get('.govuk-table__cell').should('contain.text', 'Hinde House 2-16 Academy');


    });
});