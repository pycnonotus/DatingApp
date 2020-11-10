import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResults } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  user: User;
  userParams: UserParams;

  constructor(
    private http: HttpClient,
    private accountService: AccountService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe((user) => {
      this.user = user;
      this.userParams = new UserParams(user);
    });
  }

  getUserParams() {
    return this.userParams;
  }
  setUserParams(userParams: UserParams) {
    this.userParams = userParams;
  }

  private getPaginationHeader(pageNumber: number, pageSize: number) {


    let params = new HttpParams();
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());
    return params;
  }

  getMembers(userParam: UserParams) {
    var response = this.memberCache.get(Object.values(userParam).join('-'));
    if (response) {
      return of(response);
    }
    let params = this.getPaginationHeader(
      userParam.pageNumber,
      userParam.pageSize
    );
    params = params.append('minAge', userParam.minAge.toString());
    params = params.append('maxAge', userParam.maxAge.toString());
    params = params.append('gender', userParam.gender);
    params = params.append('orderBy', userParam.orderBy);
    return this.getPaginatedResult<Member[]>(
      this.baseUrl + 'users',
      params
    ).pipe(
      map((response) => {
        this.memberCache.set(Object.values(userParam).join('-'), response);
        return response;
      })
    );
  }
  // tslint:disable-next-line: typedef
  // tslint:disable-next-line: no-shadowed-variable
  // tslint:disable-next-line: typedef
  private getPaginatedResult<T>(url, params: HttpParams) {
    const paginatedResults: PaginatedResults<T> = new PaginatedResults<T>();
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

  getMember(username: string): Observable<Member> {
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => {
        return arr.concat(elem.result);
      }, [])
      .find((x: Member) => x.username === username);
    if (member !== undefined) {
      return of(member);
    }
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }
  updateMember(member: Member) {
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

  addLike(username: string) {
    return this.http.post(this.baseUrl + 'like/' + username, {});
  }
  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = this.getPaginationHeader(pageNumber, pageSize);
    params = params.append('predicate', predicate);
    return this.getPaginatedResult<Partial<Member[]>>(
      this.baseUrl + 'like',
      params
    );
  }
}
