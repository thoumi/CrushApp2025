import { Component, inject, OnInit, signal } from '@angular/core';
import { MemberService } from '../../core/services/member-service';
import { Member } from '../../types/member';
import { MemberCard } from '../members/member-card/member-card';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-daily',
  imports: [MemberCard, TranslatePipe],
  templateUrl: './daily.html',
  styleUrl: './daily.css'
})
export class Daily implements OnInit {
  private memberService = inject(MemberService);
  protected members = signal<Member[] | null>(null);

  ngOnInit(): void {
    this.memberService.getDailySelection().subscribe(members => this.members.set(members));
  }
}
