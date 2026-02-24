import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import { format } from 'date-fns';
import Modal from '../components/Modal';
import { useToast, ToastContainer } from '../components/Toast';

interface Note {
    id?: number;
    noteDate: string;
    content: unknown;
    workTaskId?: number | null;
    timeMinutes: number;
    isPinned: boolean;
    visibility: string;
}

const today = () => new Date().toISOString().split('T')[0];
const EMPTY: Note = { noteDate: today(), content: {}, workTaskId: null, timeMinutes: 0, isPinned: false, visibility: 'private' };

const extractText = (content: unknown): string => {
    if (!content) return '';
    if (typeof content === 'string') return content;
    const c = content as Record<string, unknown>;
    if (typeof c.text === 'string') return c.text;
    if (c.type === 'doc' && Array.isArray(c.content)) {
        return (c.content as unknown[]).map((node: unknown) => {
            const n = node as Record<string, unknown>;
            if (Array.isArray(n.content)) {
                return (n.content as unknown[]).map((t: unknown) => {
                    const tx = t as Record<string, unknown>;
                    return typeof tx.text === 'string' ? tx.text : '';
                }).join('');
            }
            return '';
        }).join('\n');
    }
    return '';
};

export default function NotesPage() {
    const qc = useQueryClient();
    const { toasts, toast, dismiss } = useToast();
    const [editing, setEditing] = useState<Note | null>(null);
    const [isNew, setIsNew] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<Note | null>(null);

    const { data: notes, isLoading } = useQuery({
        queryKey: ['notes'],
        queryFn: () => api.get('/work-notes').then((r) => r.data),
    });

    const { data: tasks } = useQuery({
        queryKey: ['tasks', 'all'],
        queryFn: () => api.get('/work-tasks').then((r) => r.data),
    });

    const taskMap = useMemo(() => {
        const map: Record<number, string> = {};
        tasks?.forEach((t: any) => { map[t.id] = t.name; });
        return map;
    }, [tasks]);

    const save = useMutation({
        mutationFn: (n: Note) =>
            n.id ? api.put(`/work-notes/${n.id}`, n) : api.post('/work-notes', n),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['notes'] });
            toast(isNew ? 'Note created' : 'Note updated', 'success');
            setEditing(null);
        },
        onError: () => toast('Failed to save note', 'error'),
    });

    const del = useMutation({
        mutationFn: (id: number) => api.delete(`/work-notes/${id}`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['notes'] });
            toast('Note deleted', 'success');
            setConfirmDelete(null);
        },
        onError: () => toast('Failed to delete note', 'error'),
    });

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!editing) return;
        const fd = new FormData(e.currentTarget);
        const taskId = fd.get('taskId');
        if (!taskId) {
            toast('A linked task is required', 'error');
            return;
        }
        save.mutate({
            ...editing,
            noteDate: fd.get('noteDate') as string,
            content: { text: fd.get('text') as string },
            workTaskId: Number(taskId),
            timeMinutes: Number(fd.get('timeMinutes')) || 0,
            isPinned: fd.get('isPinned') === 'on',
            visibility: fd.get('visibility') as string,
        });
    };

    const getTaskName = (id?: number | null) =>
        id ? taskMap[id] ?? `Task #${id}` : null;

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>Work Notes</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Daily work journal</p>
                </div>
                <button className="btn btn-primary" onClick={() => { setIsNew(true); setEditing({ ...EMPTY }); }}>
                    + New Note
                </button>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                        {notes?.map((note: any) => {
                            const text = extractText(note.content);
                            return (
                                <div key={note.id} className="card" style={{ padding: '1.25rem' }}>
                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                        <div style={{ flex: 1 }}>
                                            <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center', marginBottom: '0.5rem' }}>
                                                <span style={{ fontWeight: 600, fontSize: '0.9rem' }}>
                                                    {format(new Date(note.noteDate), 'MMM d, yyyy')}
                                                </span>
                                                {note.workTaskId && (
                                                    <span style={{ fontSize: '0.75rem', color: 'var(--color-accent)' }}>
                                                        {getTaskName(note.workTaskId)}
                                                    </span>
                                                )}
                                                {note.timeMinutes > 0 && (
                                                    <span className="badge badge-primary">{note.timeMinutes}m</span>
                                                )}
                                                {note.isPinned && <span className="badge badge-warning">Pinned</span>}
                                            </div>
                                            {text && (
                                                <p style={{ fontSize: '0.875rem', color: 'var(--color-text-secondary)', whiteSpace: 'pre-wrap' }}>
                                                    {text.length > 200 ? text.slice(0, 200) + '…' : text}
                                                </p>
                                            )}
                                        </div>
                                        <div style={{ display: 'flex', gap: '0.5rem', marginLeft: '1rem', flexShrink: 0 }}>
                                            <button className="btn btn-secondary" style={{ padding: '0.3rem 0.7rem', fontSize: '0.75rem' }}
                                                onClick={() => { setIsNew(false); setEditing(note); }}>Edit</button>
                                            <button className="btn btn-danger" style={{ padding: '0.3rem 0.7rem', fontSize: '0.75rem' }}
                                                onClick={() => setConfirmDelete(note)}>Del</button>
                                        </div>
                                    </div>
                                </div>
                            );
                        })}
                        {(!notes || notes.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: '2rem' }}>No notes yet</p>
                        )}
                    </div>
                )}
            </div>

            {editing && (
                <Modal title={isNew ? 'New Note' : 'Edit Note'} onClose={() => setEditing(null)} width={560}>
                    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Date *</label>
                                <input className="input" name="noteDate" type="date"
                                    defaultValue={typeof editing.noteDate === 'string' ? editing.noteDate.split('T')[0] : today()}
                                    required />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Linked Task *</label>
                                <select className="input" name="taskId" defaultValue={editing.workTaskId ?? ''} required>
                                    <option value="" disabled>Select a task…</option>
                                    {tasks?.map((t: any) => (
                                        <option key={t.id} value={t.id}>{t.name}</option>
                                    ))}
                                </select>
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Time (minutes)</label>
                                <input className="input" name="timeMinutes" type="number" min="0" defaultValue={editing.timeMinutes} />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Visibility</label>
                                <select className="input" name="visibility" defaultValue={editing.visibility}>
                                    <option value="private">Private</option>
                                    <option value="team">Team</option>
                                </select>
                            </div>
                        </div>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Content</label>
                            <textarea className="input" name="text" rows={8}
                                defaultValue={extractText(editing.content)}
                                placeholder="Write your note here..."
                                autoFocus
                                style={{ resize: 'vertical', fontFamily: 'inherit' }} />
                        </div>
                        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.875rem', cursor: 'pointer' }}>
                            <input type="checkbox" name="isPinned" defaultChecked={editing.isPinned} />
                            Pin this note
                        </label>
                        <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end', marginTop: '0.5rem' }}>
                            <button type="button" className="btn btn-secondary" onClick={() => setEditing(null)}>Cancel</button>
                            <button type="submit" className="btn btn-primary" disabled={save.isPending}>
                                {save.isPending ? 'Saving...' : 'Save'}
                            </button>
                        </div>
                    </form>
                </Modal>
            )}

            {confirmDelete && (
                <Modal title="Delete Note" onClose={() => setConfirmDelete(null)} width={380}>
                    <p style={{ marginBottom: '1.5rem' }}>
                        Delete note from <strong>{format(new Date((confirmDelete as any).noteDate), 'MMM d, yyyy')}</strong>? This cannot be undone.
                    </p>
                    <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
                        <button className="btn btn-secondary" onClick={() => setConfirmDelete(null)}>Cancel</button>
                        <button className="btn btn-danger" disabled={del.isPending}
                            onClick={() => del.mutate(confirmDelete.id!)}>
                            {del.isPending ? 'Deleting...' : 'Delete'}
                        </button>
                    </div>
                </Modal>
            )}

            <ToastContainer toasts={toasts} dismiss={dismiss} />
        </>
    );
}
