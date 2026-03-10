import axios from 'axios';
import { notifyAuthInvalid } from './authEvents';

const api = axios.create({
    baseURL: '/api',
    headers: { 'Content-Type': 'application/json' },
    withCredentials: true,
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers = config.headers ?? {};
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config as (typeof error.config & { _retry?: boolean });
        if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
            originalRequest._retry = true;
            try {
                // Cookie sent automatically — no body needed
                const { data } = await axios.post('/api/auth/refresh', null, { withCredentials: true });
                localStorage.setItem('token', data.token);
                originalRequest.headers = originalRequest.headers ?? {};
                originalRequest.headers.Authorization = `Bearer ${data.token}`;
                return api(originalRequest);
            } catch {
                notifyAuthInvalid();
            }
        }
        return Promise.reject(error);
    }
);

export default api;
