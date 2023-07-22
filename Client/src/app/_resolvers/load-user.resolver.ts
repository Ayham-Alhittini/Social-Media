import { Injectable } from '@angular/core';
import {
  Router, Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot
} from '@angular/router';
import { Observable, of, take } from 'rxjs';
import { User } from '../Models/user';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root'
})
export class LoadUserResolver implements Resolve<User> {
  constructor(private accountService: AccountService){}
  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<User> {
    return this.accountService.loadedUser.pipe(take(1));
  }
}
