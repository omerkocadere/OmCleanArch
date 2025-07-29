import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter, withViewTransitions } from '@angular/router';

import { provideHttpClient } from '@angular/common/http';
import { tap, timer } from 'rxjs';
import { InitService } from '../core/services/init-service';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes, withViewTransitions()),
    provideHttpClient(),
    provideAppInitializer(() => {
      const initService = inject(InitService);

      return timer(1000).pipe(
        tap(() => {
          initService.init();
          const splash = document.getElementById('initial-splash');
          if (splash) {
            splash.remove();
          }
        })
      );
    }),
  ],
};
