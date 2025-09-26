import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { User } from '../../types/user';
import { Member, MemberParams, Photo } from '../../types/member';
import { Observable } from 'rxjs/internal/Observable';
import { PaginatedResult } from '../../types/pagination';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getUserWithRoles(memberParams: MemberParams): Observable<PaginatedResult<User>> {
    let params = new HttpParams();
  
    if (memberParams.pageNumber) {
      params = params.append('pageNumber', memberParams.pageNumber.toString());
    }
    if (memberParams.pageSize) {
      params = params.append('pageSize', memberParams.pageSize.toString());
    }
    if (memberParams.searchTerm) {
      params = params.append('searchTerm', memberParams.searchTerm);
    }
    if (memberParams.roles && memberParams.roles.trim()) {
      params = params.append('roles', memberParams.roles);
    }
    if (memberParams.isLockedOut !== undefined) {
      params = params.append('isLockedOut', memberParams.isLockedOut.toString());
    }
  
    // Debug: afficher les paramètres envoyés
    console.log('Sending params to backend:', params.toString());
    console.log('MemberParams:', memberParams);
  
    return this.http.get<PaginatedResult<User>>(this.baseUrl + 'admin/users-with-roles', { params });
  }

  updateUserRoles(userId: string, roles: string[]) {
    return this.http.post<string[]>(this.baseUrl + 'admin/edit-roles/' 
        + userId + '?roles=' + roles, {})
  }

  getPhotosForApproval() {
    return this.http.get<Photo[]>(this.baseUrl + 'admin/photos-to-moderate');
  }

  approvePhoto(photoId: number) {
    return this.http.post(this.baseUrl + 'admin/approve-photo/' + photoId, {});
  }

  rejectPhoto(photoId: number) {
    return this.http.post(this.baseUrl + 'admin/reject-photo/' + photoId, {});
  }
  // block/unblock user
  toggleUserStatus(userId: string){
    return this.http.post<{isLockedOut: boolean}>(this.baseUrl + 'admin/toggle-user-status/' + userId, {});
  }
  
 //add search users
  searchUsers(query: string) {
    return this.http.get<User[]>(this.baseUrl + 'admin/search-users?query=' + query);
  }
}
