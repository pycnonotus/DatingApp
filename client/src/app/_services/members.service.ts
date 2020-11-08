import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member, Member as T } from '../_models/member';
import { PaginatedResults } from '../_models/pagination';
import { UserParams } from '../_models/userParams';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: T[] = [];

  constructor(private http: HttpClient) {}

  private getPaginationHeader(pageNumber: number, pageSize: number) {
    let params = new HttpParams();
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());
    return params;
  }

  getMembers(userParam: UserParams) {
    let params = this.getPaginationHeader(
      userParam.pageNumber,
      userParam.pageSize
    );
    params = params.append('minAge', userParam.minAge.toString());
    params = params.append('maxAge', userParam.maxAge.toString());
    params = params.append('gender', userParam.gender);
    return this.getPaginatedResult<Member[]>(this.baseUrl+'usrs',params);
  }
  // tslint:disable-next-line: typedef
  // tslint:disable-next-line: no-shadowed-variable
  // tslint:disable-next-line: typedef
  private getPaginatedResult<T>(url,params: HttpParams) {
    const paginatedResults: PaginatedResults<T> = new PaginatedResults<
      T
    >();
    return this.http
      .get<T>(url, {
        observe: 'response',
        params,
      })
      .pipe(
        map((response) => {
          paginatedResults.result = response.body;
          if (response.headers.get('Pagination') !== null) {
            paginatedResults.pagination = JSON.parse(
              response.headers.get('Pagination')
            );
          }
          return paginatedResults;
        })
      );
  }

  getMember(username: string): Observable<T> {
    const member = this.members.find((x) => x.username === username);
    if (member !== undefined) {
      return of(member);
    }
    return this.http.get<T>(this.baseUrl + 'users/' + username);
  }
  updateMember(member: T) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );
  }
  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }
  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }
}
