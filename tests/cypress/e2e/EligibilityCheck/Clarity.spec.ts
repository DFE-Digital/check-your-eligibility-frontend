describe('Clarity', () => {
    it('Loads Clarity when it is enabled', () => {
        cy.visit("/");

        cy.get('body')
            .invoke('attr', 'data-clarity')
            .then(($clarity) => {
                if($clarity) {
                    cy.get('head script[src*="clarity"]');
                }
            }
            );
    });

    it('Does not Clarity when it is disabled', () => {
        cy.visit("/");

        cy.get('body')
            .invoke('attr', 'data-clarity')
            .then(($clarity) => {
                    if(!$clarity) {
                        cy.get('head script[src*="clarity"]').should('not.exist');
                    }
                }
            );
    });
});