import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { catchError, tap } from 'rxjs';
import { ToastService } from '../services/toast-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);

  logRequest(req);

  return next(req).pipe(
    tap((response) => {
      logResponse(req, undefined, response);
    }),
    catchError((error: HttpErrorResponse) => {
      logResponse(req, error);

      if (error) {
        const errorMessage = getErrorMessage(error);

        switch (error.status) {
          case 400:
            toast.error(errorMessage);
            break;
          case 401:
            toast.error(errorMessage);
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

const logRequest = (req: HttpRequest<any>) => {
  console.log(`🚀 API Request: ${req.method} ${req.url}`);
};

const logResponse = (req: HttpRequest<any>, error?: HttpErrorResponse, responseData?: any) => {
  if (error) {
    console.error(`❌ API Error: ${req.method} ${req.url}`, error);
  } else {
    console.log(`✅ API Success: ${req.method} ${req.url}`, responseData);
  }
};

const getErrorMessage = (error: HttpErrorResponse): string => {
  // Check if error.error matches ProblemDetails structure
  const problemDetails = error.error as ProblemDetails;

  // If there are validation errors, return them as a formatted string
  if (problemDetails?.errors && problemDetails.errors.length > 0) {
    return problemDetails.errors
      .map((err: ValidationError) => err.description || err.code)
      .join(', ');
  }

  if (problemDetails?.detail) {
    return problemDetails.detail;
  }
  if (problemDetails?.title) {
    return problemDetails.title;
  }

  return 'Request failed, no details available';
};

interface ValidationError {
  code: string;
  description: string;
  type: string;
}

interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: ValidationError[]; // Validation errors array
  [key: string]: unknown; // Allow additional properties
}
