import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {BehaviorSubject, tap} from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../Models/user';
import { PresenceService } from './presence.service';
@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private http : HttpClient, private presenceService: PresenceService) { }

  private baseUrl = environment.apiBase;

  loadedUser = new BehaviorSubject<User>(null);

  login(model : any)
  {
    return this.http.post<User>(this.baseUrl + "account/login", model).pipe(
      tap(
        res => {
          this.setCurrentUser(res);
        },
      )
    );
  }


  autoLogin()
  {
    const user = localStorage.getItem('user');

    if (user) {
      this.loadedUser.next(JSON.parse(user));
      this.presenceService.createHubConnection(JSON.parse(user));
    }
  }

  logout() {
    localStorage.removeItem('user');
    this.loadedUser.next(null);
    window.location.href = '/';

    this.presenceService.stopHubConnection();
  }


  register(model : any) {
    return this.http.post<User>(this.baseUrl + "account/register", model).pipe(
      tap(
        res => {
          this.setCurrentUser(res);
        },
      )
    );
  }

  setCurrentUser(user : User) {
    this.loadedUser.next(user);
    localStorage.setItem('user', JSON.stringify(user));

    this.presenceService.createHubConnection(user);
  }
}
