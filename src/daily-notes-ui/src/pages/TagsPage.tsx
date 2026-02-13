import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';

export default function TagsPage() {
    const { data: tags, isLoading } = useQuery({
        queryKey: ['tags'],
        queryFn: () => api.get('/tags').then((r) => r.data),
    });

    return (
        <>
            <div className="page-header">
                <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>🏷️ Tags</h1>
                <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Manage your tags</p>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.75rem' }}>
                        {tags?.map((tag: any) => (
                            <div key={tag.id} style={{
                                display: 'inline-flex',
                                alignItems: 'center',
                                gap: '0.5rem',
                                padding: '0.5rem 1rem',
                                background: 'var(--color-bg-card)',
                                border: '1px solid var(--color-border)',
                                borderRadius: '9999px',
                                cursor: 'pointer',
                                transition: 'all 0.15s ease',
                            }}>
                                <span style={{
                                    width: 12,
                                    height: 12,
                                    borderRadius: '50%',
                                    background: tag.color || '#6366f1',
                                }} />
                                <span style={{ fontWeight: 500, fontSize: '0.875rem' }}>{tag.name}</span>
                            </div>
                        ))}
                        {(!tags || tags.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', width: '100%', textAlign: 'center', padding: '2rem' }}>
                                No tags yet
                            </p>
                        )}
                    </div>
                )}
            </div>
        </>
    );
}
