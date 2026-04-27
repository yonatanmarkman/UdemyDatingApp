import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../../types/member';
import { RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { LikesService } from '../../../core/services/likes-service';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css'
})
export class MemberCard {
  private likeService = inject(LikesService);
  member = input.required<Member>();
  protected hasLiked = computed(
    () => this.likeService.likeIds().includes(this.member().id)
  );

  toggleLike(event: Event) {
    event.stopPropagation();
    this.likeService.toggleLike(this.member().id).subscribe({
      next: () => {
        if (this.hasLiked()) {
          // Like has been toggled off - Remove this member's id from the MemberCard's ids list
          this.likeService.likeIds.update(ids => ids.filter(x => x !== this.member().id));
        } else {
          // Like has been toggled on - Update the MemberCard's ids list with this member's id
          this.likeService.likeIds.update(ids => [...ids, this.member().id]);
        }
      }
    })
  }
}
