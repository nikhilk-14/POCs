import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { WeatherForecast } from './weather-forecast';

@Injectable({
  providedIn: 'root'
})
export class DataService {

  private url = environment.baseUrl + 'weatherforecast';

  httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  constructor(private http: HttpClient) { }

  //Any Data Type
  getAPIData(): Observable<WeatherForecast[]> {
    this.httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    return this.http.get<WeatherForecast[]>(this.url, this.httpOptions);
  }
}
