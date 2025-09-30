import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private router = inject(Router);

  constructor() {
    this.createToastContainer();
  }

  private createToastContainer() {
    if (!document.getElementById('toast-container')) {
      const container = document.createElement('div');
      container.id = 'toast-container';
      container.className = 'toast toast-bottom toast-end z-50'
      document.body.appendChild(container)
    }
  }

  private createToastElement(message: string, alertClass: string, duration = 5000, 
      avatar?: string, route?: string) {
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) return;

    const toast = document.createElement('div');
    toast.classList.add('alert', alertClass, 'shadow-lg', 'flex', 
        'items-center', 'gap-3', 'cursor-pointer', 'mb-2', 'max-w-sm');

    // Ajouter une animation de pulsation pour les notifications de messages
    if (alertClass === 'alert-info' && message.includes('💬')) {
      toast.classList.add('pulse');
    }

    if (route) {
      toast.addEventListener('click', () => this.router.navigateByUrl(route))
    }

    toast.innerHTML = `
      ${avatar ? `<img src=${avatar || '/user.png'} class='w-10 h-10 rounded-full border-2 border-white'` : ''}
      <div class="flex-1">
        <div class="font-semibold text-sm">${message}</div>
        <div class="text-xs opacity-90">Cliquez pour voir le message</div>
      </div>
      <button class="ml-2 btn btn-sm btn-ghost text-white hover:bg-white hover:bg-opacity-20">×</button>
    `

    toast.querySelector('button')?.addEventListener('click', (e) => {
      e.stopPropagation();
      toastContainer.removeChild(toast);
    })

    toastContainer.append(toast);

    setTimeout(() => {
      if (toastContainer.contains(toast)) {
        toastContainer.removeChild(toast);
      }
    }, duration);
  }

  success(message: string, duration?: number, avatar?: string, route?: string) {
    this.createToastElement(message, 'alert-success', duration, avatar, route);
  }

  error(message: string, duration?: number, avatar?: string, route?: string) {
    this.createToastElement(message, 'alert-error', duration, avatar, route);
  }

  warning(message: string, duration?: number, avatar?: string, route?: string) {
    this.createToastElement(message, 'alert-warning', duration, avatar, route);
  }

  info(message: string, duration?: number, avatar?: string, route?: string) {
    this.createToastElement(message, 'alert-info', duration, avatar, route);
  }
}
