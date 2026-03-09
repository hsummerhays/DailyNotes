import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import Modal from '../components/Modal';
import { useToast, ToastContainer } from '../components/Toast';
import { ChevronRight, ChevronDown, Folder, FolderOpen, FileText, Plus, Edit2, Trash2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import { formatDisplayDate } from '../lib/dateUtils';

interface Topic {
    id?: number;
    title: string;
    description?: string;
    proficiency?: string;
    parentTopicId?: number | null;
    isPinned: boolean;
}

const EMPTY: Topic = { title: '', description: '', proficiency: '', parentTopicId: null, isPinned: false };
const PROFICIENCY = ['', 'beginner', 'intermediate', 'advanced', 'expert'];

const extractText = (content: any): string => {
    if (!content) return '';
    if (typeof content === 'string') return content;
    const c = content as Record<string, unknown>;
    if (typeof c.text === 'string') return c.text;
    if (c.type === 'doc' && Array.isArray(c.content)) {
        return (c.content as unknown[]).map((node: unknown) => {
            const n = node as Record<string, unknown>;
            if (Array.isArray(n.content)) {
                return (n.content as unknown[]).map((t: unknown) => {
                    const tx = t as Record<string, unknown>;
                    return typeof tx.text === 'string' ? tx.text : '';
                }).join('');
            }
            return '';
        }).join('\n');
    }
    return '';
};

export default function TopicsPage() {
    const qc = useQueryClient();
    const { toasts, toast, dismiss } = useToast();
    const [editing, setEditing] = useState<Topic | null>(null);
    const [isNew, setIsNew] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<Topic | null>(null);
    const [selectedId, setSelectedId] = useState<number | null>(null);
    const [expandedIds, setExpandedIds] = useState<Set<number>>(new Set());

    const { data: allTopics, isLoading } = useQuery({
        queryKey: ['topics-all'],
        queryFn: () => api.get('/topics?all=true').then((r) => r.data),
    });

    const { data: topicNotes, isLoading: isNotesLoading } = useQuery({
        queryKey: ['topic-notes', selectedId],
        queryFn: () => api.get(`/topics/${selectedId}/notes`).then(r => r.data),
        enabled: !!selectedId,
    });

    // Build the Knowledge Base Tree
    const tree = useMemo(() => {
        if (!allTopics) return [];
        const map = new Map();
        allTopics.forEach((t: Topic) => map.set(t.id, { ...t, children: [] }));
        const roots: any[] = [];
        allTopics.forEach((t: Topic) => {
            if (t.parentTopicId) {
                const parent = map.get(t.parentTopicId);
                if (parent) parent.children.push(map.get(t.id));
            } else {
                roots.push(map.get(t.id));
            }
        });
        return roots;
    }, [allTopics]);

    const selectedTopic = useMemo(() => {
        return allTopics?.find((t: Topic) => t.id === selectedId) || null;
    }, [allTopics, selectedId]);

    const save = useMutation({
        mutationFn: (t: Topic) =>
            t.id ? api.put(`/topics/${t.id}`, t) : api.post('/topics', t),
        onSuccess: (data) => {
            qc.invalidateQueries({ queryKey: ['topics-all'] });
            toast(isNew ? 'Topic created' : 'Topic updated', 'success');
            setEditing(null);
            if (isNew) {
                // If we added a child, make sure parent is expanded
                if (data.data.parentTopicId) {
                    setExpandedIds(prev => new Set([...prev, data.data.parentTopicId]));
                }
                setSelectedId(data.data.id);
            }
        },
        onError: () => toast('Failed to save topic', 'error'),
    });

    const del = useMutation({
        mutationFn: (id: number) => api.delete(`/topics/${id}`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['topics-all'] });
            toast('Topic deleted', 'success');
            setConfirmDelete(null);
            if (selectedId === confirmDelete?.id) {
                setSelectedId(null);
            }
        },
        onError: () => toast('Failed to delete topic', 'error'),
    });

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!editing) return;
        const fd = new FormData(e.currentTarget);
        save.mutate({
            ...editing,
            title: fd.get('title') as string,
            description: fd.get('description') as string || undefined,
            proficiency: fd.get('proficiency') as string || undefined,
            parentTopicId: fd.get('parentTopicId') ? Number(fd.get('parentTopicId')) : null,
            isPinned: fd.get('isPinned') === 'on',
        });
    };

    const toggleExpand = (id: number, e: React.MouseEvent) => {
        e.stopPropagation();
        setExpandedIds(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id);
            else next.add(id);
            return next;
        });
    };

    const TreeNode = ({ node, level }: { node: any, level: number }) => {
        const isExpanded = expandedIds.has(node.id);
        const hasChildren = node.children && node.children.length > 0;
        const isSelected = selectedId === node.id;

        return (
            <div>
                <div
                    onClick={() => setSelectedId(node.id)}
                    style={{
                        display: 'flex',
                        alignItems: 'center',
                        padding: '0.4rem 0.5rem',
                        paddingLeft: `${level * 1.5 + 0.5}rem`,
                        cursor: 'pointer',
                        background: isSelected ? 'rgba(var(--color-primary-rgb), 0.15)' : 'transparent',
                        color: isSelected ? 'var(--color-primary-light)' : 'inherit',
                        borderLeft: isSelected ? '3px solid var(--color-primary)' : '3px solid transparent',
                        borderRadius: '0 4px 4px 0',
                        fontSize: '0.9rem',
                        transition: 'background 0.15s ease'
                    }}
                    onMouseEnter={(e) => {
                        if (!isSelected) e.currentTarget.style.background = 'var(--color-bg-hover)';
                    }}
                    onMouseLeave={(e) => {
                        if (!isSelected) e.currentTarget.style.background = 'transparent';
                    }}
                >
                    <div
                        onClick={(e) => hasChildren && toggleExpand(node.id, e)}
                        style={{ width: '20px', display: 'flex', justifyContent: 'center', alignItems: 'center', visibility: hasChildren ? 'visible' : 'hidden', cursor: hasChildren ? 'pointer' : 'default', color: 'var(--color-text-muted)' }}
                    >
                        {isExpanded ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', flex: 1 }}>
                        <span style={{ color: isSelected ? 'var(--color-primary)' : 'var(--color-text-secondary)' }}>
                            {isExpanded || isSelected ? <FolderOpen size={16} /> : <Folder size={16} />}
                        </span>
                        <span style={{ fontWeight: isSelected ? 600 : 500, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                            {node.title}
                        </span>
                        {node.isPinned && <span style={{ width: 6, height: 6, borderRadius: '50%', background: 'var(--color-warning)' }} title="Pinned" />}
                    </div>
                </div>
                {isExpanded && hasChildren && (
                    <div>
                        {node.children.map((child: any) => (
                            <TreeNode key={child.id} node={child} level={level + 1} />
                        ))}
                    </div>
                )}
            </div>
        );
    };

    return (
        <>
            <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 0 }}>
                <div>
                    <h1 style={{ fontSize: '1.5rem', fontWeight: 600 }}>Knowledge Base</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>Organize your reference notes into a hierarchy</p>
                </div>
                <button className="btn btn-primary" onClick={() => { setIsNew(true); setEditing({ ...EMPTY, parentTopicId: selectedId }); }}>
                    <Plus size={16} style={{ marginRight: '0.25rem' }} /> New Topic
                </button>
            </div>

            <div style={{ display: 'flex', height: 'calc(100vh - 150px)', overflow: 'hidden', borderTop: '1px solid var(--color-border)' }}>
                {/* Sidebar Tree */}
                <aside style={{
                    width: '300px',
                    borderRight: '1px solid var(--color-border)',
                    background: 'rgba(0,0,0,0.1)',
                    overflowY: 'auto',
                    padding: '1rem 0'
                }}>
                    {isLoading ? (
                        <div style={{ display: 'flex', justifyContent: 'center', padding: '2rem' }}><span className="spinner" /></div>
                    ) : tree.length > 0 ? (
                        tree.map(node => <TreeNode key={node.id} node={node} level={0} />)
                    ) : (
                        <div style={{ padding: '2rem 1rem', textAlign: 'center', color: 'var(--color-text-muted)', fontSize: '0.875rem' }}>
                            No topics found. Create your first topic to get started!
                        </div>
                    )}
                </aside>

                {/* Main Content Area */}
                <main style={{ flex: 1, overflowY: 'auto', padding: '2rem', background: 'var(--color-bg-base)' }}>
                    {selectedTopic ? (
                        <div style={{ maxWidth: '800px', margin: '0 auto' }}>
                            {/* Topic Header details */}
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1.5rem' }}>
                                <div>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.5rem' }}>
                                        <h2 style={{ fontSize: '1.8rem', fontWeight: 700, margin: 0 }}>{selectedTopic.title}</h2>
                                        {selectedTopic.proficiency && (
                                            <span className="badge badge-primary">{selectedTopic.proficiency}</span>
                                        )}
                                        {selectedTopic.isPinned && (
                                            <span className="badge badge-warning">Pinned</span>
                                        )}
                                    </div>
                                    {selectedTopic.description && (
                                        <p style={{ color: 'var(--color-text-secondary)', fontSize: '1rem', lineHeight: '1.5' }}>
                                            {selectedTopic.description}
                                        </p>
                                    )}
                                </div>
                                <div style={{ display: 'flex', gap: '0.5rem' }}>
                                    <button className="btn btn-secondary" onClick={() => { setIsNew(false); setEditing(selectedTopic); }} title="Edit Topic">
                                        <Edit2 size={16} />
                                    </button>
                                    <button className="btn btn-danger" onClick={() => setConfirmDelete(selectedTopic)} title="Delete Topic">
                                        <Trash2 size={16} />
                                    </button>
                                </div>
                            </div>

                            <hr style={{ border: 'none', borderTop: '1px solid var(--color-border)', margin: '2rem 0' }} />

                            {/* Reference Notes / Linked Notes */}
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                                <h3 style={{ fontSize: '1.2rem', fontWeight: 600, display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                                    <FileText size={18} color="var(--color-primary-light)" /> Reference Notes
                                </h3>
                                {/* Assuming Notes page will eventually handle topic pre-filling or we just link to it */}
                                <Link to="/notes" className="btn btn-secondary" style={{ fontSize: '0.75rem', padding: '0.3rem 0.6rem' }}>
                                    View Note Journal
                                </Link>
                            </div>

                            {isNotesLoading ? (
                                <div style={{ display: 'flex', justifyContent: 'center', padding: '2rem' }}><span className="spinner" /></div>
                            ) : topicNotes?.length > 0 ? (
                                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                                    {topicNotes.map((note: any) => (
                                        <div key={note.id} className="card" style={{ padding: '1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                                <div>
                                                    <span style={{ fontWeight: 600, fontSize: '1.05rem', color: 'var(--color-text)' }}>
                                                        {note.title || 'Untitled Session'}
                                                    </span>
                                                    <div style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)', marginTop: '0.2rem' }}>
                                                        {formatDisplayDate(note.createdAt, 'MMM d, yyyy h:mm a')}
                                                        {note.timeMinutes > 0 && <span className="badge badge-primary" style={{ marginLeft: '0.75rem' }}>{note.timeMinutes}m studied</span>}
                                                    </div>
                                                </div>
                                                <button className="btn btn-secondary" style={{ padding: '0.3rem 0.6rem', fontSize: '0.75rem' }}>
                                                    <Edit2 size={12} style={{ marginRight: '0.2rem' }} /> Edit
                                                </button>
                                            </div>
                                            <p style={{ fontSize: '0.9rem', color: 'var(--color-text-secondary)', margin: 0, display: '-webkit-box', WebkitLineClamp: 3, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                                                {extractText(note.content)}
                                            </p>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <div style={{ background: 'rgba(0,0,0,0.1)', border: '1px dashed var(--color-border)', padding: '3rem 2rem', textAlign: 'center', borderRadius: 'var(--radius-md)' }}>
                                    <FileText size={32} style={{ color: 'var(--color-text-muted)', opacity: 0.5, marginBottom: '1rem' }} />
                                    <p style={{ margin: 0, fontWeight: 500 }}>No study notes for this topic yet.</p>
                                    <p style={{ margin: '0.5rem 0 0 0', fontSize: '0.875rem', color: 'var(--color-text-muted)' }}>
                                        Create notes to document your learning progress!
                                    </p>
                                </div>
                            )}
                        </div>
                    ) : (
                        <div style={{ height: '100%', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', opacity: 0.6 }}>
                            <FolderOpen size={64} style={{ color: 'var(--color-text-muted)', marginBottom: '1.5rem' }} />
                            <h2 style={{ margin: 0, fontSize: '1.25rem' }}>Select a topic to view details</h2>
                        </div>
                    )}
                </main>
            </div>

            {editing && (
                <Modal title={isNew ? 'New Topic' : 'Edit Topic'} onClose={() => setEditing(null)}>
                    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Title *</label>
                            <input className="input" name="title" defaultValue={editing.title} required autoFocus />
                        </div>
                        <div>
                            <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Description</label>
                            <textarea className="input" name="description" rows={3} defaultValue={editing.description ?? ''} style={{ resize: 'vertical' }} />
                        </div>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Proficiency</label>
                                <select className="input" name="proficiency" defaultValue={editing.proficiency ?? ''}>
                                    {PROFICIENCY.map((p) => <option key={p} value={p}>{p || 'None'}</option>)}
                                </select>
                            </div>
                            <div>
                                <label style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '0.375rem' }}>Parent Topic</label>
                                <select className="input" name="parentTopicId" defaultValue={editing.parentTopicId ?? ''}>
                                    <option value="">None (root)</option>
                                    {allTopics?.filter((t: any) => t.id !== editing.id).map((t: any) => (
                                        <option key={t.id} value={t.id}>{t.title}</option>
                                    ))}
                                </select>
                            </div>
                        </div>
                        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.875rem', cursor: 'pointer' }}>
                            <input type="checkbox" name="isPinned" defaultChecked={editing.isPinned} />
                            Pin this topic
                        </label>
                        <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end', marginTop: '0.5rem' }}>
                            <button type="button" className="btn btn-secondary" onClick={() => setEditing(null)}>Cancel</button>
                            <button type="submit" className="btn btn-primary" disabled={save.isPending}>
                                {save.isPending ? 'Saving...' : 'Save'}
                            </button>
                        </div>
                    </form>
                </Modal>
            )}

            {confirmDelete && (
                <Modal title="Delete Topic" onClose={() => setConfirmDelete(null)} width={380}>
                    <p style={{ marginBottom: '1.5rem' }}>
                        Delete <strong>{confirmDelete.title}</strong>? Child topics and notes linked to this topic may be affected.
                    </p>
                    <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
                        <button className="btn btn-secondary" onClick={() => setConfirmDelete(null)}>Cancel</button>
                        <button className="btn btn-danger" disabled={del.isPending}
                            onClick={() => del.mutate(confirmDelete.id!)}>
                            {del.isPending ? 'Deleting...' : 'Delete'}
                        </button>
                    </div>
                </Modal>
            )}

            <ToastContainer toasts={toasts} dismiss={dismiss} />
        </>
    );
}
