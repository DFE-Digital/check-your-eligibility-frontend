describe("email addresses provide the user with an error if they are invalid" , () => {
    const parentFirstName = 'Tim';
    const parentLastName = 'Jones';
    const NIN = 'PN668767B'

    const invalidEmailAddresses = [
        'email@123.123.123.123',
        'email@[123.123.123.123]',
        'plainaddress',
        '@no-local-part.com',
        'Outlook Contact <outlook-contact@domain.com>',
        'no-at.domain.com',
        'no-tld@domain',
        ';beginning-semicolon@domain.co.uk',
        'middle-semicolon@domain.co;uk',
        'trailing-semicolon@domain.com;',
        '"email+leading-quotes@domain.com',
        'email+middle"-quotes@domain.com',
        '"quoted-local-part"@domain.com',
        '"quoted@domain.com"',
        'lots-of-dots@domain..gov..uk',
        'multiple@domains@domain.com',
        'spaces in local@domain.com',
        'spaces-in-domain@dom ain.com',
        'underscores-in-domain@dom_ain.com',
        'pipe-in-domain@example.com|gov.uk',
        'comma,in-local@gov.uk',
        'comma-in-domain@domain,gov.uk',
        'pound-sign-in-local£@domain.com',
        'local-with-’-apostrophe@domain.com',
        'local-with-”-quotes@domain.com',
        'domain-starts-with-a-dot@.domain.com',
    ];

    const validEmailAddresses = [
        'email@domain.com',
    'email@domain.COM',
    'firstname.lastname@domain.com',
    'firstname.o\'lastname@domain.com',
    'email@subdomain.domain.com',
    'firstname+lastname@domain.com',
    '1234567890@domain.com',
    'email@domain-one.com',
    '_______@domain.com',
    'email@domain.name',
    'email@domain.superlongtld',
    'email@domain.co.jp',
    'firstname-lastname@domain.com',
    'info@german-financial-services.vermögensberatung',
    'info@german-financial-services.reallylongarbitrarytldthatiswaytoohugejustincase',
    'japanese-info@例え.テスト',
    'technically..valid@domain.com',
    ];

    beforeEach(() => {
        cy.session("Session 1", () => {
            cy.SignInSchool();
            cy.wait(1);
        });
    });
    
    invalidEmailAddresses.forEach((email) =>{
        it(`should invalidate the email address: ${email}`, () => {
            cy.visit("/Check/Enter_Details");
            cy.get('#FirstName').type(parentFirstName);
            cy.get('#LastName').type(parentLastName);
            cy.get('#Day').type('01');
            cy.get('#Month').type('01');
            cy.get('#Year').type('1990');
            cy.get('#NinAsrSelection').click();
            cy.get('#NationalInsuranceNumber').type(NIN);
            cy.get('#EmailAddress').clear().type(email);
            cy.contains('Perform check').click();

            cy.get('.govuk-error-message').and('contain', 'Enter an email address in the correct format, like name@example.com');
        });
    });
    
    validEmailAddresses.forEach((email) => {
        it(`${email} should be valid` , () => {
            cy.visit("/Check/Enter_Details");
            cy.get('#FirstName').type(parentFirstName);
            cy.get('#LastName').type(parentLastName);
            cy.get('#Day').type('01');
            cy.get('#Month').type('01');
            cy.get('#Year').type('1990');
            cy.get('#NinAsrSelection').click();
            cy.get('#NationalInsuranceNumber').type(NIN);
            cy.get('#EmailAddress').clear().type(email);
            cy.contains('Perform check').click();

            cy.get('li').should('not.have.text', 'Enter the email address in the correct format, like name@example.com');
        });
    });
});