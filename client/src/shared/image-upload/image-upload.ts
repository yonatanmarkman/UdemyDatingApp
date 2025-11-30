import { Component, ElementRef, input, output, signal, ViewChild } from '@angular/core';

@Component({
  selector: 'app-image-upload',
  imports: [],
  templateUrl: './image-upload.html',
  styleUrl: './image-upload.css'
})
export class ImageUpload {
  protected imageSource = signal<string | ArrayBuffer | null | undefined>(null);
  protected isDragging = false;
  private fileToUpload: File | null = null;
  uploadFile = output<File>();
  loading = input<boolean>(false);

  @ViewChild('imageInput') imageInput!: ElementRef<HTMLInputElement>;
  
  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    if (event.dataTransfer?.files.length) {
      // Allow only one image upload at a time
      const imageFile = event.dataTransfer.files[0];
      this.previewImage(imageFile);
      this.fileToUpload = imageFile;
    }
  }

  onChange() {
    this.isDragging = false;
    const files = this.imageInput.nativeElement.files;

    if (files?.length === 1) {
      // Allow only one image upload at a time
      const imageFile = files[0];
      this.previewImage(imageFile);
      this.fileToUpload = imageFile;
    }

    // Clean the 'memory' of the element
    this.imageInput.nativeElement.value = '';
  }

  onCancel() {
    this.fileToUpload = null;
    this.imageSource.set(null);
  }

  onUploadFile() {
    if (this.fileToUpload) {
      this.uploadFile.emit(this.fileToUpload);
    }
  }

  private previewImage(imageFile: File) {
    const reader = new FileReader();
    reader.onload = (e) => {
      this.imageSource.set(e.target?.result);
    };
    reader.readAsDataURL(imageFile);
  }
}
