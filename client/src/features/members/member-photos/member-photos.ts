import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { ActivatedRoute } from '@angular/router';
import { Photo } from '../../../types/member';
import { ImageUpload } from '../../../shared/image-upload/image-upload';
import { MemberPhotoSelector } from "../member-photo-selector/member-photo-selector";

@Component({
  selector: 'app-member-photos',
  imports: [ImageUpload, MemberPhotoSelector],
  templateUrl: './member-photos.html',
  styleUrl: './member-photos.css'
})
export class MemberPhotos implements OnInit {
  protected memberService = inject(MemberService);

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
      },
      error: error => {
        console.log('Error uploading image: ', error);
        this.loading.set(false);
      }
    });
  }

  arrayWithProfilePhotoAtTheFront() {
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
}
