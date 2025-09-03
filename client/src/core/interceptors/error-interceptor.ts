import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError } from 'rxjs';
import { ToastService } from '../services/toast-service';
import { NavigationExtras, Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);

  return next(req).pipe(
    catchError(error => {
      if (error) {
        switch (error.status) {
          case 400:
            var errorObject = error.error;
            var errorObjectArray = errorObject.errors;
            if (errorObjectArray) {
              const modelStateErrors = [];
              for (const key in errorObjectArray) {
                if (errorObjectArray[key]) {
                  modelStateErrors.push(errorObjectArray[key]);
                }
              }
              throw modelStateErrors.flat();
            } else {
              toast.error(errorObject);
            }
            break;
          case 401:
            toast.error('Unauthorized');
            break;
          case 404:
            router.navigateByUrl('/not-found');
            break;
          case 500:
            var errorObject = error.error;
            const navigationExtras: NavigationExtras = {state: {error: errorObject}};
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
