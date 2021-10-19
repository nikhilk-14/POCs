import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from "@angular/common/http";

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  users: any = [];
  selectedUser: any = {};
  selectUserWorkItems: any = {};
  showLoader: boolean = false;

  constructor(private httpClient: HttpClient) { }

  ngOnInit(): void {
    this.httpClient.get('assets/users.json').subscribe(data => {
      this.users = data;
    });
  }

  getNextUser() {
    this.selectedUser = {};
    let items = this.users.filter((itm: any) => !itm.isDone);
    if (items && items.length) {
      this.selectedUser = this.getRandomItem(items);
      if (this.selectedUser) {
        this.selectedUser.isDone = true;
        this.getUserWorkItemIds(this.selectedUser);
      }
    } else {
      let nomination = this.getRandomItem(this.users);
      alert(`All are done, thanks for joining standup and providing updates, I would like to nominate ${nomination.name} for tomorrow's standup.`);
    }
  }

  getRandomItem(items: any[]) {
    return items[Math.floor(Math.random() * items.length)];
  }

  getUserWorkItemIds(user: any) {
    this.showLoader = true;
    let inputJson = {
      'query': `Select [System.ID] From WorkItems Where [System.WorkItemType] In ("Task","Bug") AND [State] <> "Removed" AND [System.IterationPath] Under "Category Management System\\Sprint 113" AND [System.AssignedTo] == "${user.name}"`
    };
    let vsts_url = 'https://dev.azure.com/seyc/_apis/wit/wiql?api-version=6.0';
    let workItemsUrl = 'https://dev.azure.com/seyc/_apis/wit/workItems';
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Basic OmZ4dDZoeWRoeWN4bGxoNjdlNWVtejJka2RjdzNkNjQzNTM1dWlrZmlkcmY0dXNzbWlmb3E=`,
      'X-TFS-FedAuthRedirect': 'Suppress' 
    });
    let options = { headers: headers };
    this.httpClient.post(vsts_url, inputJson, options).subscribe((data: any) => {
      if (data && data.workItems && data.workItems.length) {
        let workItemIds = data.workItems.map((itm: any) => itm['id']).join(',');
        let url = `${workItemsUrl}?ids=${workItemIds}&fields=System.ID,System.Title,System.WorkItemType,System.State&api-version=6.0`;
        this.httpClient.get(url, options).subscribe((workItems: any) => {
          let items = workItems.value.map((itm: any) => itm.fields);
          this.selectUserWorkItems = items;
        }, (err) => {
        }, () => {
          this.showLoader = false;
        });
      } else {
        this.showLoader = false;
        this.selectUserWorkItems = {};
      }
    }, (err) => {
    }, () => {
    });
  }
}
