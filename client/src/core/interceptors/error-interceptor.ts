import {
  HttpErrorResponse,
  HttpEventType,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { catchError, tap } from 'rxjs';
import { ProblemDetails, ValidationError } from '../../types/error';
import { ToastService } from '../services/toast-service';
import { AccountService } from '../services/account-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);
  const accountService = inject(AccountService);

  logRequest(req);

  return next(req).pipe(
    tap((event) => {
      // Only log when we get the actual HttpResponse (not intermediate events)
      if (event.type === HttpEventType.Response) {
        // HttpEventType.Response
        logResponse(req, undefined, event);
      }
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
            toast.error('Your session has expired. Please login again.');
            accountService.logout();
            router.navigateByUrl('/');
            break;
          case 404:
            router.navigateByUrl('/not-found');
            break;
          case 500: {
            const navigationExtras: NavigationExtras = {
              state: { problemDetails: error.error as ProblemDetails },
            };
            router.navigateByUrl('/server-error', navigationExtras);
            break;
          }
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
