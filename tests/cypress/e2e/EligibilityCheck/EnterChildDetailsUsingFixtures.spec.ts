
describe('Parents can enter childs details successfully', () => {

    it('Allows a parent to enter their childs details', () => {
        // import Child Details fixtures
        cy.fixture('childDetails').then(details => {

            cy.visit('/Check/Enter_Child_Details');
            cy.get('h1').should('include.text', 'Provide details of your children');
    
            // cy.log(childDetails.childOne.FirstName)
            cy.get('[id="ChildList[0].FirstName"]').type(details.childOne.firstName);
            cy.get('[id="ChildList[0].LastName"]').type(details.childOne.lastName);
            cy.get('[id="ChildList[0].School.Name"]').type(details.childOne.school);
    
            cy.get('#schoolList0')
                .should('be.visible')
                .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
                .click({ force: true})
    
            cy.get('[id="ChildList[0].Day"]').type(details.childOne.birthDate.day);
            cy.get('[id="ChildList[0].Month"]').type(details.childOne.birthDate.month);
            cy.get('[id="ChildList[0].Year"]').type(details.childOne.birthDate.year);
    
            cy.contains('Save and continue').click();
    
            cy.contains(details.childOne.fullDetails.fullName);
            cy.contains(details.childOne.school);
            cy.contains(details.childOne.fullDetails.fullBirthDate);
    
            cy.get('h1').should('contain.text', 'Check your answers before registering');
            cy.contains('Register details').click();
        });

    });

    it('Allows a parent to enter details for multiple children', () => {
        cy.fixture('childDetails').then(details => {
            cy.visit('/Check/Enter_Child_Details');
            cy.get('h1').should('include.text', 'Provide details of your children');
            
    
            cy.get('[id="ChildList[0].FirstName"]').type(details.childOne.firstName);
            cy.get('[id="ChildList[0].LastName"]').type(details.childOne.lastName);
            cy.get('[id="ChildList[0].School.Name"]').type(details.childOne.school);

            cy.get('#schoolList0')
                .should('be.visible')
                .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
                .click( {force: true} );
    
            cy.get('[id="ChildList[0].Day"]').type(details.childOne.birthDate.day);
            cy.get('[id="ChildList[0].Month"]').type(details.childOne.birthDate.month);
            cy.get('[id="ChildList[0].Year"]').type(details.childOne.birthDate.year);
            
            cy.contains('Add another child').click();
    
            cy.get('[id="ChildList[1].FirstName"]').type(details.childTwo.firstName);
            cy.get('[id="ChildList[1].LastName"]').type(details.childTwo.lastName);
            cy.get('[id="ChildList[1].School.Name"]').type(details.childTwo.school);

            cy.get('#schoolList1')
                .should('be.visible')
                .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
                .click( {force: true });
    
            cy.get('[id="ChildList[1].Day"]').type(details.childTwo.birthDate.day);
            cy.get('[id="ChildList[1].Month"]').type(details.childTwo.birthDate.month);
            cy.get('[id="ChildList[1].Year"]').type(details.childTwo.birthDate.year);
    
            cy.contains('Save and continue').click();
    
            cy.get('h1').should('contain.text', 'Check your answers before registering');
    
            cy.contains('Child 1');
            cy.contains(details.childOne.fullDetails.fullName);
            cy.contains(details.childOne.school);
            cy.contains(details.childOne.fullDetails.fullBirthDate);
    
            cy.contains('Child 2');
            cy.contains(details.childTwo.fullDetails.fullName);
            cy.contains(details.childTwo.school);
            cy.contains(details.childTwo.fullDetails.fullBirthDate);
    
            cy.contains('Register details').click();

        });
    });

});