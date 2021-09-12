import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { ApiComponent } from './api/api.component';
import { CRUDComponent } from './crud/crud.component';
import { HomeComponent } from './home/home.component';

const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
    canActivate: [MsalGuard]
  },
  {
    path: 'api',
    component: ApiComponent,
    canActivate: [MsalGuard]
  },
  {
    path: 'crud',
    component: CRUDComponent,
    canActivate: [MsalGuard]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
