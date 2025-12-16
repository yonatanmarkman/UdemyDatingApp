import { Component, ElementRef, input, output, signal, ViewChild } from '@angular/core';

@Component({
  selector: 'app-image-upload',
  imports: [],
  templateUrl: './image-upload.html',
  styleUrl: './image-upload.css'
})
export class ImageUpload {
  @ViewChild('imageInput') imageInput!: ElementRef<HTMLInputElement>;

  protected errorMessage = signal<string | null>(null);
  protected imageSource = signal<string | ArrayBuffer | null | undefined>(null);
  protected isDragging = false;
  private fileToUpload: File | null = null;
  uploadedFileOutput = output<File>();
  loading = input<boolean>(false);


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
      this.validateAndSetImage(imageFile);
    }
  }

  onChange() {
    this.isDragging = false;
    const files = this.imageInput.nativeElement.files;

    if (files?.length === 1) {
      // Allow only one image upload at a time
      const imageFile = files[0];
      this.validateAndSetImage(imageFile);
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
      this.uploadedFileOutput.emit(this.fileToUpload);
    }
  }

  private validateAndSetImage(imageFile: File) {
    this.errorMessage.set(null);
    if (this.isValidImageFile(imageFile)) {
      this.previewImage(imageFile);
      this.fileToUpload = imageFile;
    }
    else {
      this.errorMessage.set('Please upload a valid image file');
    }
  }

  private previewImage(imageFile: File) {
    const reader = new FileReader();
    reader.onload = (e) => {
      this.imageSource.set(e.target?.result);
    };
    reader.readAsDataURL(imageFile);
  }

  private isValidImageFile(file: File): boolean {
    return file.type.startsWith('image/') && file.type !== 'image/svg+xml';
  }
}
