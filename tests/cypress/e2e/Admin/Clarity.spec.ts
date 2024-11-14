describe('Clarity', () => {
    var schoolClarityId: string
    var LAClarityId: string
    
    it('Loads Clarity when it is enabled', () => {
        cy.SignInSchool();

        cy.get('body')
            .invoke('attr', 'data-clarity')
            .then(($clarity) => {
                if($clarity) 
                    schoolClarityId = $clarity;
                    cy.get('head script[src*="clarity"]');
                }
            );
    });

    it('Does not Clarity when it is disabled', () => {
        cy.SignInLA();

        cy.get('body')
            .invoke('attr', 'data-clarity')
            .then(($clarity) => {
                    if(!$clarity) {
                        cy.get('head script[src*="clarity"]').should('not.exist');
                    }
                    
                    else {
                        LAClarityId = $clarity;
                    }
                }
            );
    });

    it('Loads different Clarity for School and LA', () => {
        if(schoolClarityId) {
            expect(schoolClarityId).not.equal(LAClarityId)
        }
    });
});