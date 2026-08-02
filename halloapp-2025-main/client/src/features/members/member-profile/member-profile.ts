import { Component, HostListener, inject, OnDestroy, OnInit, signal, ViewChild } from '@angular/core';
import { EditableMember, Member, Prompt } from '../../../types/member';
import { DatePipe } from '@angular/common';
import { MemberService } from '../../../core/services/member-service';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastService } from '../../../core/services/toast-service';
import { AccountService } from '../../../core/services/account-service';
import { TimeAgoPipe } from '../../../core/pipes/time-ago-pipe';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-member-profile',
  imports: [DatePipe, FormsModule, TimeAgoPipe, TranslatePipe],
  templateUrl: './member-profile.html',
  styleUrl: './member-profile.css'
})
export class MemberProfile implements OnInit, OnDestroy {
  @ViewChild('editForm') editForm?: NgForm;
  @HostListener('window:beforeunload', ['$event']) notify($event: BeforeUnloadEvent) {
    if (this.editForm?.dirty) {
      $event.preventDefault();
    }
  }
  private accountService = inject(AccountService);
  protected memberService = inject(MemberService);
  private toast = inject(ToastService);
  protected editableMember: EditableMember = {
    displayName: '',
    description: '',
    city: '',
    country: ''
  }
  protected promptBank = signal<Prompt[]>([]);
  protected editablePrompts: { promptId: number | null; answer: string }[] = [];

  ngOnInit(): void {
    const member = this.memberService.member();
    this.editableMember = {
      displayName: member?.displayName || '',
      description: member?.description || '',
      city: member?.city || '',
      country: member?.country || '',
    }

    const existing = member?.promptAnswers ?? [];
    this.editablePrompts = [0, 1, 2].map(i => ({
      promptId: existing[i]?.promptId ?? null,
      answer: existing[i]?.answer ?? ''
    }));

    this.memberService.getPromptBank().subscribe(bank => this.promptBank.set(bank));
  }

  promptOptionsFor(index: number): Prompt[] {
    const chosenElsewhere = this.editablePrompts
      .filter((_, i) => i !== index)
      .map(slot => slot.promptId);
    return this.promptBank().filter(p => !chosenElsewhere.includes(p.id));
  }

  updateProfile() {
    if (!this.memberService.member()) return;
    const updatedMember = { ...this.memberService.member(), ...this.editableMember }
    this.memberService.updateMember(this.editableMember).subscribe({
      next: () => {
        const currentUser = this.accountService.currentUser();
        if (currentUser && updatedMember.displayName !== currentUser?.displayName) {
          currentUser.displayName = updatedMember.displayName;
          this.accountService.setCurrentUser(currentUser);
        }

        const filledPrompts = this.editablePrompts.filter(p => p.promptId && p.answer.trim());
        const savePrompts$ = filledPrompts.length > 0
          ? this.memberService.savePromptAnswers(filledPrompts as { promptId: number; answer: string }[])
          : null;

        const finish = (promptAnswers: Member['promptAnswers']) => {
          this.toast.success('Profile updated successfully');
          this.memberService.editMode.set(false);
          this.memberService.member.set({ ...updatedMember, promptAnswers } as Member);
          this.editForm?.reset(updatedMember);
        };

        if (savePrompts$) {
          savePrompts$.subscribe({
            next: () => this.memberService.getMember(updatedMember.id!).subscribe(m => finish(m.promptAnswers)),
            error: () => finish(this.memberService.member()?.promptAnswers)
          });
        } else {
          finish(this.memberService.member()?.promptAnswers);
        }
      }
    })

  }

  ngOnDestroy(): void {
    if (this.memberService.editMode()) {
      this.memberService.editMode.set(false);
    }
  }
}
