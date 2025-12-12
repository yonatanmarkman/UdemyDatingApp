import { Component, inject, output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { RegisterCreds } from '../../../types/user';
import { JsonPipe } from '@angular/common';
import { TextInput } from "../../../shared/text-input/text-input";

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, JsonPipe, TextInput],
  templateUrl: './register.html',
  styleUrl: './register.css'
})

export class Register {
  private formBuilder = inject(FormBuilder);

  cancelRegister = output<boolean>();

  protected creds = {} as RegisterCreds;
  protected registerForm: FormGroup;

  constructor() {
    this.registerForm = this.formBuilder.group({
      email: ['', 
        [Validators.required, Validators.email]],
      displayName: ['', 
        [Validators.required]],
      password: ['', 
        [Validators.required, Validators.minLength(4), Validators.maxLength(16)]],
      confirmPassword: ['', 
        [Validators.required, this.matchValues('password')]]
    });
    this.registerForm.controls['password'].valueChanges.subscribe(() => {
      this.registerForm.controls['confirmPassword'].updateValueAndValidity();
    })
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const parent = control.parent;
      if (!parent)
        return null;

      const matchValue = parent.get(matchTo)?.value;
      return control.value === matchValue ? null : {passwordMismatch: true}
    }
  }

  register() {
    
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
