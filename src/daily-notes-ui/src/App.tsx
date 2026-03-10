import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEffect } from 'react';
import { useAuthStore } from './stores/authStore';
import AppLayout from './components/AppLayout';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import WorkDaysPage from './pages/WorkDaysPage';
import TasksPage from './pages/TasksPage';
import ProjectsPage from './pages/ProjectsPage';
import NotesPage from './pages/NotesPage';
import TopicsPage from './pages/TopicsPage';
import CoursesPage from './pages/CoursesPage';
import QuizzesPage from './pages/QuizzesPage';
import TagsPage from './pages/TagsPage';
import PayPeriodsPage from './pages/PayPeriodsPage';
import SearchPage from './pages/SearchPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isHydrating } = useAuthStore();
  if (isHydrating) return null;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return <>{children}</>;
}

export default function App() {
  const { hydrate } = useAuthStore();

  useEffect(() => {
    hydrate();
  }, [hydrate]);

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<DashboardPage />} />
            <Route path="work-days" element={<WorkDaysPage />} />
            <Route path="tasks" element={<TasksPage />} />
            <Route path="projects" element={<ProjectsPage />} />
            <Route path="notes" element={<NotesPage />} />
            <Route path="topics" element={<TopicsPage />} />
            <Route path="courses" element={<CoursesPage />} />
            <Route path="quizzes" element={<QuizzesPage />} />
            <Route path="tags" element={<TagsPage />} />
            <Route path="pay-periods" element={<PayPeriodsPage />} />
            <Route path="search" element={<SearchPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
