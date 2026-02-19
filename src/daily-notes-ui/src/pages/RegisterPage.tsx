import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

export default function RegisterPage() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [displayName, setDisplayName] = useState('');
    const { register, isLoading, error, isAuthenticated } = useAuthStore();
    const navigate = useNavigate();

    useEffect(() => {
        if (isAuthenticated) navigate('/', { replace: true });
    }, [isAuthenticated, navigate]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await register(email, password, displayName);
            navigate('/');
        } catch {
            // error shown via store
        }
    };

    return (
        <div style={{
            minHeight: '100vh',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            background: 'linear-gradient(135deg, #0f172a 0%, #1e1b4b 50%, #0f172a 100%)',
        }}>
            <div style={{
                width: '100%',
                maxWidth: 420,
                padding: '2.5rem',
                background: 'var(--color-bg-card)',
                borderRadius: 'var(--radius)',
                border: '1px solid var(--color-border)',
                boxShadow: 'var(--shadow-elevated)',
            }}>
                <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
                    <h1 style={{
                        fontSize: '1.75rem',
                        fontWeight: 700,
                        background: 'linear-gradient(135deg, #818cf8, #06b6d4)',
                        WebkitBackgroundClip: 'text',
                        WebkitTextFillColor: 'transparent',
                        marginBottom: '0.5rem',
                    }}>Create Account</h1>
                    <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>
                        Start your productivity journey
                    </p>
                </div>

                {error && (
                    <div style={{
                        padding: '0.75rem',
                        background: 'rgba(239, 68, 68, 0.1)',
                        border: '1px solid rgba(239, 68, 68, 0.3)',
                        borderRadius: 'var(--radius-sm)',
                        color: 'var(--color-danger)',
                        fontSize: '0.875rem',
                        marginBottom: '1rem',
                    }}>{error}</div>
                )}

                <form onSubmit={handleSubmit}>
                    <div style={{ marginBottom: '1rem' }}>
                        <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--color-text-secondary)', marginBottom: '0.375rem' }}>
                            Display Name
                        </label>
                        <input
                            className="input"
                            type="text"
                            value={displayName}
                            onChange={(e) => setDisplayName(e.target.value)}
                            placeholder="John Doe"
                            required
                            autoFocus
                            autoComplete="name"
                        />
                    </div>

                    <div style={{ marginBottom: '1rem' }}>
                        <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--color-text-secondary)', marginBottom: '0.375rem' }}>
                            Email
                        </label>
                        <input
                            className="input"
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="you@example.com"
                            required
                            autoComplete="email"
                        />
                    </div>

                    <div style={{ marginBottom: '1.5rem' }}>
                        <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--color-text-secondary)', marginBottom: '0.375rem' }}>
                            Password
                        </label>
                        <input
                            className="input"
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder="••••••••"
                            required
                            minLength={8}
                            autoComplete="new-password"
                        />
                    </div>

                    <button className="btn btn-primary" type="submit" disabled={isLoading} style={{ width: '100%', justifyContent: 'center' }}>
                        {isLoading ? <span className="spinner" /> : 'Create Account'}
                    </button>
                </form>

                <p style={{
                    textAlign: 'center',
                    marginTop: '1.5rem',
                    fontSize: '0.8rem',
                    color: 'var(--color-text-muted)',
                }}>
                    Already have an account?{' '}
                    <Link to="/login" style={{ color: 'var(--color-primary-light)', textDecoration: 'none' }}>
                        Sign in
                    </Link>
                </p>
            </div>
        </div>
    );
}
