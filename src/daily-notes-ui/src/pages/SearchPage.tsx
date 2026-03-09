import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import api from '../lib/api';
import { formatDisplayDate } from '../lib/dateUtils';
import { TASK_STATUS } from '../lib/taskStatus';
import {
    Search as SearchIcon,
    Filter,
    Calendar,
    FileText,
    CheckSquare,
    Book,
    ChevronRight,
    Folder
} from 'lucide-react';

type SearchType = 'all' | 'notes' | 'tasks' | 'topics';

export default function SearchPage() {
    const [query, setQuery] = useState('');
    const [submitted, setSubmitted] = useState('');
    const [searchType, setSearchType] = useState<SearchType>('all');
    const [dateFrom, setDateFrom] = useState('');
    const [dateTo, setDateTo] = useState('');
    const [projectId, setProjectId] = useState<number | null>(null);
    const [selectedStatuses, setSelectedStatuses] = useState<string[]>([]);

    // Fetch filters metadata
    const { data: projects } = useQuery({
        queryKey: ['projects'],
        queryFn: () => api.get('/projects').then((r) => r.data),
    });

    const { data, isLoading } = useQuery({
        queryKey: ['search', submitted, searchType, dateFrom, dateTo, projectId, selectedStatuses],
        queryFn: () => {
            let url = `/search?q=${encodeURIComponent(submitted)}&type=${searchType}`;
            if (dateFrom) url += `&dateFrom=${dateFrom}`;
            if (dateTo) url += `&dateTo=${dateTo}`;
            if (projectId) url += `&projectId=${projectId}`;
            if (selectedStatuses.length > 0) url += `&statuses=${selectedStatuses.join(',')}`;
            return api.get(url).then((r) => r.data);
        },
        enabled: submitted.length > 0,
    });

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(query);
    };

    const toggleStatus = (status: string) => {
        setSelectedStatuses(prev =>
            prev.includes(status) ? prev.filter(s => s !== status) : [...prev, status]
        );
    };

    const clearFilters = () => {
        setSearchType('all');
        setDateFrom('');
        setDateTo('');
        setProjectId(null);
        setSelectedStatuses([]);
    };

    return (
        <div style={{ display: 'flex', minHeight: 'calc(100vh - 64px)' }}>
            {/* Facet Sidebar */}
            <aside style={{
                width: '300px',
                borderRight: '1px solid var(--color-border)',
                background: 'rgba(30, 41, 59, 0.5)',
                padding: '2rem',
                display: 'flex',
                flexDirection: 'column',
                gap: '2rem',
                position: 'sticky',
                top: '64px',
                height: 'calc(100vh - 64px)',
                overflowY: 'auto'
            }}>
                <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                        <h3 style={{ fontSize: '0.875rem', fontWeight: 600, display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                            <Filter size={16} /> Filters
                        </h3>
                        <button onClick={clearFilters} style={{
                            fontSize: '0.75rem',
                            color: 'var(--color-primary-light)',
                            background: 'none',
                            border: 'none',
                            cursor: 'pointer'
                        }}>Clear All</button>
                    </div>

                    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
                        {/* Entity Type Facet */}
                        <section>
                            <label style={{ fontSize: '0.75rem', fontWeight: 700, color: 'var(--color-text-secondary)', textTransform: 'uppercase', marginBottom: '0.75rem', display: 'block' }}>
                                Result Type
                            </label>
                            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                                {(['all', 'notes', 'tasks', 'topics'] as SearchType[]).map(t => (
                                    <label key={t} style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', cursor: 'pointer', fontSize: '0.875rem' }}>
                                        <input type="radio" checked={searchType === t} onChange={() => setSearchType(t)} style={{ accentColor: 'var(--color-primary)' }} />
                                        <span>{t.charAt(0).toUpperCase() + t.slice(1)}</span>
                                    </label>
                                ))}
                            </div>
                        </section>

                        <hr style={{ border: 'none', borderTop: '1px solid var(--color-border)' }} />

                        {/* Date Facet */}
                        <section>
                            <label style={{ fontSize: '0.75rem', fontWeight: 700, color: 'var(--color-text-secondary)', textTransform: 'uppercase', marginBottom: '0.75rem', display: 'block' }}>
                                Date Range
                            </label>
                            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                                <div>
                                    <span style={{ fontSize: '0.7rem', color: 'var(--color-text-muted)', display: 'block', marginBottom: '0.25rem' }}>From</span>
                                    <input type="date" className="input" value={dateFrom} onChange={e => setDateFrom(e.target.value)} style={{ padding: '0.4rem 0.6rem' }} />
                                </div>
                                <div>
                                    <span style={{ fontSize: '0.7rem', color: 'var(--color-text-muted)', display: 'block', marginBottom: '0.25rem' }}>To</span>
                                    <input type="date" className="input" value={dateTo} onChange={e => setDateTo(e.target.value)} style={{ padding: '0.4rem 0.6rem' }} />
                                </div>
                            </div>
                        </section>

                        <hr style={{ border: 'none', borderTop: '1px solid var(--color-border)' }} />

                        {/* Status Facet (Only relevant if Tasks included) */}
                        <section>
                            <label style={{ fontSize: '0.75rem', fontWeight: 700, color: 'var(--color-text-secondary)', textTransform: 'uppercase', marginBottom: '0.75rem', display: 'block' }}>
                                Task Status
                            </label>
                            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                                {[TASK_STATUS.pending, TASK_STATUS.inProgress, TASK_STATUS.completed].map(s => (
                                    <label key={s} style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', cursor: 'pointer', fontSize: '0.875rem' }}>
                                        <input type="checkbox" checked={selectedStatuses.includes(s)} onChange={() => toggleStatus(s)} style={{ accentColor: 'var(--color-primary)' }} />
                                        <span>{s.replace('_', ' ')}</span>
                                    </label>
                                ))}
                            </div>
                        </section>

                        <hr style={{ border: 'none', borderTop: '1px solid var(--color-border)' }} />

                        {/* Project Facet */}
                        <section>
                            <label style={{ fontSize: '0.75rem', fontWeight: 700, color: 'var(--color-text-secondary)', textTransform: 'uppercase', marginBottom: '0.75rem', display: 'block' }}>
                                Project
                            </label>
                            <select
                                className="input"
                                value={projectId ?? ''}
                                onChange={e => setProjectId(e.target.value ? Number(e.target.value) : null)}
                                style={{ padding: '0.4rem 0.6rem' }}
                            >
                                <option value="">All Projects</option>
                                {projects?.map((p: any) => (
                                    <option key={p.id} value={p.id}>{p.name}</option>
                                ))}
                            </select>
                        </section>
                    </div>
                </div>
            </aside>

            {/* Main Area */}
            <main style={{ flex: 1, padding: '2rem' }}>
                <div style={{ maxWidth: '900px', margin: '0 auto' }}>
                    <form onSubmit={handleSearch} style={{ position: 'relative', marginBottom: '2.5rem' }}>
                        <SearchIcon style={{ position: 'absolute', left: '1rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--color-text-muted)' }} size={20} />
                        <input
                            className="input"
                            type="text"
                            value={query}
                            onChange={(e) => setQuery(e.target.value)}
                            placeholder="Type to search through everything..."
                            autoFocus
                            style={{ paddingLeft: '3rem', fontSize: '1.1rem', height: '3.5rem', borderRadius: '1rem', boxShadow: 'var(--shadow-elevated)' }}
                        />
                        <button className="btn btn-primary" type="submit" style={{ position: 'absolute', right: '0.5rem', top: '0.5rem', bottom: '0.5rem', borderRadius: '0.75rem' }}>
                            Search
                        </button>
                    </form>

                    {isLoading && (
                        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '1rem', padding: '5rem 0' }}>
                            <span className="spinner" style={{ width: '40px', height: '40px' }} />
                            <p style={{ color: 'var(--color-text-muted)' }}>Searching the vault...</p>
                        </div>
                    )}

                    {!submitted && !isLoading && (
                        <div style={{ textAlign: 'center', padding: '5rem 0' }}>
                            <div style={{ width: '80px', height: '80px', borderRadius: '50%', background: 'var(--color-bg-elevated)', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 1.5rem', color: 'var(--color-text-muted)' }}>
                                <SearchIcon size={32} />
                            </div>
                            <h2 style={{ fontSize: '1.25rem', fontWeight: 600, marginBottom: '0.5rem' }}>Experience the Power of Recall</h2>
                            <p style={{ color: 'var(--color-text-muted)', maxWidth: '400px', margin: '0 auto' }}>
                                Find any note, task, or topic across your entire history with instant faceted filtering.
                            </p>
                        </div>
                    )}

                    {data && submitted && (
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '2.5rem' }}>
                            <ResultsList
                                title="Notes"
                                icon={<FileText size={18} />}
                                items={data.workNotes}
                                linkPrefix="/notes"
                                renderContent={(n: any) => (
                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                        <span style={{ fontSize: '0.9rem', color: 'var(--color-text-secondary)' }}>
                                            Work Entry from {formatDisplayDate(n.noteDate)}
                                        </span>
                                        {n.workTaskId && <span className="badge badge-primary">Linked to Task</span>}
                                    </div>
                                )}
                            />

                            <ResultsList
                                title="Tasks"
                                icon={<CheckSquare size={18} />}
                                items={data.workTasks}
                                linkPrefix="/tasks"
                                renderContent={(t: any) => (
                                    <div>
                                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
                                            <span style={{ fontWeight: 600 }}>{t.name}</span>
                                            <span className={`badge ${t.status === 'completed' ? 'badge-success' : 'badge-primary'}`}>
                                                {t.status.replace('_', ' ')}
                                            </span>
                                        </div>
                                        <div style={{ display: 'flex', gap: '1rem', fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                                            <span style={{ display: 'flex', alignItems: 'center', gap: '0.25rem' }}>
                                                <Calendar size={12} /> {formatDisplayDate(t.createdAt)}
                                            </span>
                                            {t.projectId && (
                                                <span style={{ display: 'flex', alignItems: 'center', gap: '0.25rem' }}>
                                                    <Folder size={12} /> Project #{t.projectId}
                                                </span>
                                            )}
                                        </div>
                                    </div>
                                )}
                            />

                            <ResultsList
                                title="Topics"
                                icon={<Book size={18} />}
                                items={data.topics}
                                linkPrefix="/topics"
                                renderContent={(t: any) => (
                                    <div>
                                        <div style={{ fontWeight: 600, marginBottom: '0.25rem' }}>{t.title}</div>
                                        {t.description && <p style={{ fontSize: '0.85rem', color: 'var(--color-text-secondary)' }}>{t.description}</p>}
                                    </div>
                                )}
                            />

                            {/* No results empty state */}
                            {Object.values(data).every((v: any) => !v?.length) && (
                                <div style={{ textAlign: 'center', padding: '5rem 0' }}>
                                    <SearchIcon size={48} style={{ color: 'var(--color-text-muted)', marginBottom: '1rem', opacity: 0.5 }} />
                                    <h3>No matches found</h3>
                                    <p style={{ color: 'var(--color-text-muted)' }}>Try adjusting your filters or using broader terms.</p>
                                </div>
                            )}
                        </div>
                    )}
                </div>
            </main>
        </div>
    );
}

function ResultsList({ title, icon, items, renderContent, linkPrefix }: { title: string; icon: React.ReactNode; items: any[]; renderContent: (item: any) => React.ReactNode; linkPrefix: string }) {
    if (!items || items.length === 0) return null;

    return (
        <section>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1rem', borderBottom: '1px solid var(--color-border)', paddingBottom: '0.5rem' }}>
                <span style={{ color: 'var(--color-primary-light)' }}>{icon}</span>
                <h2 style={{ fontSize: '1rem', fontWeight: 600 }}>{title} ({items.length})</h2>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                {items.map((item) => (
                    <Link key={item.id} to={linkPrefix} style={{ textDecoration: 'none', color: 'inherit' }}>
                        <div className="card" style={{ padding: '1.25rem', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '1rem' }}>
                            <div style={{ flex: 1 }}>
                                {renderContent(item)}
                            </div>
                            <ChevronRight size={18} style={{ color: 'var(--color-text-muted)', opacity: 0.5 }} />
                        </div>
                    </Link>
                ))}
            </div>
        </section>
    );
}
