import { create } from 'zustand';
import api from '../lib/api';
import { setOnAuthInvalid } from '../lib/authEvents';

interface AuthState {
    token: string | null;
    refreshToken: string | null;
    tenantId: string | null;
    role: string | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    error: string | null;
    login: (email: string, password: string) => Promise<void>;
    register: (email: string, password: string, displayName: string) => Promise<void>;
    logout: () => void;
    hydrate: () => void;
}

const getInitialState = () => {
    try {
        const token = localStorage.getItem('token');
        const refreshToken = localStorage.getItem('refreshToken');
        
        if (!token) {
            return {
                token: null,
                refreshToken: null,
                tenantId: null,
                role: null,
                isAuthenticated: false,
            };
        }

        // Decode the JWT payload (base64url) to restore claims without a network call
        const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
        const exp = payload.exp as number | undefined;
        
        // Treat token as invalid if it has already expired
        if (exp && Date.now() / 1000 > exp) {
            localStorage.removeItem('token');
            localStorage.removeItem('refreshToken');
            return {
                token: null,
                refreshToken: null,
                tenantId: null,
                role: null,
                isAuthenticated: false,
            };
        }

        return {
            token,
            refreshToken,
            tenantId: payload['tenant_id'] ?? null,
            role: payload['role'] ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? null,
            isAuthenticated: true,
        };
    } catch (error) {
        console.error('Failed to hydrate auth state:', error);
        return {
            token: null,
            refreshToken: null,
            tenantId: null,
            role: null,
            isAuthenticated: false,
        };
    }
};

const initialState = getInitialState();

export const useAuthStore = create<AuthState>((set) => ({
    ...initialState,
    isLoading: false,
    error: null,

    login: async (email, password) => {
        set({ isLoading: true, error: null });
        try {
            const { data } = await api.post('/auth/login', { email, password });
            localStorage.setItem('token', data.token);
            localStorage.setItem('refreshToken', data.refreshToken);
            set({
                token: data.token,
                refreshToken: data.refreshToken,
                tenantId: data.tenantId,
                role: data.role,
                isAuthenticated: true,
                isLoading: false,
            });
        } catch (err: any) {
            set({
                isLoading: false,
                error: err.response?.data?.message || 'Login failed',
            });
            throw err;
        }
    },

    register: async (email, password, displayName) => {
        set({ isLoading: true, error: null });
        try {
            const { data } = await api.post('/auth/register', { email, password, displayName });
            localStorage.setItem('token', data.token);
            localStorage.setItem('refreshToken', data.refreshToken);
            set({
                token: data.token,
                refreshToken: data.refreshToken,
                tenantId: data.tenantId,
                role: data.role,
                isAuthenticated: true,
                isLoading: false,
            });
        } catch (err: any) {
            set({
                isLoading: false,
                error: err.response?.data?.message || 'Registration failed',
            });
            throw err;
        }
    },

    logout: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        set({
            token: null,
            refreshToken: null,
            tenantId: null,
            role: null,
            isAuthenticated: false,
        });
    },

    hydrate: () => {
        const state = getInitialState();
        set(state);
    },
}));

setOnAuthInvalid(() => {
    useAuthStore.getState().logout();
});
