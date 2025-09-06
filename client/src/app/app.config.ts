import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter, withViewTransitions } from '@angular/router';

import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { timer, switchMap, tap, catchError, of } from 'rxjs';
import { errorInterceptor } from '../core/interceptors/error-interceptor';
import { jwtInterceptor } from '../core/interceptors/jwt-interceptor';
import { loadingInterceptor } from '../core/interceptors/loading-interceptor';
import { InitService } from '../core/services/init-service';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes, withViewTransitions()),
    provideHttpClient(withInterceptors([errorInterceptor, jwtInterceptor, loadingInterceptor])),
    provideAppInitializer(() => {
      const initService = inject(InitService);

      return timer(500).pipe(
        switchMap(() => initService.init()),
        catchError((error) => {
          console.error('Failed to initialize app:', error);
          return of(null); // Continue even if init fails
        }),
        tap(() => {
          const splash = document.getElementById('initial-splash');
          if (splash) {
            splash.remove();
          }
        })
      );
    }),
  ],
};
