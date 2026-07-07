import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CurrentUserService {
  currentUser = signal<User | null>(null);

  setCurrentUser(user: User) {
    this.currentUser.set(user);
  }

  setCurrentUserToNull() {
    this.currentUser.set(null);
  }
}