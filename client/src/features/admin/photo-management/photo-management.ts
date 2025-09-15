import { Component, inject, OnInit, signal } from '@angular/core';
import { Photo } from '../../../types/member';
import { AdminService } from '../../../core/services/admin-service';

@Component({
  selector: 'app-photo-management',
  imports: [],
  templateUrl: './photo-management.html',
  styleUrl: './photo-management.css',
})
export class PhotoManagement implements OnInit {
  photos = signal<Photo[]>([]);
  private readonly adminService = inject(AdminService);

  ngOnInit(): void {
    this.getPhotosForApproval();
  }

  getPhotosForApproval() {
    this.adminService.getPhotosForApproval().subscribe({
      next: (photos) => this.photos.set(photos),
    });
  }

  approvePhoto(photoId: string) {
    this.adminService.approvePhoto(photoId).subscribe({
      next: () =>
        this.photos.update((photos) => {
          return photos.filter((x) => x.id !== photoId);
        }),
    });
  }

  rejectPhoto(photoId: string) {
    this.adminService.rejectPhoto(photoId).subscribe({
      next: () =>
        this.photos.update((photos) => {
          return photos.filter((x) => x.id !== photoId);
        }),
    });
  }
}
