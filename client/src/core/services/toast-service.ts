import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';

type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastOptions {
  duration?: number;
  avatar?: string;
  route?: string;
}

@Injectable({
  providedIn: 'root'
})

/*
 * Important note:
 * This manual and 'old-style' code was written as part of the course, 
 * in order to refrain from using a JavaScript library which updates every six months and breaks the project.
 * This would never be a solution that I'd consider in a real world setting.
 */
export class ToastService {
  private router = inject(Router);
  
  constructor() { 
    this.createToastContainer();
  }

  private createToastContainer() {
    if (!document.getElementById('toast-container')) {
      const container = document.createElement('div');
      container.id = 'toast-container';
      container.className = 'toast toast-bottom toast-end z-50';
      document.body.appendChild(container);
    }
  }

  private createToastElement(message: string, alertClass: string, duration = 5000, 
      avatar?: string, route?: string) {

    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) return;

    const toast = document.createElement('div');
    toast.classList.add('alert', alertClass, 'shadow-lg', 'flex', 
        'items-center', 'gap-3', 'cursor-pointer');
    
    if (route) {
      toast.addEventListener('click', () => this.router.navigateByUrl(route));
    }
    
    // Add an image to the toast, when it exists.
    toast.innerHTML = `
      <img src="${avatar || '/user.png'}" class='w-10 h-10 rounded'> 
      <span>${message}</span>
      <button class="ml-4 btn btn-sm btn-ghost">&times;</button>
    `

    toast.querySelector('button')?.addEventListener('click', () => {
      toastContainer.removeChild(toast);
    });

    toastContainer.append(toast);

    setTimeout(() => {
      if (toastContainer.contains(toast)) {
        toastContainer.removeChild(toast);
      }
    }, duration);
  }

  success(message: string, options?: ToastOptions) {
    this.show('success', message, options); 
  }

  error(message: string, options?: ToastOptions) {
    this.show('error', message, options); 
  }

  warning(message: string, options?: ToastOptions) { 
    this.show('warning', message, options); 
  }

  info(message: string, options?: ToastOptions) { 
    this.show('info', message, options);
  }

  private show(type: ToastType, message: string, { duration, avatar, route }: ToastOptions = {}) {
    this.createToastElement(message, `alert-${type}`, duration, avatar, route);
  }
}
