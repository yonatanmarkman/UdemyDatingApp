import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { Member, Photo } from '../../../types/member';
import { AccountService } from '../../../core/services/account-service';
import { User } from '../../../types/user';

@Component({
  selector: 'app-member-photo-selector',
  imports: [],
  templateUrl: './member-photo-selector.html',
  styleUrl: './member-photo-selector.css'
})
export class MemberPhotoSelector {
  @Input() photoFile!: Photo;

  protected memberService = inject(MemberService);
  private accountService = inject(AccountService);

  setMainPhoto() {
    this.memberService.setMainPhoto(this.photoFile).subscribe({
      next: () => {
        const currentUser = this.accountService.currentUser();
        if (currentUser) {
          currentUser.imageUrl = this.photoFile.url;
        }
        this.accountService.setCurrentUser(currentUser as User);
        this.memberService.member.update(member => ({
          ...member,
          imageUrl: this.photoFile.url
        }) as Member);
      }
    })
  }

  isMainPhoto() {
    return this.photoFile.url === this.memberService.member()?.imageUrl;
  }
}
