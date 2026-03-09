import { useState, useEffect, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import { useToast } from './Toast';

interface QuickCaptureProps {
    isOpen: boolean;
    onClose: () => void;
}

export default function QuickCapture({ isOpen, onClose }: QuickCaptureProps) {
    const qc = useQueryClient();
    const { toast } = useToast();
    const [type, setType] = useState<'note' | 'task'>('note');
    const [content, setContent] = useState('');
    const [targetId, setTargetId] = useState(''); // workTaskId for notes, projectId for tasks
    const inputRef = useRef<HTMLTextAreaElement>(null);

    // Provide hotkey inside the component since we want it available globally
    useEffect(() => {
        const handleKey = (e: KeyboardEvent) => {
            if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
                e.preventDefault();
                if (!isOpen) { /* the parent should open it */ }
            }
        };
        document.addEventListener('keydown', handleKey);
        return () => document.removeEventListener('keydown', handleKey);
    }, [isOpen]);

    useEffect(() => {
        if (isOpen) {
            setContent('');
            inputRef.current?.focus();
        }
    }, [isOpen]);

    const { data: tasks } = useQuery({
        queryKey: ['tasks', 'all'],
        queryFn: () => api.get('/work-tasks').then(r => r.data),
        enabled: isOpen && type === 'note',
    });

    const { data: projects } = useQuery({
        queryKey: ['projects'],
        queryFn: () => api.get('/projects').then(r => r.data),
        enabled: isOpen && type === 'task',
    });

    const saveNote = useMutation({
        mutationFn: (noteContent: string) => {
            const today = new Date().toISOString().split('T')[0];
            return api.post('/work-notes', {
                noteDate: today,
                workTaskId: targetId ? Number(targetId) : null,
                content: { text: noteContent },
                timeMinutes: 0,
                isPinned: false,
                visibility: 'private'
            });
        },
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['notes'] });
            toast('Quick note saved!', 'success');
            onClose();
        },
        onError: () => toast('Failed to save note. Task may be required.', 'error')
    });

    const saveTask = useMutation({
        mutationFn: (taskName: string) => {
            return api.post('/work-tasks', {
                name: taskName,
                status: 'pending',
                projectId: targetId ? Number(targetId) : null,
                isPinned: false
            });
        },
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['tasks'] });
            toast('Quick task created!', 'success');
            onClose();
        },
        onError: () => toast('Failed to create task. Project may be required.', 'error')
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!content.trim()) return;

        if (type === 'note') {
            saveNote.mutate(content);
        } else {
            saveTask.mutate(content);
        }
    };

    if (!isOpen) return null;

    return (
        <div
            onClick={onClose}
            style={{
                position: 'fixed', inset: 0, zIndex: 9999,
                background: 'rgba(0,0,0,0.6)', backdropFilter: 'blur(4px)',
                display: 'flex', alignItems: 'flex-start', justifyContent: 'center', paddingTop: '15vh'
            }}
        >
            <div
                className="card"
                onClick={e => e.stopPropagation()}
                style={{ width: '100%', maxWidth: '600px', display: 'flex', flexDirection: 'column', overflow: 'hidden', boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.5)' }}
            >
                <div style={{ padding: '0.75rem 1.25rem', borderBottom: '1px solid var(--color-border)', display: 'flex', justifyContent: 'space-between', alignItems: 'center', background: 'var(--color-bg-elevated)' }}>
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                        <button
                            type="button"
                            onClick={() => { setType('note'); setTargetId(''); }}
                            className={`btn ${type === 'note' ? 'btn-primary' : ''}`}
                            style={{ padding: '0.3rem 0.6rem', fontSize: '0.8rem', background: type === 'note' ? '' : 'transparent', color: type === 'note' ? '' : 'var(--color-text-muted)' }}
                        >
                            📝 Note
                        </button>
                        <button
                            type="button"
                            onClick={() => { setType('task'); setTargetId(''); }}
                            className={`btn ${type === 'task' ? 'btn-primary' : ''}`}
                            style={{ padding: '0.3rem 0.6rem', fontSize: '0.8rem', background: type === 'task' ? '' : 'transparent', color: type === 'task' ? '' : 'var(--color-text-muted)' }}
                        >
                            ✅ Task
                        </button>
                    </div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                        Press <kbd style={{ background: 'var(--color-bg-base)', padding: '0.1rem 0.3rem', borderRadius: '3px' }}>Esc</kbd> to close
                    </div>
                </div>

                <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column' }}>
                    <textarea
                        ref={inputRef}
                        value={content}
                        onChange={e => setContent(e.target.value)}
                        onKeyDown={e => {
                            if (e.key === 'Enter' && (e.metaKey || e.ctrlKey)) {
                                handleSubmit(e);
                            }
                        }}
                        placeholder={type === 'note' ? "What's on your mind?" : "What needs to be done?"}
                        style={{
                            width: '100%', border: 'none', outline: 'none', background: 'transparent',
                            padding: '1.25rem', fontSize: '1.1rem', color: 'var(--color-text)', resize: 'none', minHeight: '120px'
                        }}
                        autoFocus
                    />

                    <div style={{ padding: '0.75rem 1.25rem', borderTop: '1px solid var(--color-border)', display: 'flex', gap: '1rem', alignItems: 'center', background: 'var(--color-bg-elevated)' }}>
                        <div style={{ flex: 1 }}>
                            {type === 'note' ? (
                                <select
                                    className="input"
                                    value={targetId}
                                    onChange={e => setTargetId(e.target.value)}
                                    style={{ padding: '0.4rem', fontSize: '0.85rem', width: '100%' }}
                                    required
                                >
                                    <option value="" disabled>Link to a task...</option>
                                    {tasks?.map((t: any) => <option key={t.id} value={t.id}>{t.name}</option>)}
                                </select>
                            ) : (
                                <select
                                    className="input"
                                    value={targetId}
                                    onChange={e => setTargetId(e.target.value)}
                                    style={{ padding: '0.4rem', fontSize: '0.85rem', width: '100%' }}
                                    required
                                >
                                    <option value="" disabled>In project...</option>
                                    {projects?.map((p: any) => <option key={p.id} value={p.id}>{p.name}</option>)}
                                </select>
                            )}
                        </div>
                        <button
                            type="submit"
                            className="btn btn-primary"
                            disabled={!content.trim() || !targetId || saveNote.isPending || saveTask.isPending}
                        >
                            {saveNote.isPending || saveTask.isPending ? 'Saving...' : 'Capture'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
