import { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import api from '../lib/api';
import { format } from 'date-fns';

export default function DashboardPage() {
    const [greeting, setGreeting] = useState('');

    useEffect(() => {
        const hour = new Date().getHours();
        if (hour < 12) setGreeting('Good Morning');
        else if (hour < 17) setGreeting('Good Afternoon');
        else setGreeting('Good Evening');
    }, []);

    const { data: today } = useQuery({
        queryKey: ['workday-today'],
        queryFn: () => api.get('/work-days/today').then((r) => r.data),
        retry: false,
    });

    const { data: tasks } = useQuery({
        queryKey: ['tasks-active'],
        queryFn: () => api.get('/work-tasks?status=in-progress').then((r) => r.data),
    });

    const { data: overdue } = useQuery({
        queryKey: ['tasks-overdue'],
        queryFn: () => api.get('/work-tasks/overdue').then((r) => r.data),
    });

    return (
        <>
            <div className="page-header">
                <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>{greeting} 👋</h1>
                <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem', marginTop: '0.25rem' }}>
                    {format(new Date(), 'EEEE, MMMM d, yyyy')}
                </p>
            </div>

            <div className="page-content">
                {/* Stats Grid */}
                <div style={{
                    display: 'grid',
                    gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                    gap: '1rem',
                    marginBottom: '1.5rem',
                }}>
                    <StatCard label="Today's Status" value={today ? '🟢 Clocked In' : '⚪ Not Started'} accent="#10b981" />
                    <StatCard label="Active Tasks" value={tasks?.length ?? '—'} accent="#6366f1" />
                    <StatCard label="Overdue" value={overdue?.length ?? '0'} accent={overdue?.length > 0 ? '#ef4444' : '#64748b'} />
                    <StatCard label="Hours Today" value={today?.totalHours?.toFixed(1) ?? '0.0'} accent="#06b6d4" />
                </div>

                {/* Quick Actions */}
                <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '2rem', flexWrap: 'wrap' }}>
                    <Link to="/work-days" className="btn btn-primary">📅 View Today</Link>
                    <Link to="/tasks" className="btn btn-secondary">✅ My Tasks</Link>
                    <Link to="/notes" className="btn btn-secondary">📝 New Note</Link>
                    <Link to="/search" className="btn btn-secondary">🔍 Search</Link>
                </div>

                {/* Active Tasks */}
                <div style={{ marginBottom: '2rem' }}>
                    <h2 style={{ fontSize: '1.1rem', fontWeight: 600, marginBottom: '1rem' }}>Active Tasks</h2>
                    {tasks?.length ? (
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                            {tasks.slice(0, 5).map((task: any) => (
                                <div key={task.id} className="card" style={{ padding: '1rem' }}>
                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                        <span style={{ fontWeight: 500 }}>{task.name}</span>
                                        <span className={`badge ${task.status === 'overdue' ? 'badge-danger' : 'badge-primary'}`}>
                                            {task.status}
                                        </span>
                                    </div>
                                    {task.dueDate && (
                                        <span style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)', marginTop: '0.25rem', display: 'block' }}>
                                            Due: {format(new Date(task.dueDate), 'MMM d')}
                                        </span>
                                    )}
                                </div>
                            ))}
                        </div>
                    ) : (
                        <p style={{ color: 'var(--color-text-muted)', fontStyle: 'italic' }}>No active tasks</p>
                    )}
                </div>

                {/* Overdue Tasks */}
                {overdue?.length > 0 && (
                    <div>
                        <h2 style={{ fontSize: '1.1rem', fontWeight: 600, marginBottom: '1rem', color: 'var(--color-danger)' }}>
                            ⚠️ Overdue Tasks
                        </h2>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                            {overdue.map((task: any) => (
                                <div key={task.id} className="card" style={{ padding: '1rem', borderColor: 'rgba(239, 68, 68, 0.3)' }}>
                                    <span style={{ fontWeight: 500 }}>{task.name}</span>
                                    {task.dueDate && (
                                        <span style={{ fontSize: '0.8rem', color: 'var(--color-danger)', marginLeft: '0.5rem' }}>
                                            (due {format(new Date(task.dueDate), 'MMM d')})
                                        </span>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </div>
        </>
    );
}

function StatCard({ label, value, accent }: { label: string; value: string | number; accent: string }) {
    return (
        <div className="card" style={{ padding: '1.25rem' }}>
            <div style={{
                fontSize: '0.75rem',
                fontWeight: 500,
                color: 'var(--color-text-muted)',
                textTransform: 'uppercase',
                letterSpacing: '0.05em',
                marginBottom: '0.5rem',
            }}>{label}</div>
            <div style={{ fontSize: '1.5rem', fontWeight: 700, color: accent }}>{value}</div>
        </div>
    );
}
