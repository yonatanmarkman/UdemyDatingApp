import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { LikesParams, Member } from '../../types/member';
import { PaginatedResult } from '../../types/pagination';
import { CurrentUserService } from './current-user-service';

@Injectable({
  providedIn: 'root'
})
export class LikesService {
  private baseUrl = environment.apiUrl;
  private httpClient = inject(HttpClient);
  private CurrentUserService = inject(CurrentUserService);

  likeIds = signal<string[]>([]);

  toggleLike(targetMemberId: string) {
    return this.httpClient.post(`${this.baseUrl}likes/${targetMemberId}`, {});
  }

  getLikes(likesParams: LikesParams) {
    let params = new HttpParams();

    params = params.append('predicate', likesParams.predicate);
    params = params.append('pageNumber', likesParams.pageNumber);
    params = params.append('pageSize', likesParams.pageSize);


    return this.httpClient.get<PaginatedResult<Member>>(
      this.baseUrl + `likes`,
      {params}
    );
  }

  getLikeIds() {
    return this.httpClient.get<string[]>(this.baseUrl + 'likes/list').subscribe({
      next: ids => {
        this.likeIds.set(ids);
      },
      error: error => {
        console.log('Caught error from server while trying to get like Ids. Logging out...');
        console.log('Error info: ');
        console.log(error);
        
        localStorage.removeItem('user');
        localStorage.removeItem('filters');
        this.clearLikeIds();
        this.CurrentUserService.setCurrentUserToNull();
      }
    });
    
  }

  clearLikeIds() {
    this.likeIds.set([]);
  }
}
