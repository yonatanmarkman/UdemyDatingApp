import { Component, HostListener, inject, OnDestroy, OnInit, signal, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EditableMember, Member } from '../../../types/member';
import { DatePipe } from '@angular/common';
import { MemberService } from '../../../core/services/member-service';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastService } from '../../../core/services/toast-service';

@Component({
  selector: 'app-member-profile',
  imports: [DatePipe, FormsModule],
  templateUrl: './member-profile.html',
  styleUrl: './member-profile.css'
})
export class MemberProfile implements OnInit, OnDestroy {
  @ViewChild('editForm') editForm?: NgForm;
  @HostListener('window:beforeunload', ['$event']) notify($event:BeforeUnloadEvent) {
    if (this.editForm?.dirty) {
      $event.preventDefault();
    }
  }

  private toast = inject(ToastService);
  private route = inject(ActivatedRoute);

  protected member = signal<Member | undefined>(undefined);
  protected memberService = inject(MemberService);
  protected editableMember: EditableMember = {
    displayName: '',
    description: '',
    city: '',
    country: ''
  }

  constructor() {
    
  }

  ngOnInit(): void {
    this.route.parent?.data.subscribe(data => {
      this.member.set(data['member']);
    });
    
    this.editableMember = {
      displayName: this.member()?.displayName || '',
      description: this.member()?.description || '',
      city: this.member()?.city || '',
      country: this.member()?.country || '',
    }
  }

  updateProfile() {
    if (!this.member()) return;
    // Use the spread operator, to override the member() properties
    // with the updated editableMember's properties.
    const updatedMember = {...this.member(), ...this.editableMember};
    console.log(updatedMember);
    this.toast.success('Profile updated successfully');
    this.memberService.editMode.set(false);
  }

  ngOnDestroy(): void {
    if (this.memberService.editMode()) {
      this.memberService.editMode.set(false);
    }
  }
}
