import { Pipe, PipeTransform, inject, ChangeDetectorRef } from '@angular/core';
import { TranslationService } from '../services/translation-service';

@Pipe({
  name: 'translate',
  standalone: true,
  pure: false // Pour que le pipe se mette à jour quand la langue change
})
export class TranslatePipe implements PipeTransform {
  private translationService = inject(TranslationService);
  private cdr = inject(ChangeDetectorRef);

  transform(key: string, params?: { [key: string]: string | number }): string {
    const result = this.translationService.translate(key, params);
    
    // Forcer la détection des changements si nécessaire
    if (result === key) {
      console.warn(`Translation not found for key: ${key}`);
    }
    
    return result;
  }
}
