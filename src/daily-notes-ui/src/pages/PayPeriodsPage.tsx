import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';
import { format } from 'date-fns';

export default function PayPeriodsPage() {
    const { data: periods, isLoading } = useQuery({
        queryKey: ['pay-periods'],
        queryFn: () => api.get('/pay-periods').then((r) => r.data),
    });

    return (
        <>
            <div className="page-header">
                <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>💰 Pay Periods</h1>
                <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Track hours and earnings</p>
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
                                            {format(new Date(period.startDate), 'MMM d')} — {format(new Date(period.endDate), 'MMM d, yyyy')}
                                        </span>
                                    </div>
                                    <div style={{ display: 'flex', gap: '1.5rem', alignItems: 'center' }}>
                                        <div style={{ textAlign: 'right' }}>
                                            <div style={{ fontSize: '0.7rem', color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>Total Hours</div>
                                            <div style={{ fontWeight: 600, color: 'var(--color-accent)' }}>{period.totalHours?.toFixed(1) ?? '—'}</div>
                                        </div>
                                        <div style={{ textAlign: 'right' }}>
                                            <div style={{ fontSize: '0.7rem', color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>Work Days</div>
                                            <div style={{ fontWeight: 600 }}>{period.totalDays ?? '—'}</div>
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
        </>
    );
}
