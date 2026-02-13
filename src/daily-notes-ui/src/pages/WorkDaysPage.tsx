import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import { format } from 'date-fns';

export default function WorkDaysPage() {
    const queryClient = useQueryClient();
    const [dateFilter, setDateFilter] = useState(format(new Date(), 'yyyy-MM-dd'));

    const { data: workDays, isLoading } = useQuery({
        queryKey: ['work-days', dateFilter],
        queryFn: () => api.get(`/work-days?date=${dateFilter}`).then((r) => r.data),
    });

    const { data: today } = useQuery({
        queryKey: ['work-day-today'],
        queryFn: () => api.get('/work-days/today').then((r) => r.data).catch(() => null),
        retry: false,
    });

    const clockIn = useMutation({
        mutationFn: () => api.post('/work-days', {
            workDate: new Date().toISOString(),
            timeIn: new Date().toISOString(),
        }),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['work-day'] }),
    });

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>📅 Work Days</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Track your daily work sessions</p>
                </div>
                <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center' }}>
                    <input
                        className="input"
                        type="date"
                        value={dateFilter}
                        onChange={(e) => setDateFilter(e.target.value)}
                        style={{ width: 180 }}
                    />
                    {!today && (
                        <button className="btn btn-primary" onClick={() => clockIn.mutate()}>
                            Clock In
                        </button>
                    )}
                </div>
            </div>

            <div className="page-content">
                {/* Today's Card */}
                {today && (
                    <div className="card" style={{
                        padding: '1.5rem',
                        marginBottom: '1.5rem',
                        borderLeft: '4px solid var(--color-success)',
                    }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                            <h3 style={{ fontWeight: 600 }}>Today — {format(new Date(), 'MMMM d')}</h3>
                            <span className="badge badge-success">Active</span>
                        </div>
                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem' }}>
                            <MiniStat label="Time In" value={today.timeIn ? format(new Date(today.timeIn), 'h:mm a') : '—'} />
                            <MiniStat label="Time Out" value={today.timeOut ? format(new Date(today.timeOut), 'h:mm a') : '—'} />
                            <MiniStat label="Breaks" value={`${today.breakMinutes ?? 0} min`} />
                            <MiniStat label="Total Hours" value={today.totalHours?.toFixed(1) ?? '0.0'} />
                        </div>
                    </div>
                )}

                {/* Work Days List */}
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                        {workDays?.map((wd: any) => (
                            <div key={wd.id} className="card" style={{ padding: '1rem' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                    <div>
                                        <span style={{ fontWeight: 500 }}>{format(new Date(wd.workDate), 'EEEE, MMM d')}</span>
                                        <span style={{ marginLeft: '1rem', fontSize: '0.8rem', color: 'var(--color-text-muted)' }}>
                                            {wd.timeIn && format(new Date(wd.timeIn), 'h:mm a')}
                                            {wd.timeOut && ` → ${format(new Date(wd.timeOut), 'h:mm a')}`}
                                        </span>
                                    </div>
                                    <span style={{ fontWeight: 600, color: 'var(--color-accent)' }}>
                                        {wd.totalHours?.toFixed(1) ?? '—'}h
                                    </span>
                                </div>
                                {wd.comments && (
                                    <p style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', marginTop: '0.5rem' }}>
                                        {wd.comments}
                                    </p>
                                )}
                            </div>
                        ))}
                        {(!workDays || workDays.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: '2rem' }}>
                                No work days found for this date
                            </p>
                        )}
                    </div>
                )}
            </div>
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
