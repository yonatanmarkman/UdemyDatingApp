import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { lastValueFrom } from 'rxjs';

interface Member {
  id: number;
  displayName: string;
  email: string;
}

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App implements OnInit {
  private http = inject(HttpClient);
  protected title = 'Udemy Dating App';
  protected members = signal<Member[]>([])

  async ngOnInit() {
    await this.loadMembers();
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
