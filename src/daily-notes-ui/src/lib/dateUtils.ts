import { parseISO, format as dateFnsFormat } from 'date-fns';

/**
 * Safely parses a YYYY-MM-DD date string from the API and treats it as a local date
 * to avoid off-by-one timezone shift errors in the browser.
 */
export function parseLocalDate(dateStr: string | null | undefined): Date {
    if (!dateStr) return new Date();

    // If it's just a YYYY-MM-DD string, append T00:00:00 to force local-ish parsing
    // or better, split and use the constructor which is local-by-default.
    if (typeof dateStr === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(dateStr)) {
        const [year, month, day] = dateStr.split('-').map(Number);
        return new Date(year, month - 1, day);
    }

    // Fallback for full ISO strings or Date objects
    const date = typeof dateStr === 'string' ? parseISO(dateStr) : new Date(dateStr as any);
    return isNaN(date.getTime()) ? new Date() : date;
}

/**
 * Formats a date string from the API using the local parsing logic.
 */
export function formatDisplayDate(dateStr: string | null | undefined, formatStr: string = 'MMM d, yyyy'): string {
    if (!dateStr) return '—';
    try {
        return dateFnsFormat(parseLocalDate(dateStr), formatStr);
    } catch (e) {
        console.error('Date formatting error:', e, dateStr);
        return String(dateStr);
    }
}
