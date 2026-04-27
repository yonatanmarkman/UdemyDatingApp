import { Component, computed, input, model, output } from '@angular/core';

@Component({
  selector: 'app-paginator',
  imports: [],
  templateUrl: './paginator.html',
  styleUrl: './paginator.css'
})
export class Paginator {
  pageNumber = model(1);
  pageSize = model(10);
  totalCount = input(0);
  totalPages = input(0);

  
  defaultPageSize!: number;
  defaultPageSizeOptions = [5, 10, 20, 50];

  ngOnInit() {
    this.defaultPageSize = this.pageSize(); // Capture the initial value
  }

  pageSizeOptions = computed(() => 
    this.defaultPageSizeOptions.includes(this.defaultPageSize)
      ? this.defaultPageSizeOptions
      : [this.defaultPageSize, ...this.defaultPageSizeOptions]
  );

  pageChange = output<{pageNumber: number, pageSize: number}>();

  lastItemIndex = computed(() => {
    return Math.min(this.pageNumber() * this.pageSize(), this.totalCount())
  })

  lastPageNumber = computed(() => {
    return this.totalPages();
  })

  onPageChange(newPage?: number, pageSize?: EventTarget | null) {
    if (newPage) this.pageNumber.set(newPage);
    if (pageSize) {
      const size = Number((pageSize as HTMLSelectElement).value);
      this.pageSize.set(size);
    }

    this.pageChange.emit({
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize()
    })
  }
}
