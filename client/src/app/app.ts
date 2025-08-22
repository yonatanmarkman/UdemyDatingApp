import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { Nav } from '../layout/nav/nav';
import { AccountService } from '../core/services/account-service';
import { Member } from '../types/user';
import { Home } from "../features/home/home";

@Component({
  selector: 'app-root',
  imports: [Nav, Home],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App implements OnInit {
  private accountService = inject(AccountService);
  private http = inject(HttpClient);
  protected title = 'Udemy Dating App';
  protected members = signal<Member[]>([])

  async ngOnInit() {
    this.members.set(await this.getMembers());
    this.setCurrentUser();
  }

  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString)
      return;

    const user = JSON.parse(userString);
    this.accountService.currentUser.set(user);
  }

  private async loadMembers(): Promise<void> {
    try {
      const members = await this.getMembers();
      this.members.set(members);
    } catch (error) {
      console.error('Failed to load members:', error);
      // Handle error appropriately (show user message, etc.)
    }
  }

  private async getMembers(): Promise<Member[]> {
    try {
      return lastValueFrom(
        this.http.get<Member[]>('https://localhost:5201/api/members')
      );
    } catch (error) {
      console.log(error);
      throw error;
    }
  }
}
