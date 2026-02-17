import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import { format } from 'date-fns';
import Modal from '../components/Modal';
import { useToast, ToastContainer } from '../components/Toast';

interface Project {
    id?: number;
    name: string;
    category?: string;
    visibility: string;
    createdDate?: string;
    completedDate?: string;
}

const EMPTY: Project = { name: '', category: '', visibility: 'private', createdDate: '', completedDate: '' };

export default function ProjectsPage() {
    const qc = useQueryClient();
    const { toasts, toast, dismiss } = useToast();
    const [editing, setEditing] = useState<Project | null>(null);
    const [isNew, setIsNew] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<Project | null>(null);

    const { data: projects, isLoading } = useQuery({
        queryKey: ['projects'],
        queryFn: () => api.get('/projects').then((r) => r.data),
    });

    const save = useMutation({
        mutationFn: (p: Project) =>
            p.id ? api.put(`/projects/${p.id}`, p) : api.post('/projects', p),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['projects'] });
            toast(isNew ? 'Project created' : 'Project updated', 'success');
            setEditing(null);
        },
        onError: () => toast('Failed to save project', 'error'),
    });

    const del = useMutation({
        mutationFn: (id: number) => api.delete(`/projects/${id}`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['projects'] });
            toast('Project deleted', 'success');
            setConfirmDelete(null);
        },
        onError: () => toast('Failed to delete project', 'error'),
    });

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!editing) return;
        const fd = new FormData(e.currentTarget);
        save.mutate({
            ...editing,
            name: fd.get('name') as string,
            category: fd.get('category') as string || undefined,
            visibility: fd.get('visibility') as string,
            createdDate: fd.get('createdDate') as string || undefined,
            completedDate: fd.get('completedDate') as string || undefined,
        });
    };

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>Projects</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Organize work by project</p>
                </div>
                <button className="btn btn-primary" onClick={() => { setIsNew(true); setEditing({ ...EMPTY }); }}>
                    + New Project
                </button>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '1rem' }}>
                        {projects?.map((project: any) => (
                            <div key={project.id} className="card" style={{ padding: '1.25rem' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                                    <h3 style={{ fontWeight: 600, fontSize: '1rem', flex: 1 }}>{project.name}</h3>
                                    <span className="badge badge-primary" style={{ marginLeft: '0.5rem' }}>{project.category || 'General'}</span>
                                </div>
                                <div style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)', marginBottom: '0.75rem' }}>
                                    Created {format(new Date(project.createdAt), 'MMM d, yyyy')}
                                </div>
                                {project.completedDate && (
                                    <span className="badge badge-success" style={{ marginBottom: '0.75rem', display: 'inline-flex' }}>
                                        Completed {format(new Date(project.completedDate), 'MMM d, yyyy')}
                                    </span>
                                )}
                                <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
                                    <button className="btn btn-secondary" style={{ flex: 1, justifyContent: 'center', padding: '0.4rem', fontSize: '0.8rem' }}
                                        onClick={() => { setIsNew(false); setEditing(project); }}>Edit</button>
                                    <button className="btn btn-danger" style={{ padding: '0.4rem 0.8rem', fontSize: '0.8rem' }}
                                        onClick={() => setConfirmDelete(project)}>Del</button>
                                </div>
                            </div>
                        ))}
                        {(!projects || projects.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', gridColumn: '1 / -1', textAlign: 'center', padding: '2rem' }}>
                                No projects yet
                            </p>
                        )}
                    </div>
                )}
            </div>

            {editing && (
                <Modal title={isNew ? 'New Project' : 'Edit Project'} onClose={() => setEditing(null)}>
                    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Name *</label>
                            <input className="input" name="name" defaultValue={editing.name} required autoFocus />
                        </div>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Category</label>
                                <input className="input" name="category" defaultValue={editing.category ?? ''} placeholder="e.g. Work, Personal" />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Visibility</label>
                                <select className="input" name="visibility" defaultValue={editing.visibility}>
                                    <option value="private">Private</option>
                                    <option value="team">Team</option>
                                    <option value="public">Public</option>
                                </select>
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Start Date</label>
                                <input className="input" name="createdDate" type="date" defaultValue={editing.createdDate?.split('T')[0] ?? ''} />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Completed Date</label>
                                <input className="input" name="completedDate" type="date" defaultValue={editing.completedDate?.split('T')[0] ?? ''} />
                            </div>
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
                <Modal title="Delete Project" onClose={() => setConfirmDelete(null)} width={380}>
                    <p style={{ marginBottom: '1.5rem' }}>
                        Delete <strong>{confirmDelete.name}</strong>? Tasks linked to this project will lose their project association.
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
