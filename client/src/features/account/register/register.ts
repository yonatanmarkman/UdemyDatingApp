import { Component, Input, input, signal, Signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Member, RegisterCreds } from '../../../types/user';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})

export class Register {
  public membersFromHome = input.required<Member[]>();
  
  protected creds = {} as RegisterCreds;

  register() {
    console.log(this.creds);
  }

  cancel() {
    console.log('Registration cancelled. ');
  }
}
