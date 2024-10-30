
describe('After errors have been input initially a Parent with valid details can complete full Eligibility check and application', () => {

    it('Will show the correct validation errors when user leaves the fields blank', () => {

        cy.visit('/Check/Enter_Details');
        cy.get('h1').should('contain.text', 'Enter your details');

        cy.contains('Save and continue').click();

        cy.get('h2').should('contain.text', 'There is a problem');

        cy.get('li').should('contain.text', 'Enter a first name');
        cy.get('li').should('contain.text', 'Enter a last name');
        cy.get('li').should('contain.text', 'Enter a date of birth');
    });

    it('returns the correct error message when invalid charaters are used in the first name field', () => {
        cy.visit('/Check/Enter_Details');
        cy.get('#FirstName').should('be.visible').type('123456');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Enter a first name with valid characters');
    });

    it('returns the correct error message when invalid charaters are used in the last name field', () => {
        cy.visit('/Check/Enter_Details');
        cy.get('#LastName').should('be.visible').type('123456');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Enter a last name with valid characters');
    });

    it('returns the correct error message when invalid dates are added to the date fields', () => {
        cy.visit('/Check/Enter_Details');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('32');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('32');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('4001');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Enter a valid day');

        cy.get('#DateOfBirth\\.Day').clear().type('01');
        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Enter a valid month');

        cy.get('#DateOfBirth\\.Month').clear().type('01');
        cy.contains('Save and continue').click();
        cy.get('li').should('contain.text', 'Enter a date in the past');

    });

    it('returns the correct error message when letters are used instead of numbers in the date field', () => {
        cy.visit('/Check/Enter_Details');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('ff');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('ff');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('ff');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Enter a date of birth using numbers only');

    })


    it('returns the correct error message when invalid characters are used in the input fields', () => {

        cy.visit('/Check/Enter_Child_Details');

        cy.get('[id="ChildList[0].FirstName"]').type('123456');
        cy.get('[id="ChildList[0].LastName"]').type('123456');
        cy.get('[id="school-search-0"]').clear();

        cy.get('[id="ChildList[0].Day"]').clear().type('32');
        cy.get('[id="ChildList[0].Month"]').clear().type('13');
        cy.get('[id="ChildList[0].Year"]').clear().type('2050');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Enter a first name with valid characters');
        cy.get('li').should('contain.text', 'Enter a last name with valid characters');
        cy.get('li').should('contain.text', 'Enter a valid day');

        cy.get('[id="ChildList[0].Day"]').clear().type('01');
        cy.contains('Save and continue').click();

        cy.get('[id="ChildList[0].Month"]').clear().type('01');
        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Enter a date in the past');
    });


    it('Parent can make the full journey using correct details after correcting issues in child details', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');

        cy.contains('Start Now').click()
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Enter your details');

        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('Smith');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1990');

        cy.get('#IsNinoSelected').click();

        cy.get('#NationalInsuranceNumber').should('be.visible').type('NN668767B');

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

            cy.contains('Sign in',{ timeout: 60000 }).click();

            cy.get('input[name=email]').type(Cypress.env('ONEGOV_EMAIL'));
            cy.contains('Continue').click();

            cy.get('input[name=password]').type(Cypress.env('ONEGOV_PASSWORD'));
            cy.contains('Continue').click();

        });

        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('h1').should('include.text', 'Provide details of your children');

        // Blank fields
        cy.contains('Save and continue').click();

        cy.get('h2').should('contain.text', 'There is a problem');

        cy.get('li').should('contain.text', "Enter child's first name");
        cy.get('li').should('contain.text', "Enter child's last name");
        cy.get('li').should('contain.text', 'School is required');
        cy.get('li').should('contain.text', 'Enter a date of birth');


        cy.get('[id="ChildList[0].FirstName"]').type('Timmy');
        cy.get('[id="ChildList[0].LastName"]').type('Smith');
        cy.get('[id="school-search-0"]').type('Hinde House 2-16 Academy');

        cy.get('#schoolList0', {timeout: 5000})
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

        cy.contains('dt', 'National Insurance number')
        .next('dd')
        .contains('NN668767B');

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