import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { map, of, take, tap } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../Models/member';
import {  Pagination, PaginationResult } from '../Models/pagination';
import { Photo } from '../Models/photo';
import { User } from '../Models/user';
import { UserParams } from '../Models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';


@Injectable({
  providedIn: 'root'
})
export class MemberService {

  private baseUrl = environment.apiBase;
  
  membersCache = new Map();
  user: User;
  userParams: UserParams;

  constructor(private http : HttpClient, private accountService: AccountService ) { 
    this.accountService.loadedUser.pipe(take(1)).subscribe({
      next : res => {
        this.user = res;
        this.userParams = new UserParams(this.user);
      }
    });
  }


  getUserParams() {
    return this.userParams;
  }

  setUserParams(userParams: UserParams) {
    this.userParams = userParams;
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user);
  }

  getMembers(userPrams: UserParams) {
    const response = this.membersCache.get(Object.values(userPrams).join('-'));
    if (response) {
      return of(response);
    }
    
    
    let params = getPaginationHeaders(userPrams.pageNumber, this.userParams.pageSize);
    params = params.append('minAge', userPrams.minAge);
    params = params.append('maxAge', userPrams.maxAge);
    params = params.append('gender', userPrams.gender);
    params = params.append('orderBy', userPrams.orderBy);

    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(
      tap(response => {
        this.membersCache.set(Object.values(userPrams).join('-'), response);
      })
    );
  }

  getMember(username : string) {
    const member = [...this.membersCache.values()]
      .reduce((arr, ele) => arr.concat(ele.result), [])
      .find(u => u.userName === username);
    
    if (member) {
      return of(member);
    }
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }
  
  updateMember(memberUpdateDto) {
    return this.http.put(this.baseUrl + 'users', memberUpdateDto);
  }
  
  setMainPhoto(photoId : number) {
    return this.http.put(this.baseUrl + 'Users/set-main-photo/' + photoId, {});
  }

  addLike(username: string) {
    return this.http.post<string>(this.baseUrl+ "Likes/"+ username,{});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {

    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('predicate', predicate);

    return getPaginatedResult<Member[]>(this.baseUrl + "likes", params, this.http);
  }

  removePhoto(photoId : number) {
    return this.http.delete(this.baseUrl + 'Users/delete-photo/' + photoId);
  }
}
