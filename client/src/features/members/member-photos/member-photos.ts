import { Component, inject, OnInit, signal } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { ActivatedRoute } from '@angular/router';
import { Member, Photo } from '../../../types/member';
import { ImageUpload } from '../../../shared/image-upload/image-upload';
import { PhotoSelector } from "../../../shared/photo-selector/photo-selector";
import { PhotoDeleteButton } from "../../../shared/photo-delete-button/photo-delete-button";
import { AccountService } from '../../../core/services/account-service';
import { User } from '../../../types/user';

@Component({
  selector: 'app-member-photos',
  imports: [ImageUpload, PhotoSelector, PhotoDeleteButton],
  templateUrl: './member-photos.html',
  styleUrl: './member-photos.css'
})
export class MemberPhotos implements OnInit {
  protected memberService = inject(MemberService);
  protected accountService = inject(AccountService);

  private route = inject(ActivatedRoute);
  protected photos = signal<Photo[]>([]);
  protected loading = signal(false);

  ngOnInit(): void {
    const memberId = this.route.parent?.snapshot.paramMap.get('id');
    if (memberId) {
      this.memberService.getMemberPhotos(memberId).subscribe({
        next: photos => this.photos.set(photos)
      });
    }
  }

  onUploadImage(imageFile: File) {
    this.loading.set(true);
    this.memberService.uploadPhoto(imageFile).subscribe({
      next: photo => {
        this.memberService.editMode.set(false);
        this.loading.set(false);
        this.photos.update(photos => [...photos, photo]);
        // If member doesn't have a profile picture,
        // set the first photo to be his profile picture.
        if (!this.memberService.member()?.imageUrl) {
          this.updateProfilePhotoOnClientSideObjects(photo);
        }
      },
      error: error => {
        console.log('Error uploading image: ', error);
        this.loading.set(false);
      }
    });
  }

  obtainArrayWithProfilePhotoAtTheFront() {
    const array = this.photos();

    const profilePhoto = array
      .find(item => item.url === this.memberService.member()?.imageUrl);

    if (profilePhoto == null)
      return array;

    const otherPhotos = array
      .filter(item => item.id !== profilePhoto?.id)
      .sort((a, b) => a.id - b.id);

    return [profilePhoto, ...otherPhotos];
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo).subscribe({
      next: () => {
        this.updateProfilePhotoOnClientSideObjects(photo);
      }
    })
  }

  updateProfilePhotoOnClientSideObjects(photo: Photo) {
    const currentUser = this.accountService.currentUser();
    if (currentUser) {
      currentUser.imageUrl = photo.url;
    }
    this.accountService.setCurrentUser(currentUser as User);
    this.memberService.member.update(member => ({
      ...member,
      imageUrl: photo.url
    }) as Member);
  }

  isMainPhoto(photo: Photo) {
    return photo.url === this.memberService.member()?.imageUrl;
  }

  deletePhoto(photoId: number) {
    this.memberService.deletePhoto(photoId).subscribe({
      next: () => {
        this.photos.update(photos => photos.filter(x => x.id !== photoId))
      }
    });
  }
}
