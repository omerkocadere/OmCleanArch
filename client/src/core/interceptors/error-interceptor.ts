import {
  HttpErrorResponse,
  HttpEventType,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';
import { ProblemDetails, ValidationError } from '../../types/error';
import { ToastService } from '../services/toast-service';
import { AccountService } from '../services/account-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);
  const accountService = inject(AccountService);
  const startTime = Date.now();
  const requestId = generateRequestId();

  logRequest(req, requestId);

  return next(req).pipe(
    tap((response) => {
      // Only log when we get the actual HttpResponse (not intermediate events)
      if (response.type === HttpEventType.Response) {
        logResponse(req, requestId, startTime, undefined, response);
      }
    }),
    catchError((error: HttpErrorResponse) => {
      logResponse(req, requestId, startTime, error);

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

function generateRequestId(): string {
  return Math.random().toString(36).substring(2, 8).toUpperCase();
}

const logRequest = (req: HttpRequest<any>, requestId: string): void => {
  console.group(`🚀 HTTP Request [${requestId}] - ${req.method} ${req.url}`);
  console.log(`📍 Full Request:`, req);
};

const logResponse = (
  req: HttpRequest<any>,
  requestId: string,
  startTime: number,
  error?: HttpErrorResponse,
  responseData?: any
) => {
  const duration = Date.now() - startTime;
  if (error) {
    console.error(`❌ API Error: [${requestId}] - ${req.method} ${req.url} - ${duration}ms`, error);
  } else {
    console.log(
      `✅ API Success: [${requestId}] - ${req.method} ${req.url} - ${duration}ms`,
      responseData
    );
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
