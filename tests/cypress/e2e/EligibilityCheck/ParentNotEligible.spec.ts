
describe('Parents journey when not eligible', () => {

    it('Will not return the correct responses if the Parent is not eligible for free school meals', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start Now').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Jones');

        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.get('#IsNassSelected').click();
        cy.get('#NationalInsuranceNumber').type('AB123456C');

        cy.contains('Save and continue').click();

        cy.url().should('include', '/Check/Loader');
        cy.wait(2000);
        cy.get('h1').should('include.text', 'Your children may not be entitled to free school meals');
    });

    it('Will return the correct response if we cannot find the user', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start Now').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Stevens');

        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.get('#IsNassSelected').click();
        cy.get('#NationalInsuranceNumber').type('AB123456C');

        cy.contains('Save and continue').click();

        cy.wait(2000);
        cy.url().should('include', '/Check/Loader');
        cy.get('h1').should('include.text', "We could not check your children’s entitlement to free school meals",{ timeout: 60000 });
    });

    it('Will return the correct error response if the user inputs a NI number in the incorrect format', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start Now').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Smith');

        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.get('#IsNassSelected').click();
        cy.get('#NationalInsuranceNumber').type('ABCDE456C');

        cy.contains('Save and continue').click();

        cy.get('h2').should('include.text', 'There is a problem');
        cy.get('a').should('include.text', 'Invalid National Insurance Number format');
    });

    it('Will return the correct error response if the user inputs a NI number is too long', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start Now').click();

        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Smith');

        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.get('#IsNassSelected').click();
        cy.get('#NationalInsuranceNumber').type('0123456789');

        cy.contains('Save and continue').click();

        cy.get('h2').should('include.text', 'There is a problem');
        cy.get('a').should('include.text', 'National Insurance Number should contain no more than 9 alphanumeric characters');
    });

    it('Allows a user to enter correct NI number after entering an incorrect one', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start Now').click();

        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Smith');

        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.get('#IsNassSelected').click();
        cy.get('#NationalInsuranceNumber').type('0123456789');

        cy.contains('Save and continue').click();

        cy.get('h2').should('include.text', 'There is a problem');
        cy.get('a').should('include.text', 'National Insurance Number should contain no more than 9 alphanumeric characters');

        cy.get('#NationalInsuranceNumber').clear().type('AB123456C');
        cy.contains('Save and continue').click();
        cy.url().should('include','/Check/Loader');

    })

});