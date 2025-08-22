import { Component, Input, signal } from '@angular/core';
import { Register } from "../account/register/register";
import { Member } from '../../types/user';

@Component({
  selector: 'app-home',
  imports: [Register],
  templateUrl: './home.html',
  styleUrl: './home.css'
})

export class Home {
  @Input({required: true}) membersFromApp: Member[] = [];

  protected registerMode = signal(false);

  showRegister() {
    this.registerMode.set(true);
  }
}
