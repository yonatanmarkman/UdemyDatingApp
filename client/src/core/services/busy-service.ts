import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  busyRequestCount = signal(0);

  addTask() {
    this.busyRequestCount.update(current => current + 1);
  }

  removeTask() {
    this.busyRequestCount.update(current => Math.max(0, current - 1));
  }
}
