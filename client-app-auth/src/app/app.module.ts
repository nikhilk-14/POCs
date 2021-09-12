import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { environment } from 'src/environments/environment';
import { MsalModule } from '@azure/msal-angular';
import { HttpClientModule, HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import { CRUDComponent } from './crud/crud.component';
import { ApiComponent } from './api/api.component';
import { HomeComponent } from './home/home.component';
import { TokenInterceptor } from './token-interceptor';

export const protectedResourceMap: any =
  [
    [
      environment.baseUrl, environment.scopeUri
    ]
  ];

@NgModule({
  declarations: [
    AppComponent,
    CRUDComponent,
    ApiComponent,
    HomeComponent
  ],
  imports: [
    MsalModule.forRoot({
      clientID: environment.uiClienId,
      authority: 'https://login.microsoftonline.com/' + environment.tenantId,
      cacheLocation: 'localStorage',  
      protectedResourceMap: protectedResourceMap,
      redirectUri: environment.redirectUrl
    }),
    BrowserModule,
    AppRoutingModule,
    HttpClientModule
  ],
  providers: [
    HttpClient,  
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
