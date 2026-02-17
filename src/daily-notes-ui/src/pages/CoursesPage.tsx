import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import Modal from '../components/Modal';
import { useToast, ToastContainer } from '../components/Toast';

interface Course {
    id?: number;
    name: string;
    semester?: string;
    instructor?: string;
    description?: string;
    credits: number;
    targetGrade?: number | null;
    currentGrade?: number | null;
    progressPercent: number;
    topicId?: number | null;
    isPinned: boolean;
}

const EMPTY: Course = {
    name: '', semester: '', instructor: '', description: '',
    credits: 3, targetGrade: null, currentGrade: null,
    progressPercent: 0, topicId: null, isPinned: false,
};

export default function CoursesPage() {
    const qc = useQueryClient();
    const { toasts, toast, dismiss } = useToast();
    const [semester, setSemester] = useState('');
    const [editing, setEditing] = useState<Course | null>(null);
    const [isNew, setIsNew] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<Course | null>(null);

    const { data: courses, isLoading } = useQuery({
        queryKey: ['courses', semester],
        queryFn: () => {
            const params = semester ? `?semester=${encodeURIComponent(semester)}` : '';
            return api.get(`/courses${params}`).then((r) => r.data);
        },
    });

    const { data: topics } = useQuery({
        queryKey: ['topics-root'],
        queryFn: () => api.get('/topics').then((r) => r.data),
    });

    const save = useMutation({
        mutationFn: (c: Course) =>
            c.id ? api.put(`/courses/${c.id}`, c) : api.post('/courses', c),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['courses'] });
            toast(isNew ? 'Course created' : 'Course updated', 'success');
            setEditing(null);
        },
        onError: () => toast('Failed to save course', 'error'),
    });

    const del = useMutation({
        mutationFn: (id: number) => api.delete(`/courses/${id}`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['courses'] });
            toast('Course deleted', 'success');
            setConfirmDelete(null);
        },
        onError: () => toast('Failed to delete course', 'error'),
    });

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!editing) return;
        const fd = new FormData(e.currentTarget);
        save.mutate({
            ...editing,
            name: fd.get('name') as string,
            semester: fd.get('semester') as string || undefined,
            instructor: fd.get('instructor') as string || undefined,
            description: fd.get('description') as string || undefined,
            credits: Number(fd.get('credits')) || 0,
            targetGrade: fd.get('targetGrade') ? Number(fd.get('targetGrade')) : null,
            currentGrade: fd.get('currentGrade') ? Number(fd.get('currentGrade')) : null,
            progressPercent: Number(fd.get('progressPercent')) || 0,
            topicId: fd.get('topicId') ? Number(fd.get('topicId')) : null,
            isPinned: fd.get('isPinned') === 'on',
        });
    };

    const gradeColor = (g: number) =>
        g >= 90 ? 'var(--color-success)' : g >= 70 ? 'var(--color-warning)' : 'var(--color-danger)';

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>Courses</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Track courses and grades</p>
                </div>
                <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                    <input
                        className="input"
                        type="text"
                        placeholder="Filter by semester..."
                        value={semester}
                        onChange={(e) => setSemester(e.target.value)}
                        style={{ width: 200 }}
                    />
                    <button className="btn btn-primary" onClick={() => { setIsNew(true); setEditing({ ...EMPTY }); }}>
                        + New Course
                    </button>
                </div>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1rem' }}>
                        {courses?.map((course: any) => (
                            <div key={course.id} className="card" style={{ padding: '1.25rem' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                                    <h3 style={{ fontWeight: 600, flex: 1, marginRight: '0.5rem' }}>{course.name}</h3>
                                    <span className="badge badge-primary">{course.credits ?? '—'} cr</span>
                                </div>
                                {course.instructor && (
                                    <div style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', marginBottom: '0.25rem' }}>
                                        {course.instructor}
                                    </div>
                                )}
                                {course.semester && (
                                    <div style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)', marginBottom: '0.75rem' }}>
                                        {course.semester}
                                    </div>
                                )}
                                {course.description && (
                                    <p style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', marginBottom: '0.75rem' }}>
                                        {course.description.length > 100 ? course.description.slice(0, 100) + '…' : course.description}
                                    </p>
                                )}
                                <div style={{ display: 'flex', gap: '1rem', alignItems: 'center', marginBottom: '0.75rem' }}>
                                    {course.currentGrade != null && (
                                        <span style={{ fontWeight: 600, color: gradeColor(course.currentGrade) }}>
                                            {course.currentGrade}%
                                        </span>
                                    )}
                                    {course.progressPercent != null && (
                                        <div style={{ flex: 1, display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                                            <div style={{ flex: 1, height: 6, background: 'var(--color-bg)', borderRadius: 3, overflow: 'hidden' }}>
                                                <div style={{
                                                    width: `${course.progressPercent}%`, height: '100%',
                                                    background: 'linear-gradient(90deg, var(--color-primary), var(--color-accent))',
                                                    borderRadius: 3,
                                                }} />
                                            </div>
                                            <span style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>{course.progressPercent}%</span>
                                        </div>
                                    )}
                                </div>
                                <div style={{ display: 'flex', gap: '0.5rem' }}>
                                    {course.isPinned && <span className="badge badge-warning" style={{ marginRight: 'auto' }}>Pinned</span>}
                                    <button className="btn btn-secondary" style={{ flex: course.isPinned ? undefined : 1, justifyContent: 'center', padding: '0.35rem 0.7rem', fontSize: '0.78rem' }}
                                        onClick={() => { setIsNew(false); setEditing(course); }}>Edit</button>
                                    <button className="btn btn-danger" style={{ padding: '0.35rem 0.7rem', fontSize: '0.78rem' }}
                                        onClick={() => setConfirmDelete(course)}>Del</button>
                                </div>
                            </div>
                        ))}
                        {(!courses || courses.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', gridColumn: '1 / -1', textAlign: 'center', padding: '2rem' }}>
                                No courses found
                            </p>
                        )}
                    </div>
                )}
            </div>

            {editing && (
                <Modal title={isNew ? 'New Course' : 'Edit Course'} onClose={() => setEditing(null)} width={520}>
                    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Name *</label>
                            <input className="input" name="name" defaultValue={editing.name} required autoFocus />
                        </div>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Semester</label>
                                <input className="input" name="semester" defaultValue={editing.semester ?? ''} placeholder="e.g. Fall 2025" />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Instructor</label>
                                <input className="input" name="instructor" defaultValue={editing.instructor ?? ''} />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Credits</label>
                                <input className="input" name="credits" type="number" min="0" max="20" defaultValue={editing.credits} />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Progress (%)</label>
                                <input className="input" name="progressPercent" type="number" min="0" max="100" defaultValue={editing.progressPercent} />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Target Grade (%)</label>
                                <input className="input" name="targetGrade" type="number" min="0" max="100" defaultValue={editing.targetGrade ?? ''} />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Current Grade (%)</label>
                                <input className="input" name="currentGrade" type="number" min="0" max="100" defaultValue={editing.currentGrade ?? ''} />
                            </div>
                        </div>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Description</label>
                            <textarea className="input" name="description" rows={3} defaultValue={editing.description ?? ''} style={{ resize: 'vertical' }} />
                        </div>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Linked Topic</label>
                            <select className="input" name="topicId" defaultValue={editing.topicId ?? ''}>
                                <option value="">None</option>
                                {topics?.map((t: any) => (
                                    <option key={t.id} value={t.id}>{t.title}</option>
                                ))}
                            </select>
                        </div>
                        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.875rem', cursor: 'pointer' }}>
                            <input type="checkbox" name="isPinned" defaultChecked={editing.isPinned} />
                            Pin this course
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
                <Modal title="Delete Course" onClose={() => setConfirmDelete(null)} width={380}>
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
