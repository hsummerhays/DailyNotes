import { useState, useCallback, useRef } from 'react';

type ToastType = 'success' | 'error' | 'info';

interface Toast {
    id: number;
    message: string;
    type: ToastType;
}

const COLOR: Record<ToastType, string> = {
    success: 'var(--color-success)',
    error: 'var(--color-danger)',
    info: 'var(--color-accent)',
};

const ICON: Record<ToastType, string> = {
    success: '✓',
    error: '✕',
    info: 'ℹ',
};

let nextId = 1;

export function useToast() {
    const [toasts, setToasts] = useState<Toast[]>([]);
    const timers = useRef<Record<number, ReturnType<typeof setTimeout>>>({});

    const dismiss = useCallback((id: number) => {
        clearTimeout(timers.current[id]);
        delete timers.current[id];
        setToasts((prev) => prev.filter((t) => t.id !== id));
    }, []);

    const toast = useCallback((message: string, type: ToastType = 'info') => {
        const id = nextId++;
        setToasts((prev) => [...prev, { id, message, type }]);
        timers.current[id] = setTimeout(() => dismiss(id), 4000);
    }, [dismiss]);

    return { toasts, toast, dismiss };
}

export function ToastContainer({ toasts, dismiss }: { toasts: ReturnType<typeof useToast>['toasts']; dismiss: (id: number) => void }) {
    return (
        <div style={{
            position: 'fixed', bottom: '1.5rem', right: '1.5rem',
            zIndex: 200, display: 'flex', flexDirection: 'column', gap: '0.5rem',
        }}>
            {toasts.map((t) => (
                <div
                    key={t.id}
                    className="animate-fade-in"
                    style={{
                        display: 'flex', alignItems: 'center', gap: '0.75rem',
                        padding: '0.875rem 1.25rem',
                        background: 'var(--color-bg-elevated)',
                        border: `1px solid ${COLOR[t.type]}`,
                        borderRadius: 'var(--radius-sm)',
                        boxShadow: 'var(--shadow-elevated)',
                        minWidth: 280, maxWidth: 400,
                        fontSize: '0.875rem',
                    }}
                >
                    <span style={{
                        width: 22, height: 22, borderRadius: '50%',
                        background: COLOR[t.type], color: '#fff',
                        display: 'flex', alignItems: 'center', justifyContent: 'center',
                        fontSize: '0.75rem', fontWeight: 700, flexShrink: 0,
                    }}>{ICON[t.type]}</span>
                    <span style={{ flex: 1 }}>{t.message}</span>
                    <button
                        onClick={() => dismiss(t.id)}
                        style={{ background: 'none', border: 'none', color: 'var(--color-text-muted)', cursor: 'pointer', fontSize: '1rem' }}
                    >✕</button>
                </div>
            ))}
        </div>
    );
}
