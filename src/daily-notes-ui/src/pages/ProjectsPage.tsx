import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';
import { format } from 'date-fns';

export default function ProjectsPage() {
    const { data: projects, isLoading } = useQuery({
        queryKey: ['projects'],
        queryFn: () => api.get('/projects').then((r) => r.data),
    });

    return (
        <>
            <div className="page-header">
                <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>📁 Projects</h1>
                <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Organize work by project</p>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '1rem' }}>
                        {projects?.map((project: any) => (
                            <div key={project.id} className="card" style={{ padding: '1.25rem' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                                    <h3 style={{ fontWeight: 600, fontSize: '1rem' }}>{project.name}</h3>
                                    <span className="badge badge-primary">{project.category || 'Uncategorized'}</span>
                                </div>
                                <div style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)', marginBottom: '0.5rem' }}>
                                    Created {format(new Date(project.createdAt), 'MMM d, yyyy')}
                                </div>
                                {project.completedAt && (
                                    <span className="badge badge-success" style={{ marginTop: '0.25rem' }}>
                                        ✓ Completed
                                    </span>
                                )}
                            </div>
                        ))}
                        {(!projects || projects.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', gridColumn: '1 / -1', textAlign: 'center', padding: '2rem' }}>
                                No projects yet
                            </p>
                        )}
                    </div>
                )}
            </div>
        </>
    );
}
