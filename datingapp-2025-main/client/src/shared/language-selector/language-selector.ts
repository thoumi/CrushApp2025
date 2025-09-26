import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslationService } from '../../core/services/translation-service';

@Component({
  selector: 'app-language-selector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './language-selector.html',
  styleUrls: ['./language-selector.css']
})
export class LanguageSelector {
  protected translationService = inject(TranslationService);
  protected availableLanguages = signal<string[]>([]);

  constructor() {
    this.availableLanguages.set(this.translationService.getAvailableLanguages());
  }

  changeLanguage(language: string): void {
    this.translationService.setLanguage(language);
  }
}
