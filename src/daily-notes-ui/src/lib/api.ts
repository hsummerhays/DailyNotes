import axios from 'axios';
import { notifyAuthInvalid } from './authEvents';

const api = axios.create({
    baseURL: '/api',
    headers: { 'Content-Type': 'application/json' },
});

// JWT interceptor — attach token to every request
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers = config.headers ?? {};
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Response interceptor — handle 401 with refresh token
api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config as (typeof error.config & { _retry?: boolean });
        if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
            originalRequest._retry = true;
            const refreshToken = localStorage.getItem('refreshToken');
            if (refreshToken) {
                try {
                    const { data } = await axios.post('/api/auth/refresh', { refreshToken });
                    localStorage.setItem('token', data.token);
                    localStorage.setItem('refreshToken', data.refreshToken);
                    originalRequest.headers = originalRequest.headers ?? {};
                    originalRequest.headers.Authorization = `Bearer ${data.token}`;
                    return api(originalRequest);
                } catch {
                    notifyAuthInvalid();
                }
            }
            else
            {
                notifyAuthInvalid();
            }
        }
        return Promise.reject(error);
    }
);

export default api;
