import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';

export default function CoursesPage() {
    const [semester, setSemester] = useState('');

    const { data: courses, isLoading } = useQuery({
        queryKey: ['courses', semester],
        queryFn: () => {
            const params = semester ? `?semester=${encodeURIComponent(semester)}` : '';
            return api.get(`/courses${params}`).then((r) => r.data);
        },
    });

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>🎓 Courses</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Track courses and grades</p>
                </div>
                <input
                    className="input"
                    type="text"
                    placeholder="Filter by semester..."
                    value={semester}
                    onChange={(e) => setSemester(e.target.value)}
                    style={{ width: 200 }}
                />
            </div>

            <div className="page-content">
                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                ) : (
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1rem' }}>
                        {courses?.map((course: any) => (
                            <div key={course.id} className="card" style={{ padding: '1.25rem' }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                                    <h3 style={{ fontWeight: 600 }}>{course.name}</h3>
                                    <span className="badge badge-primary">{course.credits ?? '—'} cr</span>
                                </div>
                                {course.instructor && (
                                    <div style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', marginBottom: '0.25rem' }}>
                                        Instructor: {course.instructor}
                                    </div>
                                )}
                                {course.semester && (
                                    <div style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)', marginBottom: '0.75rem' }}>
                                        {course.semester}
                                    </div>
                                )}
                                <div style={{ display: 'flex', gap: '1rem' }}>
                                    {course.currentGrade != null && (
                                        <span style={{
                                            fontWeight: 600,
                                            color: course.currentGrade >= 90 ? 'var(--color-success)' :
                                                course.currentGrade >= 70 ? 'var(--color-warning)' : 'var(--color-danger)',
                                        }}>
                                            {course.currentGrade}%
                                        </span>
                                    )}
                                    {course.progressPercent != null && (
                                        <div style={{ flex: 1, display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                                            <div style={{
                                                flex: 1,
                                                height: 6,
                                                background: 'var(--color-bg)',
                                                borderRadius: 3,
                                                overflow: 'hidden',
                                            }}>
                                                <div style={{
                                                    width: `${course.progressPercent}%`,
                                                    height: '100%',
                                                    background: 'linear-gradient(90deg, var(--color-primary), var(--color-accent))',
                                                    borderRadius: 3,
                                                }} />
                                            </div>
                                            <span style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                                                {course.progressPercent}%
                                            </span>
                                        </div>
                                    )}
                                </div>
                            </div>
                        ))}
                        {(!courses || courses.length === 0) && (
                            <p style={{ color: 'var(--color-text-muted)', gridColumn: '1 / -1', textAlign: 'center', padding: '2rem' }}>
                                No courses found
                            </p>
                        )}
                    </div>
                )}
            </div>
        </>
    );
}
