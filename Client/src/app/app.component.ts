import { Component, OnInit } from '@angular/core';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  title = 'client';
  users : any;
  constructor(private accountService : AccountService){}

  ngOnInit(): void {
    this.autoLogin();
  }
  autoLogin()
  {
    this.accountService.autoLogin();
  }
}
