import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-photo-delete-button',
  imports: [],
  templateUrl: './photo-delete-button.html',
  styleUrl: './photo-delete-button.css'
})
export class PhotoDeleteButton {
  disabled = input<boolean>();
  clickEvent = output<Event>();

  onClick(event: Event) {
    this.clickEvent.emit(event);
  }
}
