import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastService } from '../../core/services/toast-service';
import { lightTheme, darkTheme } from '../theme';
import { BusyService } from '../../core/services/busy-service';
import { HasRole } from '../../shared/directives/has-role';
import { LanguageSelector } from '../../shared/language-selector/language-selector';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-nav',
  imports: [FormsModule, RouterLink, RouterLinkActive, HasRole, LanguageSelector, TranslatePipe],
  templateUrl: './nav.html',
  styleUrl: './nav.css'
})
export class Nav implements OnInit {
  protected accountService = inject(AccountService);
  protected busyService = inject(BusyService);
  private router = inject(Router);
  private toast = inject(ToastService);
  protected creds: any = {}
  private prefersDark = window.matchMedia?.('(prefers-color-scheme: dark)').matches;
  protected selectedTheme = signal<string>(localStorage.getItem('theme') || (this.prefersDark ? darkTheme : lightTheme));
  protected isDark = () => this.selectedTheme() === darkTheme;
  protected loading = signal(false);

  ngOnInit(): void {
    document.documentElement.setAttribute('data-theme', this.selectedTheme());
  }

  toggleTheme() {
    const next = this.isDark() ? lightTheme : darkTheme;
    this.selectedTheme.set(next);
    localStorage.setItem('theme', next);
    document.documentElement.setAttribute('data-theme', next);
  }

  handleSelectUserItem() {
    const elem = document.activeElement as HTMLDivElement;
    if (elem) elem.blur();
  }

  login() {
    this.loading.set(true);
    this.accountService.login(this.creds).subscribe({
      next: () => {
        this.router.navigateByUrl('/daily');
        this.toast.success('Logged in successfully');
        this.creds = {};
      },
      error: error => {
        this.toast.error(error.error);
      },
      complete: () => this.loading.set(false)
    })
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
