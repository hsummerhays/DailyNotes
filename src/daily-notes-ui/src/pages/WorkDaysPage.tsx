import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import { format, startOfMonth, endOfMonth, subMonths, startOfYear, endOfYear } from 'date-fns';
import Modal from '../components/Modal';
import { useToast, ToastContainer } from '../components/Toast';

type FilterMode = 'day' | 'month' | 'last_month' | 'year' | 'all';

interface WorkDay {
    id?: number;
    workDate: string;
    timeIn1?: string;
    timeOut1?: string;
    timeIn2?: string;
    timeOut2?: string;
    breakMinutes: number;
    comments?: string;
}

const todayStr = () => new Date().toISOString().split('T')[0];
const EMPTY: WorkDay = { workDate: todayStr(), timeIn1: '', timeOut1: '', timeIn2: '', timeOut2: '', breakMinutes: 0, comments: '' };

export default function WorkDaysPage() {
    const queryClient = useQueryClient();
    const { toasts, toast, dismiss } = useToast();
    const [filterMode, setFilterMode] = useState<FilterMode>('month');
    const [selectedDate, setSelectedDate] = useState(new Date());
    const [editing, setEditing] = useState<WorkDay | null>(null);
    const [isNew, setIsNew] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<WorkDay | null>(null);

    const { data: workDays, isLoading } = useQuery({
        queryKey: ['work-days', filterMode, format(selectedDate, 'yyyy-MM-dd')],
        queryFn: async () => {
            if (filterMode === 'day') {
                return api.get(`/work-days?date=${format(selectedDate, 'yyyy-MM-dd')}`).then(r => r.data);
            }
            if (filterMode === 'all') {
                return api.get(`/work-days?all=true`).then(r => r.data);
            }

            let from, to;
            if (filterMode === 'month') {
                from = startOfMonth(selectedDate);
                to = endOfMonth(selectedDate);
            } else if (filterMode === 'last_month') {
                const lastMonth = subMonths(new Date(), 1);
                from = startOfMonth(lastMonth);
                to = endOfMonth(lastMonth);
            } else if (filterMode === 'year') {
                from = startOfYear(selectedDate);
                to = endOfYear(selectedDate);
            }

            if (from && to) {
                return api.get(`/work-days?from=${format(from, 'yyyy-MM-dd')}&to=${format(to, 'yyyy-MM-dd')}`).then(r => r.data);
            }
            return [];
        },
    });

    const { data: todayData } = useQuery({
        queryKey: ['work-day-today'],
        queryFn: () => api.get('/work-days/today').then((r) => r.data).catch(() => null),
        retry: false,
    });

    const clockIn = useMutation({
        mutationFn: () => api.post('/work-days', {
            workDate: todayStr(),
            timeIn1: format(new Date(), 'HH:mm:ss'),
            breakMinutes: 0,
        }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['work-day-today'] });
            queryClient.invalidateQueries({ queryKey: ['work-days'] });
            toast('Clocked in', 'success');
        },
        onError: () => toast('Failed to clock in', 'error'),
    });

    const save = useMutation({
        mutationFn: (wd: WorkDay) =>
            wd.id ? api.put(`/work-days/${wd.id}`, wd) : api.post('/work-days', wd),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['work-days'] });
            queryClient.invalidateQueries({ queryKey: ['work-day-today'] });
            toast(isNew ? 'Work day created' : 'Work day updated', 'success');
            setEditing(null);
        },
        onError: () => toast('Failed to save work day', 'error'),
    });

    const del = useMutation({
        mutationFn: (id: number) => api.delete(`/work-days/${id}`),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['work-days'] });
            queryClient.invalidateQueries({ queryKey: ['work-day-today'] });
            toast('Work day deleted', 'success');
            setConfirmDelete(null);
        },
        onError: () => toast('Failed to delete work day', 'error'),
    });

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!editing) return;
        const fd = new FormData(e.currentTarget);
        save.mutate({
            ...editing,
            workDate: fd.get('workDate') as string,
            timeIn1: fd.get('timeIn1') as string || undefined,
            timeOut1: fd.get('timeOut1') as string || undefined,
            timeIn2: fd.get('timeIn2') as string || undefined,
            timeOut2: fd.get('timeOut2') as string || undefined,
            breakMinutes: Number(fd.get('breakMinutes')) || 0,
            comments: fd.get('comments') as string || undefined,
        });
    };

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>Work Days</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Track your daily work sessions</p>
                </div>
                <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', flexWrap: 'wrap' }}>
                    {(['month', 'last_month', 'all'] as FilterMode[]).map((m) => (
                        <button key={m} className={`btn ${filterMode === m ? 'btn-primary' : 'btn-secondary'}`}
                            onClick={() => setFilterMode(m)} style={{ padding: '0.4rem 0.8rem', fontSize: '0.8rem' }}>
                            {m === 'month' ? 'This Month' : m === 'last_month' ? 'Last Month' : 'All'}
                        </button>
                    ))}
                    <input className="input" type="date"
                        value={format(selectedDate, 'yyyy-MM-dd')}
                        onChange={(e) => { setSelectedDate(new Date(e.target.value)); setFilterMode('day'); }}
                        style={{ width: 'auto' }} />
                    {!todayData && (
                        <button className="btn btn-primary" onClick={() => clockIn.mutate()} disabled={clockIn.isPending}>
                            {clockIn.isPending ? 'Clocking in…' : 'Clock In'}
                        </button>
                    )}
                    <button className="btn btn-secondary" onClick={() => { setIsNew(true); setEditing({ ...EMPTY }); }}>
                        + Manual Entry
                    </button>
                </div>
            </div>

            <div className="page-content">
                {todayData && (
                    <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem', borderLeft: '4px solid var(--color-success)' }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                            <h3 style={{ fontWeight: 600 }}>Today — {format(new Date(), 'MMMM d')}</h3>
                            <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                                <span className="badge badge-success">Active</span>
                                <button className="btn btn-secondary" style={{ padding: '0.3rem 0.7rem', fontSize: '0.75rem' }}
                                    onClick={() => { setIsNew(false); setEditing(todayData); }}>Edit</button>
                            </div>
                        </div>
                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem' }}>
                            <MiniStat label="Time In" value={todayData.timeIn1 ?? '—'} />
                            <MiniStat label="Time Out" value={todayData.timeOut1 ?? '—'} />
                            <MiniStat label="Breaks" value={`${todayData.breakMinutes ?? 0} min`} />
                            <MiniStat label="Hours" value={todayData.hoursWorked?.toFixed(1) ?? '0.0'} />
                        </div>
                    </div>
                )}

                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                        {workDays?.map((wd: any) => (
                            <div key={wd.id} className="card" style={{ padding: '1rem' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                    <div>
                                        <span style={{ fontWeight: 500 }}>{format(new Date(wd.workDate), 'EEEE, MMM d, yyyy')}</span>
                                        <span style={{ marginLeft: '1rem', fontSize: '0.8rem', color: 'var(--color-text-muted)' }}>
                                            {wd.timeIn1}{wd.timeOut1 && ` → ${wd.timeOut1}`}
                                        </span>
                                    </div>
                                    <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center' }}>
                                        {wd.breakMinutes > 0 && (
                                            <span style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)' }}>{wd.breakMinutes}m break</span>
                                        )}
                                        <span style={{ fontWeight: 600, color: 'var(--color-accent)', minWidth: '3rem', textAlign: 'right' }}>
                                            {wd.hoursWorked?.toFixed(1) ?? '—'}h
                                        </span>
                                        <button className="btn btn-secondary" style={{ padding: '0.3rem 0.7rem', fontSize: '0.75rem' }}
                                            onClick={() => { setIsNew(false); setEditing(wd); }}>Edit</button>
                                        <button className="btn btn-danger" style={{ padding: '0.3rem 0.7rem', fontSize: '0.75rem' }}
                                            onClick={() => setConfirmDelete(wd)}>Del</button>
                                    </div>
                                </div>
                                {wd.comments && (
                                    <p style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', marginTop: '0.5rem' }}>{wd.comments}</p>
                                )}
                            </div>
                        ))}
                        {(!workDays || workDays.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: '2rem' }}>
                                No work days found for this filter.
                            </p>
                        )}
                    </div>
                )}
            </div>

            {editing && (
                <Modal title={isNew ? 'New Work Day' : 'Edit Work Day'} onClose={() => setEditing(null)}>
                    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Date *</label>
                            <input className="input" name="workDate" type="date"
                                defaultValue={typeof editing.workDate === 'string' ? editing.workDate.split('T')[0] : todayStr()}
                                required autoFocus />
                        </div>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Time In 1</label>
                                <input className="input" name="timeIn1" type="time" defaultValue={editing.timeIn1 ?? ''} />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Time Out 1</label>
                                <input className="input" name="timeOut1" type="time" defaultValue={editing.timeOut1 ?? ''} />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Time In 2 (optional)</label>
                                <input className="input" name="timeIn2" type="time" defaultValue={editing.timeIn2 ?? ''} />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Time Out 2 (optional)</label>
                                <input className="input" name="timeOut2" type="time" defaultValue={editing.timeOut2 ?? ''} />
                            </div>
                        </div>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Break (minutes)</label>
                            <input className="input" name="breakMinutes" type="number" min="0" defaultValue={editing.breakMinutes} />
                        </div>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Comments</label>
                            <textarea className="input" name="comments" rows={2} defaultValue={editing.comments ?? ''} style={{ resize: 'vertical' }} />
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
                <Modal title="Delete Work Day" onClose={() => setConfirmDelete(null)} width={380}>
                    <p style={{ marginBottom: '1.5rem' }}>
                        Delete work day <strong>{format(new Date((confirmDelete as any).workDate), 'MMM d, yyyy')}</strong>? This cannot be undone.
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

function MiniStat({ label, value }: { label: string; value: string }) {
    return (
        <div>
            <div style={{ fontSize: '0.7rem', color: 'var(--color-text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>{label}</div>
            <div style={{ fontSize: '1.1rem', fontWeight: 600, marginTop: '0.25rem' }}>{value}</div>
        </div>
    );
}
