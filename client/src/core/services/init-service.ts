import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);

  init(): void {
    const userString = localStorage.getItem('user');
    if (!userString) return;

    try {
      const user = JSON.parse(userString);
      this.accountService.currentUser.set(user);
    } catch (error) {
      console.error('Error parsing user data from localStorage:', error);
      localStorage.removeItem('user'); // Remove corrupted data
    }
  }
}
