import { Component, OnInit } from '@angular/core';
import { DataService } from '../data.service';
import { WeatherForecast } from '../weather-forecast';

@Component({
  selector: 'app-api',
  templateUrl: './api.component.html',
  styleUrls: ['./api.component.scss']
})
export class ApiComponent implements OnInit {
  apiData: WeatherForecast[] = [];

  constructor(private dataService: DataService) { }

  ngOnInit(): void {
    this.getAPIData();
  }

  public getAPIData() {
    const apiObservable = this.dataService.getAPIData();
    apiObservable.subscribe(
      (response) => {                           //next() callback
        console.log('response received')
        this.apiData = response;
      },
      (error) => {                              //error() callback
        console.error('Request failed with error');
        console.error(JSON.stringify(error));
      },
      () => {                                   //complete() callback
        console.log('Request completed')      //This is actually not needed 
      })
  }
}
