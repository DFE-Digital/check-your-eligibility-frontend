# Check Your Eligibility Frontend
This repo contains the user-facing parts for Eligibility Checking Engine (ECE) and the Check Free School Meals (FSM) service.

## Setup
This is a .NET 8 project - you'll need the latest .NET SDK etc to run it locally.

### Config
When you first clone the repo, you'll want to define your own config. You'll want to copy up the
file [appsettings.json](CheckYourEligibility-Parent/appsettings.json), name the copy `appsettings.developmnent.json`
in the same folder. Update the values in this new file as needed. This file should not be committed, nor will it with our .gitignore.

## How to run tests
We have two test-suites - one .NET NUnit for unit tests and one Cypress for integration and e2e tests. Cypress needs a running application responding to http calls.

### .NET
VisualStudio does most of this for you, there'll be a button in your UI. Correctly set up Rider too.
`
cd CheckYourEligibility-Parent.Tests
dotnet test
`

### Cypress
Assuming you have NPM installed.

`
cd tests
npm install
CYPRESS_BASE_URL="https://ecs-test-as-frontend.azurewebsites.net" npx cypress open
`
Inside the cypress directory create a "cypress.env.json" and paste in these:

{
    "AUTHORIZATION_HEADER": "",
    "AUTH_USERNAME": "",
    "AUTH_PASSWORD": "",
    "ONEGOV_EMAIL": "",
    "ONEGOV_PASSWORD": "",
    "AUTH_SECRET": ""
}

## Ways of working
### Releasing code
We submit PRs into `main` for functioning code. The reviewer checks that the automated tests pass, then approve.

We expect the code reviewer to run the code locally, read through it and understand what it is trying to solve.

If approved, the original code-creator merges the PR and deletes the branch.

### Secrets
We don't commit active secrets to this repo. If we do, it is crucial to notify DM/TL/PO, rewrite git history and follow DfE processes.

## Resources
### Deployment
![Deployment](docs/images/frontend-pipeline.png)

## SQL
delete all data

delete [dbo].[ApplicationStatuses]
delete [dbo].Applications
delete EligibilityCheck
delete EligibilityCheckHashes