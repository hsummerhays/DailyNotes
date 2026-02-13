import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

const navItems = [
    { to: '/', label: 'Dashboard', icon: '📊' },
    { to: '/work-days', label: 'Work Days', icon: '📅' },
    { to: '/tasks', label: 'Tasks', icon: '✅' },
    { to: '/projects', label: 'Projects', icon: '📁' },
    { to: '/notes', label: 'Notes', icon: '📝' },
    { to: '/topics', label: 'Knowledge', icon: '🧠' },
    { to: '/courses', label: 'Courses', icon: '🎓' },
    { to: '/quizzes', label: 'Quizzes', icon: '❓' },
    { to: '/tags', label: 'Tags', icon: '🏷️' },
    { to: '/pay-periods', label: 'Pay Periods', icon: '💰' },
    { to: '/search', label: 'Search', icon: '🔍' },
];

export default function AppLayout() {
    const { logout } = useAuthStore();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <div className="app-layout">
            <aside className="sidebar">
                <div style={{
                    padding: '1.25rem 1.5rem',
                    borderBottom: '1px solid var(--color-border)',
                }}>
                    <h1 style={{
                        fontSize: '1.25rem',
                        fontWeight: 700,
                        background: 'linear-gradient(135deg, #818cf8, #06b6d4)',
                        WebkitBackgroundClip: 'text',
                        WebkitTextFillColor: 'transparent',
                    }}>DailyNotes</h1>
                </div>

                <nav style={{ flex: 1, padding: '0.75rem', overflowY: 'auto' }}>
                    {navItems.map((item) => (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            end={item.to === '/'}
                            style={({ isActive }) => ({
                                display: 'flex',
                                alignItems: 'center',
                                gap: '0.75rem',
                                padding: '0.625rem 0.875rem',
                                borderRadius: 'var(--radius-sm)',
                                fontSize: '0.875rem',
                                color: isActive ? 'var(--color-text)' : 'var(--color-text-secondary)',
                                background: isActive ? 'var(--color-bg-elevated)' : 'transparent',
                                textDecoration: 'none',
                                marginBottom: '2px',
                                transition: 'all 0.15s ease',
                            })}
                        >
                            <span style={{ fontSize: '1rem' }}>{item.icon}</span>
                            {item.label}
                        </NavLink>
                    ))}
                </nav>

                <div style={{
                    padding: '1rem 1.5rem',
                    borderTop: '1px solid var(--color-border)',
                }}>
                    <button
                        onClick={handleLogout}
                        className="btn btn-secondary"
                        style={{ width: '100%', justifyContent: 'center', fontSize: '0.8rem' }}
                    >
                        Sign Out
                    </button>
                </div>
            </aside>

            <main className="main-content">
                <Outlet />
            </main>
        </div>
    );
}
