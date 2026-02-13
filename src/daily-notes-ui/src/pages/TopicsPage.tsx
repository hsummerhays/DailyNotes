import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';

export default function TopicsPage() {
    const { data: topics, isLoading } = useQuery({
        queryKey: ['topics-root'],
        queryFn: () => api.get('/topics').then((r) => r.data),
    });

    return (
        <>
            <div className="page-header">
                <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>🧠 Knowledge Base</h1>
                <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Explore and manage topics</p>
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
                                <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                                    {topic.proficiency && (
                                        <span className="badge badge-primary">{topic.proficiency}</span>
                                    )}
                                    {topic.isPinned && (
                                        <span className="badge badge-warning">📌 Pinned</span>
                                    )}
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
        </>
    );
}
