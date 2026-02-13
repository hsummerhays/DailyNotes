import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';

export default function QuizzesPage() {
    const { data: quizzes, isLoading } = useQuery({
        queryKey: ['quizzes'],
        queryFn: () => api.get('/quizzes').then((r) => r.data),
    });

    return (
        <>
            <div className="page-header">
                <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>❓ Quizzes</h1>
                <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Test your knowledge</p>
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', gap: '1rem' }}>
                        {quizzes?.map((quiz: any) => (
                            <div key={quiz.id} className="card" style={{ padding: '1.25rem' }}>
                                <h3 style={{ fontWeight: 600, marginBottom: '0.5rem' }}>{quiz.title}</h3>
                                <div style={{ display: 'flex', gap: '0.5rem' }}>
                                    {quiz.difficulty != null && (
                                        <span className={`badge ${quiz.difficulty <= 2 ? 'badge-success' :
                                                quiz.difficulty <= 4 ? 'badge-warning' : 'badge-danger'
                                            }`}>
                                            {quiz.difficulty <= 2 ? 'Easy' : quiz.difficulty <= 4 ? 'Medium' : 'Hard'}
                                        </span>
                                    )}
                                </div>
                            </div>
                        ))}
                        {(!quizzes || quizzes.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', gridColumn: '1 / -1', textAlign: 'center', padding: '2rem' }}>
                                No quizzes yet
                            </p>
                        )}
                    </div>
                )}
            </div>
        </>
    );
}
