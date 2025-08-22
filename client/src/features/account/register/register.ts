import { Component, Input, input, output, signal, Signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Member, RegisterCreds } from '../../../types/user';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})

export class Register {
  membersFromHome = input.required<Member[]>();
  cancelRegister = output<boolean>();

  protected creds = {} as RegisterCreds;

  register() {
    console.log(this.creds);
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
