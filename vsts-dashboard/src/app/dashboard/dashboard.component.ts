import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { ModalDismissReasons, NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  @ViewChild('modalData') modalData: any;

  users: any = [];
  selectedUser: any = {};
  selectUserWorkItems: any = {};
  showLoader: boolean = false;
  closeModal: string = '';
  nomination: any = {};

  constructor(private httpClient: HttpClient, private modalService: NgbModal) { }

  ngOnInit(): void {
    this.httpClient.get('assets/users.json').subscribe(data => {
      this.users = data;
      this.users = this.users.sort((item1: any, item2: any) => { return item1.name < item2.name ? -1 : 1; })
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
      this.nomination = this.getRandomItem(this.users);
      this.triggerModal(this.modalData);
    }
  }

  getRandomItem(items: any[]) {
    return items[Math.floor(Math.random() * items.length)];
  }

  getUserWorkItemIds(user: any) {
    this.showLoader = true;
    this.selectUserWorkItems = {};
    let inputJson = {
      'query': `Select [System.ID] From WorkItems Where [System.WorkItemType] In ("Task","Bug") AND [State] <> "Removed" AND [System.IterationPath] Under @CurrentIteration('[Category Management System]\\Category Management System Team') AND [System.AssignedTo] == "${user.name}"`
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
      }
    }, (err) => {
    }, () => {
    });
  }

  triggerModal(content: any) {
    this.modalService.open(content, {ariaLabelledBy: 'modal-basic-title'}).result.then((res) => {
      this.closeModal = `Closed with: ${res}`;
    }, (res) => {
      this.closeModal = `Dismissed ${this.getDismissReason(res)}`;
    });
  }
  
  private getDismissReason(reason: any): string {
    if (reason === ModalDismissReasons.ESC) {
      return 'by pressing ESC';
    } else if (reason === ModalDismissReasons.BACKDROP_CLICK) {
      return 'by clicking on a backdrop';
    } else {
      return  `with: ${reason}`;
    }
  }
}
