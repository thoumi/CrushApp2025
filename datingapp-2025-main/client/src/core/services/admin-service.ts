import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../../types/user';
import { Photo } from '../../types/member';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getUserWithRoles() {
    return this.http.get<User[]>(this.baseUrl + 'admin/users-with-roles');
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
  //Add block user
  blockUser(userId: string) {
    return this.http.post(this.baseUrl + 'admin/block-user/' + userId, {});
  }
  //Add unblock user
  unblockUser(userId: string) {
    return this.http.post(this.baseUrl + 'admin/unblock-user/' + userId, {});
  }
  //add search users
  searchUsers(query: string) {
    return this.http.get<User[]>(this.baseUrl + 'admin/search-users?query=' + query);
  }
}
