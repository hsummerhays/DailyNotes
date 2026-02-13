import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';

type SearchType = 'all' | 'notes' | 'tasks' | 'topics';

export default function SearchPage() {
    const [query, setQuery] = useState('');
    const [type, setType] = useState<SearchType>('all');
    const [submitted, setSubmitted] = useState('');

    const { data, isLoading } = useQuery({
        queryKey: ['search', submitted, type],
        queryFn: () => api.get(`/search?q=${encodeURIComponent(submitted)}&type=${type}`).then((r) => r.data),
        enabled: submitted.length > 0,
    });

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(query);
    };

    return (
        <>
            <div className="page-header">
                <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>🔍 Search</h1>
                <form onSubmit={handleSearch} style={{ display: 'flex', gap: '0.75rem', marginTop: '1rem' }}>
                    <input
                        className="input"
                        type="text"
                        value={query}
                        onChange={(e) => setQuery(e.target.value)}
                        placeholder="Search notes, tasks, topics..."
                        autoFocus
                        style={{ maxWidth: 400 }}
                    />
                    <select
                        className="input"
                        value={type}
                        onChange={(e) => setType(e.target.value as SearchType)}
                        style={{ width: 140 }}
                    >
                        <option value="all">All</option>
                        <option value="notes">Notes</option>
                        <option value="tasks">Tasks</option>
                        <option value="topics">Topics</option>
                    </select>
                    <button className="btn btn-primary" type="submit">Search</button>
                </form>
            </div>

            <div className="page-content">
                {isLoading && (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '3rem' }}><span className="spinner" /></div>
                )}

                {data && (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
                        {data.workTasks?.length > 0 && (
                            <ResultSection title="Tasks" items={data.workTasks} renderItem={(t: any) => t.name} />
                        )}
                        {data.topics?.length > 0 && (
                            <ResultSection title="Topics" items={data.topics} renderItem={(t: any) => t.title} />
                        )}
                        {data.workNotes?.length > 0 && (
                            <ResultSection title="Work Notes" items={data.workNotes} renderItem={(n: any) => `Note #${n.id}`} />
                        )}
                        {data.topicNotes?.length > 0 && (
                            <ResultSection title="Topic Notes" items={data.topicNotes} renderItem={(n: any) => n.title || `Note #${n.id}`} />
                        )}
                        {submitted && !isLoading && Object.values(data).every((v: any) => !v?.length) && (
                            <p style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: '2rem' }}>
                                No results found for "{submitted}"
                            </p>
                        )}
                    </div>
                )}

                {!submitted && !isLoading && (
                    <p style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: '3rem' }}>
                        Enter a search term above to find notes, tasks, and topics
                    </p>
                )}
            </div>
        </>
    );
}

function ResultSection({ title, items, renderItem }: { title: string; items: any[]; renderItem: (item: any) => string }) {
    return (
        <div>
            <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: '0.75rem', color: 'var(--color-text-secondary)' }}>{title}</h3>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.375rem' }}>
                {items.map((item) => (
                    <div key={item.id} className="card" style={{ padding: '0.875rem 1rem' }}>
                        <span style={{ fontWeight: 500 }}>{renderItem(item)}</span>
                    </div>
                ))}
            </div>
        </div>
    );
}
