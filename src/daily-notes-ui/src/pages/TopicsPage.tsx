import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import Modal from '../components/Modal';
import { useToast, ToastContainer } from '../components/Toast';

interface Topic {
    id?: number;
    title: string;
    description?: string;
    proficiency?: string;
    parentTopicId?: number | null;
    isPinned: boolean;
}

const EMPTY: Topic = { title: '', description: '', proficiency: '', parentTopicId: null, isPinned: false };
const PROFICIENCY = ['', 'beginner', 'intermediate', 'advanced', 'expert'];

export default function TopicsPage() {
    const qc = useQueryClient();
    const { toasts, toast, dismiss } = useToast();
    const [editing, setEditing] = useState<Topic | null>(null);
    const [isNew, setIsNew] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<Topic | null>(null);

    const { data: topics, isLoading } = useQuery({
        queryKey: ['topics-root'],
        queryFn: () => api.get('/topics').then((r) => r.data),
    });

    const save = useMutation({
        mutationFn: (t: Topic) =>
            t.id ? api.put(`/topics/${t.id}`, t) : api.post('/topics', t),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['topics-root'] });
            toast(isNew ? 'Topic created' : 'Topic updated', 'success');
            setEditing(null);
        },
        onError: () => toast('Failed to save topic', 'error'),
    });

    const del = useMutation({
        mutationFn: (id: number) => api.delete(`/topics/${id}`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['topics-root'] });
            toast('Topic deleted', 'success');
            setConfirmDelete(null);
        },
        onError: () => toast('Failed to delete topic', 'error'),
    });

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!editing) return;
        const fd = new FormData(e.currentTarget);
        save.mutate({
            ...editing,
            title: fd.get('title') as string,
            description: fd.get('description') as string || undefined,
            proficiency: fd.get('proficiency') as string || undefined,
            parentTopicId: fd.get('parentTopicId') ? Number(fd.get('parentTopicId')) : null,
            isPinned: fd.get('isPinned') === 'on',
        });
    };

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>Knowledge Base</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Explore and manage topics</p>
                </div>
                <button className="btn btn-primary" onClick={() => { setIsNew(true); setEditing({ ...EMPTY }); }}>
                    + New Topic
                </button>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', gap: '1rem' }}>
                        {topics?.map((topic: any) => (
                            <div key={topic.id} className="card" style={{ padding: '1.25rem' }}>
                                <h3 style={{ fontWeight: 600, marginBottom: '0.5rem' }}>{topic.title}</h3>
                                {topic.description && (
                                    <p style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', marginBottom: '0.75rem' }}>
                                        {topic.description}
                                    </p>
                                )}
                                <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginBottom: '0.75rem' }}>
                                    {topic.proficiency && (
                                        <span className="badge badge-primary">{topic.proficiency}</span>
                                    )}
                                    {topic.isPinned && (
                                        <span className="badge badge-warning">Pinned</span>
                                    )}
                                </div>
                                <div style={{ display: 'flex', gap: '0.5rem' }}>
                                    <button className="btn btn-secondary" style={{ flex: 1, justifyContent: 'center', padding: '0.35rem', fontSize: '0.78rem' }}
                                        onClick={() => { setIsNew(false); setEditing(topic); }}>Edit</button>
                                    <button className="btn btn-danger" style={{ padding: '0.35rem 0.7rem', fontSize: '0.78rem' }}
                                        onClick={() => setConfirmDelete(topic)}>Del</button>
                                </div>
                            </div>
                        ))}
                        {(!topics || topics.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', gridColumn: '1 / -1', textAlign: 'center', padding: '2rem' }}>
                                No topics yet
                            </p>
                        )}
                    </div>
                )}
            </div>

            {editing && (
                <Modal title={isNew ? 'New Topic' : 'Edit Topic'} onClose={() => setEditing(null)}>
                    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Title *</label>
                            <input className="input" name="title" defaultValue={editing.title} required autoFocus />
                        </div>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Description</label>
                            <textarea className="input" name="description" rows={3} defaultValue={editing.description ?? ''} style={{ resize: 'vertical' }} />
                        </div>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Proficiency</label>
                                <select className="input" name="proficiency" defaultValue={editing.proficiency ?? ''}>
                                    {PROFICIENCY.map((p) => <option key={p} value={p}>{p || 'None'}</option>)}
                                </select>
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Parent Topic</label>
                                <select className="input" name="parentTopicId" defaultValue={editing.parentTopicId ?? ''}>
                                    <option value="">None (root)</option>
                                    {topics?.filter((t: any) => t.id !== editing.id).map((t: any) => (
                                        <option key={t.id} value={t.id}>{t.title}</option>
                                    ))}
                                </select>
                            </div>
                        </div>
                        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.875rem', cursor: 'pointer' }}>
                            <input type="checkbox" name="isPinned" defaultChecked={editing.isPinned} />
                            Pin this topic
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
                <Modal title="Delete Topic" onClose={() => setConfirmDelete(null)} width={380}>
                    <p style={{ marginBottom: '1.5rem' }}>
                        Delete <strong>{confirmDelete.title}</strong>? Child topics and notes linked to this topic may be affected.
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
