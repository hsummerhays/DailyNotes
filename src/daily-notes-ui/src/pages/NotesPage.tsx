import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';
import { format } from 'date-fns';

export default function NotesPage() {
    const { data: notes, isLoading } = useQuery({
        queryKey: ['work-notes'],
        queryFn: () => api.get('/work-notes').then((r) => r.data),
    });

    return (
        <>
            <div className="page-header">
                <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>📝 Notes</h1>
                <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>All your work notes</p>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                        {notes?.map((note: any) => (
                            <div key={note.id} className="card" style={{ padding: '1rem' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                    <div>
                                        <span style={{ fontWeight: 500 }}>Note #{note.id}</span>
                                        {note.timeMinutes > 0 && (
                                            <span className="badge badge-primary" style={{ marginLeft: '0.75rem' }}>
                                                {note.timeMinutes} min
                                            </span>
                                        )}
                                    </div>
                                    <span style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)' }}>
                                        {note.noteDate ? format(new Date(note.noteDate), 'MMM d, yyyy') : '—'}
                                    </span>
                                </div>
                            </div>
                        ))}
                        {(!notes || notes.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: '2rem' }}>No notes yet</p>
                        )}
                    </div>
                )}
            </div>
        </>
    );
}
