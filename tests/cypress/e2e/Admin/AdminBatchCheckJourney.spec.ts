
describe("Admin Portal journey for checking parent eligibility using the Batch Checking Service", () => {

    it("will return an error message if the batch file contains more than 250 rows of data", () => {
        cy.SignInSchool()
        cy.contains('Run a batch check').click()
        cy.get('input[type=file]').selectFile('batchchecktemplate_too_many_records.csv')
        cy.contains('Perform Checks').click()

        cy.get('#file-upload-1-error').as('errorMessage')
        cy.get('@errorMessage').should(($p) => {
            expect($p.first()).to.contain('CSV File cannot contain more than 250 records')
        })

    }) 

    it("will return an error message if more than 10 batches are attempted within an hour", () => {
        cy.SignInSchool()
        cy.contains('Run a batch check').click()
        for (let i = 0; i < 11; i++) {
            cy.get('input[type=file]').selectFile('batchchecktemplate_too_many_records.csv')
            cy.contains('Perform Checks').click()
        }

        cy.get('#file-upload-1-error').as('errorMessage')
        cy.get('@errorMessage').should(($p) => {
            expect($p.first()).to.contain('No more than 10 bulk check requests can be made per hour')
        })
    })
})