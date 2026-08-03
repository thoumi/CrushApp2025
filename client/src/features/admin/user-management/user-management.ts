import { Component, ElementRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { AdminService } from '../../../core/services/admin-service';
import { User } from '../../../types/user';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MemberParams } from '../../../types/member';
import { PaginatedResult } from '../../../types/pagination';
import { Paginator } from "../../../shared/paginator/paginator";
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-user-management',
  imports: [CommonModule, RouterModule, FormsModule, Paginator, TranslatePipe],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css'
})
export class UserManagement implements OnInit {

selectedStatus: any;
selectedRole: any;
  @ViewChild('rolesModal') rolesModal!: ElementRef<HTMLDialogElement>;
  @ViewChild('filterModal') filterModal!: ElementRef<HTMLDialogElement>;
  private adminService = inject(AdminService);
  protected paginatedMembers = signal<PaginatedResult<User> | null>(null);
  protected users = signal<User[]>([]);
  protected availableRoles = ['Member', 'Moderator', 'Admin'];
  protected selectedUser: User | null = null;
  protected memberParams = new MemberParams();
  private updatedParams = new MemberParams();
  searchTerm = '';
paginatedResult: any;

  ngOnInit(): void {
    this.getUserWithRoles();
  }

  getUserWithRoles() {
    this.adminService.getUserWithRoles(this.memberParams).subscribe({
      next: (paginatedResult: PaginatedResult<User>) => {
        this.paginatedMembers.set(paginatedResult);
        this.users.set(paginatedResult.items);
      },
      error: error => console.log('Failed to load users', error)
    });
  }

  openRolesModal(user: User) {
    this.selectedUser = user;
    this.rolesModal.nativeElement.showModal();
  }

  toggleRole(event: Event, role: string) {
    if (!this.selectedUser) return;
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.selectedUser.roles.push(role);
    } else {
      this.selectedUser.roles = this.selectedUser.roles.filter(r => r !== role);
    }
  }

  updateRoles() {
    if (!this.selectedUser) return;
    this.adminService.updateUserRoles(this.selectedUser.id, this.selectedUser.roles).subscribe({
      next: updatedRoles => {
        this.users.update(users => users.map(u => {
          if (u.id === this.selectedUser?.id) u.roles = updatedRoles;
          return u;
        }));
        this.rolesModal.nativeElement.close();
      },
      error: error => console.log('Failed to update roles', error)
    })
  }

  //add search users
  searchUsers(query: string) {
  this.memberParams.searchTerm = query;
  if (!query.trim()) {
   this.memberParams.pageNumber = 1; 
  }
  this.getUserWithRoles();
  } 

  toggleUser(user: User) {
    const wasLocked = user.isLockedOut;
    user.isLockedOut = !user.isLockedOut;

    this.adminService.toggleUserStatus(user.id).subscribe({
      next: (res : {isLockedOut: boolean}) => user.isLockedOut = res.isLockedOut,
      error: () => {
        console.log('Failed to toggle user status');
      }
    });
  }

  //open filter modal
  openFilterModal() {
    this.filterModal.nativeElement.showModal();

  }

  openModal() {
    this.filterModal.nativeElement.showModal();
  }

  onClose() {
    this.filterModal.nativeElement.close();
  }

  applyFilters() {
    const newMemberParams = { ...this.memberParams };
    
    // Récupérer la valeur sélectionnée pour le rôle
    if (this.selectedRole && this.selectedRole.trim()) {
      newMemberParams.roles = this.selectedRole.trim();
    } else {
      newMemberParams.roles = '';
    }
  
    // Transformer le status sélectionné en booléen
    if (this.selectedStatus === 'active') {
      newMemberParams.isLockedOut = false;
    } else if (this.selectedStatus === 'blocked') {
      newMemberParams.isLockedOut = true;
    } else {
      newMemberParams.isLockedOut = undefined; // Aucun filtre
    }
  
    // Appliquer les filtres à memberParams
    newMemberParams.pageNumber = 1; // reset page
    // Update the component's memberParams
    this.memberParams = newMemberParams;
  
    // Recharger les utilisateurs filtrés
    this.getUserWithRoles();
  
    // Fermer le modal
    this.onClose();
  }
  

  resetFilters() {
    this.memberParams = new MemberParams();
    this.updatedParams = new MemberParams();
    this.selectedRole = '';
    this.selectedStatus = '';
    this.searchTerm = '';
    this.getUserWithRoles();
  }

  onPageChange(event: {pageNumber: number, pageSize: number}) {
    this.memberParams.pageSize = event.pageSize;
    this.memberParams.pageNumber = event.pageNumber;
    this.getUserWithRoles();
  }

}
