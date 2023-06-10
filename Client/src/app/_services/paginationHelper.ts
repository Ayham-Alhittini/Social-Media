import { HttpClient, HttpParams } from "@angular/common/http";
import { Pagination, PaginationResult } from "../Models/pagination";
import {map} from 'rxjs';

 export function  getPaginatedResult<T>(url: string ,params: HttpParams, http:HttpClient) {
    const paginationResult: PaginationResult<T> = new PaginationResult<T>();
    return http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        if (response.body) {
          paginationResult.result = response.body;
        }
        const pagination: Pagination = JSON.parse(response.headers.get('Pagination'));
        if (pagination) {
          paginationResult.pagination = pagination;
        }
        return paginationResult;
      })
    );
  }
  
  export function getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();
  
    params = params.append('pageNumber', pageNumber);
    params = params.append('pageSize', pageSize);
    
    return params;
  }