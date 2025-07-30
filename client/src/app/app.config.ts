import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter, withViewTransitions } from '@angular/router';

import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { tap, timer } from 'rxjs';
import { InitService } from '../core/services/init-service';
import { routes } from './app.routes';
import { errorInterceptor } from '../core/interceptors/error-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes, withViewTransitions()),
    provideHttpClient(withInterceptors([errorInterceptor])),
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
