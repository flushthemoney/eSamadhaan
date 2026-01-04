import { AbstractControl, ValidationErrors } from '@angular/forms';

export function fileValidator(control: AbstractControl): ValidationErrors | null {
  const file = control.value as File | null;
  if (!file) return null;

  const maxSize = 5 * 1024 * 1024; // 5MB
  if (file.size > maxSize) {
    return { fileSize: true };
  }

  const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf'];
  if (!allowedTypes.includes(file.type)) {
    return { fileType: true };
  }

  return null;
}

