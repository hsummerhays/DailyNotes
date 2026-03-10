import { create } from 'zustand';
import axios from 'axios';
import api from '../lib/api';
import { setOnAuthInvalid } from '../lib/authEvents';

interface AuthState {
    token: string | null;
    tenantId: string | null;
    role: string | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    isHydrating: boolean;
    error: string | null;
    login: (email: string, password: string) => Promise<void>;
    register: (email: string, password: string, displayName: string) => Promise<void>;
    logout: () => Promise<void>;
    hydrate: () => Promise<void>;
}

function parseTokenClaims(token: string) {
    try {
        const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
        return {
            tenantId: (payload['tenant_id'] ?? null) as string | null,
            role: (payload['role'] ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? null) as string | null,
            exp: payload.exp as number | undefined,
        };
    } catch {
        return { tenantId: null, role: null, exp: undefined };
    }
}

export const useAuthStore = create<AuthState>((set) => ({
    token: null,
    tenantId: null,
    role: null,
    isAuthenticated: false,
    isLoading: false,
    isHydrating: true,
    error: null,

    login: async (email, password) => {
        set({ isLoading: true, error: null });
        try {
            const { data } = await api.post('/auth/login', { email, password });
            localStorage.setItem('token', data.token);
            const claims = parseTokenClaims(data.token);
            set({ token: data.token, tenantId: data.tenantId ?? claims.tenantId, role: data.role ?? claims.role, isAuthenticated: true, isLoading: false });
        } catch (err: unknown) {
            const message = axios.isAxiosError(err) ? (err.response?.data?.message ?? 'Login failed') : 'Login failed';
            set({ isLoading: false, error: message });
            throw err;
        }
    },

    register: async (email, password, displayName) => {
        set({ isLoading: true, error: null });
        try {
            const { data } = await api.post('/auth/register', { email, password, displayName });
            localStorage.setItem('token', data.token);
            const claims = parseTokenClaims(data.token);
            set({ token: data.token, tenantId: data.tenantId ?? claims.tenantId, role: data.role ?? claims.role, isAuthenticated: true, isLoading: false });
        } catch (err: unknown) {
            const message = axios.isAxiosError(err) ? (err.response?.data?.message ?? 'Registration failed') : 'Registration failed';
            set({ isLoading: false, error: message });
            throw err;
        }
    },

    logout: async () => {
        localStorage.removeItem('token');
        set({ token: null, tenantId: null, role: null, isAuthenticated: false });
        try { await api.post('/auth/logout'); } catch { /* best-effort */ }
    },

    hydrate: async () => {
        set({ isHydrating: true });
        const token = localStorage.getItem('token');
        if (token) {
            const { exp, tenantId, role } = parseTokenClaims(token);
            if (exp && Date.now() / 1000 < exp) {
                set({ token, tenantId, role, isAuthenticated: true, isHydrating: false });
                return;
            }
        }
        // Token missing or expired — try silent refresh via httpOnly cookie
        try {
            const { data } = await axios.post('/api/auth/refresh', null, { withCredentials: true });
            localStorage.setItem('token', data.token);
            const claims = parseTokenClaims(data.token);
            set({ token: data.token, tenantId: data.tenantId ?? claims.tenantId, role: data.role ?? claims.role, isAuthenticated: true, isHydrating: false });
        } catch {
            localStorage.removeItem('token');
            set({ token: null, tenantId: null, role: null, isAuthenticated: false, isHydrating: false });
        }
    },
}));

setOnAuthInvalid(() => { useAuthStore.getState().logout(); });
