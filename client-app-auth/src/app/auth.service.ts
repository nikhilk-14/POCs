import { Injectable } from '@angular/core';
import { MsalService } from '@azure/msal-angular';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private auth: MsalService) { }

  public getToken(): any {
    return localStorage.getItem('msal.idtoken');
  }
}
