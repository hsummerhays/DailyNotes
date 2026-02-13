import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';
import { format } from 'date-fns';

const STATUS_OPTIONS = ['all', 'in-progress', 'completed', 'overdue', 'unscheduled'];

export default function TasksPage() {
    const [status, setStatus] = useState('all');

    const { data: tasks, isLoading } = useQuery({
        queryKey: ['tasks', status],
        queryFn: () => {
            const params = status !== 'all' ? `?status=${status}` : '';
            return api.get(`/work-tasks${params}`).then((r) => r.data);
        },
    });

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>✅ Tasks</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Manage your work tasks</p>
                </div>
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                    {STATUS_OPTIONS.map((s) => (
                        <button
                            key={s}
                            className={`btn ${status === s ? 'btn-primary' : 'btn-secondary'}`}
                            onClick={() => setStatus(s)}
                            style={{ padding: '0.4rem 0.8rem', fontSize: '0.8rem' }}
                        >
                            {s === 'all' ? 'All' : s.replace('-', ' ')}
                        </button>
                    ))}
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
                                                Project #{task.projectId}
                                            </span>
                                        )}
                                    </div>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                                        {task.dueDate && (
                                            <span style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)' }}>
                                                Due {format(new Date(task.dueDate), 'MMM d')}
                                            </span>
                                        )}
                                        <span className={`badge ${task.status === 'completed' ? 'badge-success' :
                                                task.status === 'overdue' ? 'badge-danger' :
                                                    task.status === 'in-progress' ? 'badge-primary' : 'badge-warning'
                                            }`}>{task.status}</span>
                                    </div>
                                </div>
                            </div>
                        ))}
                        {(!tasks || tasks.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: '2rem' }}>
                                No tasks found
                            </p>
                        )}
                    </div>
                )}
            </div>
        </>
    );
}
