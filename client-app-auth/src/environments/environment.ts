// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  baseUrl: 'https://localhost:5001/',
  scopeUri: [
    'api://b66d8e8d-1769-4ef8-85d0-1db93e4fa0de/api-access'
  ],
  tenantId: '8c3dad1d-b6bc-4f8b-939b-8263372eced6',
  uiClienId: '293dcba8-8b10-4921-988d-8f4df79fb938',
  redirectUrl: 'http://localhost:4200'
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
