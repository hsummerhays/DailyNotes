import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import { formatDisplayDate } from '../lib/dateUtils';
import Modal from '../components/Modal';
import { useToast, ToastContainer } from '../components/Toast';

interface PayPeriod {
    id?: number;
    periodStartDate: string;
    periodEndDate: string;
    holidays: number;
    ptoReported: number;
    ptoDaysOfMonth?: string;
}

const today = () => new Date().toISOString().split('T')[0];
const EMPTY: PayPeriod = {
    periodStartDate: today(), periodEndDate: today(),
    holidays: 0, ptoReported: 0, ptoDaysOfMonth: '',
};

export default function PayPeriodsPage() {
    const qc = useQueryClient();
    const { toasts, toast, dismiss } = useToast();
    const [editing, setEditing] = useState<PayPeriod | null>(null);
    const [isNew, setIsNew] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<PayPeriod | null>(null);

    const { data: periods, isLoading } = useQuery({
        queryKey: ['pay-periods'],
        queryFn: () => api.get('/pay-periods').then((r) => r.data),
    });

    const save = useMutation({
        mutationFn: (p: PayPeriod) =>
            p.id ? api.put(`/pay-periods/${p.id}`, p) : api.post('/pay-periods', p),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['pay-periods'] });
            toast(isNew ? 'Pay period created' : 'Pay period updated', 'success');
            setEditing(null);
        },
        onError: () => toast('Failed to save pay period', 'error'),
    });

    const del = useMutation({
        mutationFn: (id: number) => api.delete(`/pay-periods/${id}`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['pay-periods'] });
            toast('Pay period deleted', 'success');
            setConfirmDelete(null);
        },
        onError: () => toast('Failed to delete pay period', 'error'),
    });

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!editing) return;
        const fd = new FormData(e.currentTarget);
        save.mutate({
            ...editing,
            periodStartDate: fd.get('periodStartDate') as string,
            periodEndDate: fd.get('periodEndDate') as string,
            holidays: Number(fd.get('holidays')) || 0,
            ptoReported: Number(fd.get('ptoReported')) || 0,
            ptoDaysOfMonth: fd.get('ptoDaysOfMonth') as string || undefined,
        });
    };

    const formatDate = (d: string) => {
        return formatDisplayDate(d);
    };

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>Pay Periods</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Track hours and earnings</p>
                </div>
                <button className="btn btn-primary" onClick={() => { setIsNew(true); setEditing({ ...EMPTY }); }}>
                    + New Period
                </button>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                        {periods?.map((period: any) => (
                            <div key={period.id} className="card" style={{ padding: '1.25rem' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                    <div>
                                        <span style={{ fontWeight: 600 }}>
                                            {formatDate(period.periodStartDate)} — {formatDate(period.periodEndDate)}
                                        </span>
                                        {(period.holidays > 0 || period.ptoReported > 0) && (
                                            <div style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)', marginTop: '0.25rem' }}>
                                                {period.holidays > 0 && `${period.holidays} holiday${period.holidays !== 1 ? 's' : ''}`}
                                                {period.holidays > 0 && period.ptoReported > 0 && ' · '}
                                                {period.ptoReported > 0 && `${period.ptoReported} PTO day${period.ptoReported !== 1 ? 's' : ''}`}
                                            </div>
                                        )}
                                    </div>
                                    <div style={{ display: 'flex', gap: '1.5rem', alignItems: 'center' }}>
                                        <div style={{ textAlign: 'right' }}>
                                            <div style={{ fontSize: '0.7rem', color: 'var(--color-text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Total Hours</div>
                                            <div style={{ fontWeight: 600, color: 'var(--color-accent)' }}>{period.totalHours?.toFixed(1) ?? '—'}</div>
                                        </div>
                                        <div style={{ textAlign: 'right' }}>
                                            <div style={{ fontSize: '0.7rem', color: 'var(--color-text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Work Days</div>
                                            <div style={{ fontWeight: 600 }}>{period.totalDays ?? '—'}</div>
                                        </div>
                                        <div style={{ display: 'flex', gap: '0.5rem' }}>
                                            <button className="btn btn-secondary" style={{ padding: '0.3rem 0.7rem', fontSize: '0.75rem' }}
                                                onClick={() => { setIsNew(false); setEditing(period); }}>Edit</button>
                                            <button className="btn btn-danger" style={{ padding: '0.3rem 0.7rem', fontSize: '0.75rem' }}
                                                onClick={() => setConfirmDelete(period)}>Del</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        ))}
                        {(!periods || periods.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: '2rem' }}>No pay periods yet</p>
                        )}
                    </div>
                )}
            </div>

            {editing && (
                <Modal title={isNew ? 'New Pay Period' : 'Edit Pay Period'} onClose={() => setEditing(null)} width={440}>
                    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Start Date *</label>
                                <input className="input" name="periodStartDate" type="date"
                                    defaultValue={typeof editing.periodStartDate === 'string' ? editing.periodStartDate.split('T')[0] : today()}
                                    required autoFocus />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>End Date *</label>
                                <input className="input" name="periodEndDate" type="date"
                                    defaultValue={typeof editing.periodEndDate === 'string' ? editing.periodEndDate.split('T')[0] : today()}
                                    required />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Holidays</label>
                                <input className="input" name="holidays" type="number" min="0" defaultValue={editing.holidays} />
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>PTO Days Reported</label>
                                <input className="input" name="ptoReported" type="number" min="0" defaultValue={editing.ptoReported} />
                            </div>
                        </div>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>PTO Days of Month</label>
                            <input className="input" name="ptoDaysOfMonth" defaultValue={editing.ptoDaysOfMonth ?? ''}
                                placeholder="e.g. 3,4,5" />
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
                <Modal title="Delete Pay Period" onClose={() => setConfirmDelete(null)} width={380}>
                    <p style={{ marginBottom: '1.5rem' }}>
                        Delete pay period <strong>{formatDate((confirmDelete as any).periodStartDate)} — {formatDate((confirmDelete as any).periodEndDate)}</strong>? This cannot be undone.
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
