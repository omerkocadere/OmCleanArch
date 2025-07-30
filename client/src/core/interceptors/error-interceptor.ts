import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError } from 'rxjs';
import { ToastService } from '../services/toast-service';
import { NavigationExtras, Router } from '@angular/router';

// Centralized logging utility
const logRequest = (args: string | FetchArgs) => {
  const url = typeof args === 'string' ? args : args.url;
  const method = typeof args === 'string' ? 'GET' : args.method || 'GET';
  console.log(`🚀 API Request: ${method} ${API_BASE_URL}/${url}`);
};

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);

  logRequest(req);

  return next(req).pipe(
    catchError((error) => {
      if (error) {
        switch (error.status) {
          case 400:
            toast.error(error.error.detail);
            // if (error.error.errors) {
            //   const modelStateErrors = [];
            //   for (const key in error.error.errors) {
            //     if (error.error.errors[key]) {
            //       modelStateErrors.push(error.error.errors[key])
            //     }
            //   }
            //   throw modelStateErrors.flat();
            // } else {
            //   toast.error(error.error)
            // }
            break;
          case 401:
            toast.error('Unauthorized');
            break;
          case 404:
            router.navigateByUrl('/not-found');
            break;
          case 500:
            const navigationExtras: NavigationExtras = { state: { error: error.error } };
            router.navigateByUrl('/server-error', navigationExtras);
            break;
          default:
            toast.error('Something went wrong');
            break;
        }
      }

      throw error;
    })
  );
};

export interface ValidationError {
  code: string;
  description: string;
  type: string;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: ValidationError[]; // Validation errors array
  [key: string]: unknown; // Allow additional properties
}
