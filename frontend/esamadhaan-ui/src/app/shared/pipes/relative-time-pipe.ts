import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'relativeTime',
  standalone: true,
})
export class RelativeTimePipe implements PipeTransform {
  transform(value: string | Date | null | undefined): string {
    if (!value) {
      return 'Unknown';
    }

    let date: Date;
    
    if (typeof value === 'string') {
      // Backend stores dates in UTC (GETUTCDATE(), DateTime.UtcNow)
      // Ensure proper UTC parsing - if no timezone indicator, treat as UTC
      let dateString = value.trim();
      
      // Check if it already has timezone info (Z for UTC or +/- offset)
      const hasTimezone = dateString.endsWith('Z') || /[+-]\d{2}:?\d{2}$/.test(dateString);
      
      if (!hasTimezone && dateString.includes('T')) {
        // ISO format without timezone - append Z to indicate UTC
        // Handle milliseconds if present
        if (dateString.includes('.')) {
          const parts = dateString.split('.');
          dateString = parts[0] + 'Z';
        } else {
          dateString = dateString + 'Z';
        }
      }
      
      date = new Date(dateString);
    } else {
      date = value;
    }
    
    if (isNaN(date.getTime())) {
      return 'Invalid date';
    }

    // Calculate relative time using user's local timezone
    // The date from server is in UTC, JavaScript Date automatically converts to local timezone
    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    if (diffInSeconds < 60) {
      return 'Just now';
    }

    const diffInMinutes = Math.floor(diffInSeconds / 60);
    if (diffInMinutes < 60) {
      return `${diffInMinutes} minute${diffInMinutes === 1 ? '' : 's'} ago`;
    }

    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) {
      return `${diffInHours} hour${diffInHours === 1 ? '' : 's'} ago`;
    }

    const diffInDays = Math.floor(diffInHours / 24);
    if (diffInDays < 30) {
      return `${diffInDays} day${diffInDays === 1 ? '' : 's'} ago`;
    }

    const diffInMonths = Math.floor(diffInDays / 30);
    if (diffInMonths < 12) {
      return `${diffInMonths} month${diffInMonths === 1 ? '' : 's'} ago`;
    }

    const diffInYears = Math.floor(diffInMonths / 12);
    return `${diffInYears} year${diffInYears === 1 ? '' : 's'} ago`;
  }
}
