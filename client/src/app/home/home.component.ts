import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  registerMode = false;
  constructor() {}

  ngOnInit(): void {}

  // tslint:disable-next-line: typedef
  registerToggle() {
    // tslint:disable-next-line: no-unused-expression
    console.log('s');

    this.registerMode = !this.registerMode;
  }
}
