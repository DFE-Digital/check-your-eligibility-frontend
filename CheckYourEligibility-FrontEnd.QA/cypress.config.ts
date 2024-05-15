import { defineConfig } from 'cypress';

export default defineConfig({
  e2e: {
    specPattern: '**/*.spec.{js,jsx,ts,tsx}',
    // Configure e2e options here
    setupNodeEvents(on, config) {
      // e2e testing node events setup
      
      
    },
    baseUrl: 'https://ecs-dev-as-frontend.azurewebsites.net/', 
    viewportWidth: 1600,
    viewportHeight: 1800,
  },
});
