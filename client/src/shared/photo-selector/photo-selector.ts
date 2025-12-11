import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-photo-selector',
  imports: [],
  templateUrl: './photo-selector.html',
  styleUrl: './photo-selector.css'
})
export class PhotoSelector {
  disabled = input<boolean>();
  selected = input<boolean>();
  clickEvent = output<Event>();

  onClick(event: Event) {
    this.clickEvent.emit(event);
  }
}
