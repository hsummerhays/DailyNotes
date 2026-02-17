import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import Modal from '../components/Modal';
import { useToast, ToastContainer } from '../components/Toast';

interface Tag {
    id?: number;
    name: string;
    color: string;
}

const EMPTY: Tag = { name: '', color: '#6366f1' };

const PRESET_COLORS = [
    '#6366f1', '#8b5cf6', '#ec4899', '#ef4444',
    '#f97316', '#eab308', '#22c55e', '#14b8a6',
    '#3b82f6', '#64748b',
];

export default function TagsPage() {
    const qc = useQueryClient();
    const { toasts, toast, dismiss } = useToast();
    const [editing, setEditing] = useState<Tag | null>(null);
    const [isNew, setIsNew] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<Tag | null>(null);

    const { data: tags, isLoading } = useQuery({
        queryKey: ['tags'],
        queryFn: () => api.get('/tags').then((r) => r.data),
    });

    const save = useMutation({
        mutationFn: (t: Tag) =>
            t.id ? api.put(`/tags/${t.id}`, t) : api.post('/tags', t),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['tags'] });
            toast(isNew ? 'Tag created' : 'Tag updated', 'success');
            setEditing(null);
        },
        onError: () => toast('Failed to save tag', 'error'),
    });

    const del = useMutation({
        mutationFn: (id: number) => api.delete(`/tags/${id}`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['tags'] });
            toast('Tag deleted', 'success');
            setConfirmDelete(null);
        },
        onError: () => toast('Failed to delete tag', 'error'),
    });

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!editing) return;
        const fd = new FormData(e.currentTarget);
        save.mutate({
            ...editing,
            name: fd.get('name') as string,
            color: fd.get('color') as string || '#6366f1',
        });
    };

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>Tags</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Manage your tags</p>
                </div>
                <button className="btn btn-primary" onClick={() => { setIsNew(true); setEditing({ ...EMPTY }); }}>
                    + New Tag
                </button>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.75rem' }}>
                        {tags?.map((tag: any) => (
                            <div key={tag.id} style={{
                                display: 'inline-flex',
                                alignItems: 'center',
                                gap: '0.5rem',
                                padding: '0.5rem 0.75rem',
                                background: 'var(--color-bg-card)',
                                border: '1px solid var(--color-border)',
                                borderRadius: '9999px',
                            }}>
                                <span style={{
                                    width: 12, height: 12, borderRadius: '50%',
                                    background: tag.color || '#6366f1', flexShrink: 0,
                                }} />
                                <span style={{ fontWeight: 500, fontSize: '0.875rem' }}>{tag.name}</span>
                                <button
                                    onClick={() => { setIsNew(false); setEditing(tag); }}
                                    style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-text-muted)', fontSize: '0.75rem', padding: '0 0.15rem' }}
                                    title="Edit"
                                >✎</button>
                                <button
                                    onClick={() => setConfirmDelete(tag)}
                                    style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-danger)', fontSize: '0.75rem', padding: '0 0.15rem' }}
                                    title="Delete"
                                >✕</button>
                            </div>
                        ))}
                        {(!tags || tags.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', width: '100%', textAlign: 'center', padding: '2rem' }}>
                                No tags yet
                            </p>
                        )}
                    </div>
                )}
            </div>

            {editing && (
                <Modal title={isNew ? 'New Tag' : 'Edit Tag'} onClose={() => setEditing(null)} width={380}>
                    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Name *</label>
                            <input className="input" name="name" defaultValue={editing.name} required autoFocus />
                        </div>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.5rem' }}>Color</label>
                            <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginBottom: '0.5rem' }}>
                                {PRESET_COLORS.map((c) => (
                                    <label key={c} style={{ cursor: 'pointer' }} title={c}>
                                        <input type="radio" name="color" value={c} defaultChecked={editing.color === c} style={{ display: 'none' }} />
                                        <span style={{
                                            display: 'block', width: 28, height: 28, borderRadius: '50%',
                                            background: c, border: editing.color === c ? '3px solid var(--color-text)' : '3px solid transparent',
                                            transition: 'border-color 0.1s',
                                        }} />
                                    </label>
                                ))}
                            </div>
                            <input className="input" name="color" type="color" defaultValue={editing.color || '#6366f1'}
                                style={{ width: '100%', height: 40, padding: '0.25rem', cursor: 'pointer' }} />
                        </div>
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
                <Modal title="Delete Tag" onClose={() => setConfirmDelete(null)} width={340}>
                    <p style={{ marginBottom: '1.5rem' }}>
                        Delete tag <strong>{confirmDelete.name}</strong>? This cannot be undone.
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
