import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-error',
  templateUrl: './test-error.component.html',
  styleUrls: ['./test-error.component.css'],
})
export class TestErrorComponent implements OnInit {
  baseUrl = 'https://localhost:5001/api/';
  validationError: string[] = [];
  constructor(private http: HttpClient) {}

  ngOnInit(): void {}

  // tslint:disable-next-line: typedef
  get404Error() {
    this.http.get(this.baseUrl + 'buggy/not-found').subscribe(
      (res: Response) => console.log(res),
      (error) => {
        console.log(error);
      }
    );
  }
  // tslint:disable-next-line: typedef
  get400error() {
    this.http.get(this.baseUrl + 'buggy/bad-request').subscribe(
      (res: Response) => console.log(res),
      (error) => {
        console.log(error);
      }
    );
  }
  // tslint:disable-next-line: typedef
  get500Error() {
    this.http.get(this.baseUrl + 'buggy/server-error').subscribe(
      (res: Response) => console.log(res),
      (error) => {
        console.log(error);
      }
    );
  }
  // tslint:disable-next-line: typedef
  get401Error() {
    this.http.get(this.baseUrl + 'buggy/auth').subscribe(
      (res: Response) => console.log(res),
      (error) => {
        console.log(error);
      }
    );
  }
  // tslint:disable-next-line: typedef
  get400ValidationError() {
    this.http.post(this.baseUrl + 'account/register', {}).subscribe(
      (res: Response) => console.log(res),
      (error) => {
        this.validationError = error;
        console.log(error);
      }
    );
  }
}
