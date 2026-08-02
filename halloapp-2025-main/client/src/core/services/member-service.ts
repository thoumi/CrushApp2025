import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { EditableMember, Member, MemberParams, Photo, Prompt } from '../../types/member';
import { tap } from 'rxjs';
import { PaginatedResult } from '../../types/pagination';
import type { paths } from '../api/schema';

// Généré depuis le contrat OpenAPI (npm run generate:api-types dans client/) :
// toute dérive entre MemberParams et la vraie query GET /api/Members casse la compilation.
type MemberListQuery = NonNullable<paths['/api/Members']['get']['parameters']['query']>;

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  editMode = signal(false);
  member = signal<Member | null>(null);

  getDailySelection(count = 6) {
    const params = new HttpParams().append('count', count);
    return this.http.get<Member[]>(this.baseUrl + 'members/daily', { params });
  }

  getMembers(memberParams: MemberParams) {
    const query: MemberListQuery = {
      PageNumber: memberParams.pageNumber,
      PageSize: memberParams.pageSize,
      MinAge: memberParams.minAge,
      MaxAge: memberParams.maxAge,
      OrderBy: memberParams.orderBy,
      ...(memberParams.gender ? { Gender: memberParams.gender } : {})
    };

    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      params = params.append(key, value as string | number | boolean);
    }

    return this.http.get<PaginatedResult<Member>>(this.baseUrl + 'members', {params}).pipe(
      tap(() => {
        localStorage.setItem('filters', JSON.stringify(memberParams))
      })
    )
  }

  getMember(id: string) {
    return this.http.get<Member>(this.baseUrl + 'members/' + id).pipe(
      tap(member => {
        this.member.set(member)
      })
    )
  }

  getMemberPhotos(id: string) {
    return this.http.get<Photo[]>(this.baseUrl + 'members/' + id + '/photos');
  }

  updateMember(member: EditableMember) {
    return this.http.put(this.baseUrl + 'members', member);
  }

  uploadPhoto(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<Photo>(this.baseUrl + 'members/add-photo', formData);
  }

  setMainPhoto(photo: Photo) {
    return this.http.put(this.baseUrl + 'members/set-main-photo/' + photo.id, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'members/delete-photo/' + photoId);
  }

  getPromptBank() {
    return this.http.get<Prompt[]>(this.baseUrl + 'members/prompts/bank');
  }

  savePromptAnswers(answers: { promptId: number; answer: string }[]) {
    return this.http.put(this.baseUrl + 'members/prompts', { answers });
  }
}
