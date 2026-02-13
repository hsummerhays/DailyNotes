import { create } from 'zustand';
import api from '../lib/api';

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

export const useAuthStore = create<AuthState>((set) => ({
    token: null,
    refreshToken: null,
    tenantId: null,
    role: null,
    isAuthenticated: false,
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
        const token = localStorage.getItem('token');
        const refreshToken = localStorage.getItem('refreshToken');
        if (token) {
            set({ token, refreshToken, isAuthenticated: true });
        }
    },
}));
