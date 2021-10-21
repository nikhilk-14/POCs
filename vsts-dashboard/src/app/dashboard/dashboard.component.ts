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

  @ViewChild('modalMessage') modalMessage: any;
  modalMessageHeader: string = '';
  modalMessageBody: string = '';

  users: any = [];
  selectedUser: any = {};
  selectUserWorkItems: any = {};
  showLoader: boolean = false;
  closeModal: string = '';
  nomination: any = {};

  isValidToken: boolean = false;
  vstsApiUrl: string = 'https://dev.azure.com';
  orgName: string = 'seyc';
  projectName: string = '[Category Management System]\\Category Management System Team';

  pat: any = '';

  constructor(private httpClient: HttpClient, private modalService: NgbModal) { }

  ngOnInit(): void {
    this.httpClient.get('assets/users.json').subscribe(data => {
      this.users = data;
      this.users = this.users.sort((item1: any, item2: any) => { return item1.name < item2.name ? -1 : 1; })
    });
  }

  submitToken() {
    if (this.pat) {
      this.showLoader = true;
      let options = this.getHeaders();
      let vstsProjectsUrl = `${this.vstsApiUrl}/${this.orgName}/_apis/projects`;
      this.httpClient.get(vstsProjectsUrl, options)
      .subscribe((data: any) => {
          this.isValidToken = true;
        }, (err: any) => {
          this.modalMessageHeader = 'Error';
          this.modalMessageBody = 'Invalid Personal Access Token';
          this.triggerModal(this.modalMessage);
          this.isValidToken = false;
        }, () => {
          this.showLoader = false;
      }).add(() => {
        this.showLoader = false;
      });
    } else {
      this.modalMessageHeader = 'Warning';
      this.modalMessageBody = 'Please provide Personal Access Token';
      this.triggerModal(this.modalMessage);
    }
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

  getWorkItemsByIds(workItemIds: string, options: any) {
    let workItemsApiUrl = `${this.vstsApiUrl}/${this.orgName}/_apis/wit/workItems`;
    let workItemsUrl = `${workItemsApiUrl}?ids=${workItemIds}&fields=System.ID,System.Title,System.WorkItemType,System.State&api-version=6.0`;
    
    this.httpClient.get(workItemsUrl, options).subscribe((workItems: any) => {
      let items = workItems.value.map((itm: any) => itm.fields);
      this.selectUserWorkItems = items;
    }, (err) => {
      this.showLoader = false;
      this.modalMessageHeader = 'Error';
      this.modalMessageBody = 'Error in getting data';
      this.triggerModal(this.modalMessage);
    }, () => {
        this.showLoader = false;
    });
  }

  getHeaders(): any {
    let basicToken = btoa(`:${this.pat}`);
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Basic ${basicToken}`,
      'X-TFS-FedAuthRedirect': 'Suppress' 
    });
    let options = { headers: headers };
    return options;
  }

  getUserWorkItemIds(user: any) {
    this.showLoader = true;
    this.selectUserWorkItems = {};

    let queryDataUrl = `${this.vstsApiUrl}/${this.orgName}/_apis/wit/wiql?api-version=6.0`;
    let inputJson = {
      'query': `Select [System.ID] From WorkItems Where [System.WorkItemType] In ("Task","Bug") AND [State] <> "Removed" AND [System.IterationPath] Under @CurrentIteration('${this.projectName}') AND [System.AssignedTo] == "${user.name}"`
    };

    let options = this.getHeaders();

    this.httpClient.post(queryDataUrl, inputJson, options).subscribe((data: any) => {
      if (data && data.workItems && data.workItems.length) {
        let workItemIds = data.workItems.map((itm: any) => itm['id']).join(',');
        this.getWorkItemsByIds(workItemIds, options);
      } else {
        this.showLoader = false;
      }
    }, (err) => {
      this.showLoader = false;
      this.modalMessageHeader = 'Error';
      this.modalMessageBody = 'Error in getting data';
      this.triggerModal(this.modalMessage);
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
