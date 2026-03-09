import { useState, useMemo, useEffect, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import { formatDisplayDate } from '../lib/dateUtils';
import Modal from '../components/Modal';
import { useToast, ToastContainer } from '../components/Toast';
import { Link } from 'react-router-dom';
import { ExternalLink } from 'lucide-react';

import { TASK_STATUS, TASK_STATUS_OPTIONS, type TaskStatusFilter } from '../lib/taskStatus';

interface Task {
    id?: number;
    name: string;
    status: string;
    comments?: string;
    dueDate?: string;
    startDate?: string;
    projectId?: number | null;
    isPinned: boolean;
}

const EMPTY: Task = { name: '', status: 'pending', comments: '', dueDate: '', startDate: '', projectId: null, isPinned: false };

export default function TasksPage() {
    const qc = useQueryClient();
    const { toasts, toast, dismiss } = useToast();
    const [status, setStatus] = useState<TaskStatusFilter>(TASK_STATUS.pending);
    const hasAutoSwitched = useRef(false);
    const [editing, setEditing] = useState<Task | null>(null);
    const [isNew, setIsNew] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<Task | null>(null);

    const { data: allTasks, isLoading } = useQuery({
        queryKey: ['tasks'],
        queryFn: () => api.get('/work-tasks').then((r) => r.data),
    });

    const counts = useMemo(() => {
        const c: Record<string, number> = { all: allTasks?.length ?? 0 };
        allTasks?.forEach((t: any) => {
            c[t.status] = (c[t.status] || 0) + 1;
        });
        return c;
    }, [allTasks]);

    const tasks = useMemo(() => {
        if (status === 'all') return allTasks;
        return allTasks?.filter((t: any) => t.status === status);
    }, [allTasks, status]);

    useEffect(() => {
        if (!isLoading && allTasks && !hasAutoSwitched.current) {
            const pendingCount = allTasks.filter((t: any) => t.status === TASK_STATUS.pending).length;
            if (pendingCount === 0) {
                setStatus('all');
            }
            hasAutoSwitched.current = true;
        }
    }, [allTasks, isLoading]);

    const { data: projects } = useQuery({
        queryKey: ['projects'],
        queryFn: () => api.get('/projects').then((r) => r.data),
    });

    const save = useMutation({
        mutationFn: (t: Task) =>
            t.id ? api.put(`/work-tasks/${t.id}`, t) : api.post('/work-tasks', t),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['tasks'] });
            toast(isNew ? 'Task created' : 'Task updated', 'success');
            setEditing(null);
        },
        onError: () => toast('Failed to save task', 'error'),
    });

    const del = useMutation({
        mutationFn: (id: number) => api.delete(`/work-tasks/${id}`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['tasks'] });
            toast('Task deleted', 'success');
            setConfirmDelete(null);
        },
        onError: () => toast('Failed to delete task', 'error'),
    });

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!editing) return;
        const fd = new FormData(e.currentTarget);
        const projectId = fd.get('projectId');
        if (!projectId) {
            toast('A project is required', 'error');
            return;
        }
        save.mutate({
            ...editing,
            name: fd.get('name') as string,
            status: fd.get('status') as string,
            comments: fd.get('comments') as string || undefined,
            dueDate: fd.get('dueDate') as string || undefined,
            startDate: fd.get('startDate') as string || undefined,
            projectId: Number(projectId),
            isPinned: fd.get('isPinned') === 'on',
        });
    };

    const projectName = (id?: number | null) =>
        id ? projects?.find((p: any) => p.id === id)?.name ?? `#${id}` : null;

    // Fetch notes for the task being edited
    const { data: taskNotes, isLoading: isNotesLoading } = useQuery({
        queryKey: ['task-notes', editing?.id],
        queryFn: () => api.get(`/work-notes?taskId=${editing?.id}&pageSize=5`).then((r) => r.data),
        enabled: !!editing?.id,
    });

    const extractText = (content: unknown): string => {
        if (!content) return '';
        try {
            const getNodesText = (node: any): string => {
                let text = '';
                if (node?.text) text += node.text;
                if (node?.children && Array.isArray(node.children)) {
                    text += node.children.map(getNodesText).join(' ');
                }
                return text;
            };
            const root = (content as any).root || content;
            return getNodesText(root).trim();
        } catch (e) {
            return typeof content === 'string' ? content : 'Complex content...';
        }
    };

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>Tasks</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Manage your work tasks</p>
                </div>
                <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', alignItems: 'center' }}>
                    {TASK_STATUS_OPTIONS.map((s) => (
                        <button key={s} className={`btn ${status === s ? 'btn-primary' : 'btn-secondary'}`}
                            onClick={() => setStatus(s)}
                            style={{ padding: '0.4rem 0.8rem', fontSize: '0.8rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                            <span>{s === 'all' ? 'all' : s.replace('_', ' ')}</span>
                            <span style={{
                                backgroundColor: status === s ? 'rgba(255,255,255,0.2)' : 'rgba(0,0,0,0.05)',
                                padding: '0.1rem 0.4rem',
                                borderRadius: '4px',
                                fontSize: '0.7rem',
                                fontWeight: 600
                            }}>
                                {counts[s] ?? 0}
                            </span>
                        </button>
                    ))}
                    <button className="btn btn-primary" onClick={() => { setIsNew(true); setEditing({ ...EMPTY }); }}>
                        + New Task
                    </button>
                </div>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                        {tasks?.map((task: any) => (
                            <div key={task.id} className="card" style={{ padding: '1rem' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                    <div style={{ flex: 1 }}>
                                        <span style={{ fontWeight: 500 }}>{task.name}</span>
                                        {task.projectId && (
                                            <span style={{ fontSize: '0.75rem', color: 'var(--color-accent)', marginLeft: '0.75rem' }}>
                                                {projectName(task.projectId)}
                                            </span>
                                        )}
                                        {task.comments && (
                                            <p style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', marginTop: '0.25rem' }}>
                                                {task.comments}
                                            </p>
                                        )}
                                    </div>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                                        {task.dueDate && (
                                            <span style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)' }}>
                                                Due {formatDisplayDate(task.dueDate, 'MMM d')}
                                            </span>
                                        )}
                                        <span className={`badge ${task.status === TASK_STATUS.completed ? 'badge-success' :
                                            task.status === TASK_STATUS.inProgress ? 'badge-primary' : 'badge-warning'}`}>
                                            {task.status.replace('_', ' ')}
                                        </span>
                                        <button className="btn btn-secondary" style={{ padding: '0.3rem 0.7rem', fontSize: '0.75rem' }}
                                            onClick={() => { setIsNew(false); setEditing(task); }}>Edit</button>
                                        <button className="btn btn-danger" style={{ padding: '0.3rem 0.7rem', fontSize: '0.75rem' }}
                                            onClick={() => setConfirmDelete(task)}>Del</button>
                                    </div>
                                </div>
                            </div>
                        ))}
                        {(!tasks || tasks.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: '2rem' }}>No tasks found</p>
                        )}
                    </div>
                )}
            </div>

            {editing && (
                <Modal title={isNew ? 'New Task' : 'Edit Task'} onClose={() => setEditing(null)} width={isNew ? 500 : 800}>
                    <div style={{ display: 'grid', gridTemplateColumns: isNew ? '1fr' : '1fr 300px', gap: '2rem' }}>
                        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>
                                    Name *
                                </label>
                                <input className="input" name="name" defaultValue={editing.name} required autoFocus />
                            </div>
                            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                                <div>
                                    <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Status</label>
                                    <select className="input" name="status" defaultValue={editing.status}>
                                        <option value={TASK_STATUS.pending}>Pending</option>
                                        <option value={TASK_STATUS.inProgress}>In Progress</option>
                                        <option value={TASK_STATUS.completed}>Completed</option>
                                    </select>
                                </div>
                                <div>
                                    <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Project *</label>
                                    <select className="input" name="projectId" defaultValue={editing.projectId ?? ''} required>
                                        <option value="" disabled>Select a project…</option>
                                        {projects?.map((p: any) => (
                                            <option key={p.id} value={p.id}>{p.name}</option>
                                        ))}
                                    </select>
                                </div>
                                <div>
                                    <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Start Date</label>
                                    <input className="input" name="startDate" type="date" defaultValue={editing.startDate?.split('T')[0] ?? ''} />
                                </div>
                                <div>
                                    <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Due Date</label>
                                    <input className="input" name="dueDate" type="date" defaultValue={editing.dueDate?.split('T')[0] ?? ''} />
                                </div>
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Comments</label>
                                <textarea className="input" name="comments" rows={3} defaultValue={editing.comments ?? ''} style={{ resize: 'vertical' }} />
                            </div>
                            <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.875rem', cursor: 'pointer' }}>
                                <input type="checkbox" name="isPinned" defaultChecked={editing.isPinned} />
                                Pin this task
                            </label>
                            <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end', marginTop: '0.5rem' }}>
                                <button type="button" className="btn btn-secondary" onClick={() => setEditing(null)}>Cancel</button>
                                <button type="submit" className="btn btn-primary" disabled={save.isPending}>
                                    {save.isPending ? 'Saving...' : 'Save'}
                                </button>
                            </div>
                        </form>

                        {!isNew && (
                            <div style={{ borderLeft: '1px solid var(--color-border)', paddingLeft: '1.5rem', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                                <h3 style={{ fontSize: '0.875rem', fontWeight: 600, display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                                    Recent Activity
                                </h3>
                                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', maxHeight: '400px', overflowY: 'auto', paddingRight: '0.5rem' }}>
                                    {isNotesLoading ? (
                                        <div style={{ textAlign: 'center', padding: '1rem' }}><span className="spinner" /></div>
                                    ) : taskNotes?.length > 0 ? (
                                        taskNotes.map((n: any) => (
                                            <div key={n.id} style={{ padding: '0.75rem', background: 'rgba(0,0,0,0.1)', borderRadius: 'var(--radius-sm)', border: '1px solid var(--color-border)', position: 'relative' }}>
                                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.25rem' }}>
                                                    <div style={{ fontSize: '0.7rem', color: 'var(--color-primary-light)', fontWeight: 600 }}>
                                                        {formatDisplayDate(n.noteDate, 'MMM d, yyyy')}
                                                    </div>
                                                    <Link to={`/notes?taskId=${editing.id}&noteId=${n.id}`} style={{ color: 'var(--color-text-muted)', display: 'flex', alignItems: 'center' }} title="View in Notes">
                                                        <ExternalLink size={12} />
                                                    </Link>
                                                </div>
                                                <div style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: '-webkit-box', WebkitLineClamp: 3, WebkitBoxOrient: 'vertical', overflow: 'hidden', lineHeight: '1.4' }}>
                                                    {extractText(n.content)}
                                                </div>
                                            </div>
                                        ))
                                    ) : (
                                        <p style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)', textAlign: 'center', marginTop: '1rem' }}>
                                            No notes yet for this task.
                                        </p>
                                    )}
                                </div>
                            </div>
                        )}
                    </div>
                </Modal>
            )}

            {confirmDelete && (
                <Modal title="Delete Task" onClose={() => setConfirmDelete(null)} width={380}>
                    <p style={{ marginBottom: '1.5rem' }}>
                        Delete <strong>{confirmDelete.name}</strong>? This cannot be undone.
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
