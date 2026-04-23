import { lazy, Suspense, useMemo } from 'react';
import { Box, CircularProgress, CssBaseline, ThemeProvider } from '@mui/material';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ErrorBoundary } from '@/components/ErrorBoundary';
import { Layout } from '@/components/Layout';
import { RequireAuth } from '@/components/RequireAuth';
import { LoginPage } from '@/pages/LoginPage';
import { useUiStore } from '@/stores/uiStore';
import { buildTheme } from '@/theme';

// Code-splitting: rotas autenticadas viram chunks separados.
const HomePage = lazy(() => import('@/pages/HomePage').then(m => ({ default: m.HomePage })));
const LandingPage = lazy(() => import('@/pages/LandingPage').then(m => ({ default: m.LandingPage })));
const OnboardingPage = lazy(() => import('@/pages/OnboardingPage').then(m => ({ default: m.OnboardingPage })));
const DashboardPage = lazy(() => import('@/pages/DashboardPage').then(m => ({ default: m.DashboardPage })));
const ApartamentosPage = lazy(() => import('@/pages/ApartamentosPage').then(m => ({ default: m.ApartamentosPage })));
const EmpreendimentosPage = lazy(() => import('@/pages/EmpreendimentosPage').then(m => ({ default: m.EmpreendimentosPage })));
const ClientesPage = lazy(() => import('@/pages/ClientesPage').then(m => ({ default: m.ClientesPage })));
const ClienteDetailPage = lazy(() => import('@/pages/ClienteDetailPage').then(m => ({ default: m.ClienteDetailPage })));
const PropostasPage = lazy(() => import('@/pages/PropostasPage').then(m => ({ default: m.PropostasPage })));
const VendasPage = lazy(() => import('@/pages/VendasPage').then(m => ({ default: m.VendasPage })));
const UsuariosPage = lazy(() => import('@/pages/UsuariosPage').then(m => ({ default: m.UsuariosPage })));
const SimuladorPage = lazy(() => import('@/pages/SimuladorPage').then(m => ({ default: m.SimuladorPage })));
const PrecificacaoPage = lazy(() => import('@/pages/PrecificacaoPage').then(m => ({ default: m.PrecificacaoPage })));

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
      refetchOnWindowFocus: false
    }
  }
});

function PageLoader() {
  return (
    <Box sx={{ display: 'grid', placeItems: 'center', minHeight: '50vh' }}>
      <CircularProgress />
    </Box>
  );
}

export default function App() {
  const mode = useUiStore((s) => s.mode);
  const theme = useMemo(() => buildTheme(mode), [mode]);

  return (
    <ErrorBoundary>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <QueryClientProvider client={queryClient}>
          <BrowserRouter>
            <Suspense fallback={<PageLoader />}>
              <Routes>
                <Route path="/landing" element={<LandingPage />} />
                <Route path="/onboarding" element={<OnboardingPage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route
                  element={
                    <RequireAuth>
                      <Layout />
                    </RequireAuth>
                  }
                >
                  <Route index element={<HomePage />} />
                  <Route path="dashboard" element={<DashboardPage />} />
                  <Route path="empreendimentos" element={<EmpreendimentosPage />} />
                  <Route path="apartamentos" element={<ApartamentosPage />} />
                  <Route path="clientes" element={<ClientesPage />} />
                  <Route path="clientes/:id" element={<ClienteDetailPage />} />
                  <Route path="propostas" element={<PropostasPage />} />
                  <Route path="vendas" element={<VendasPage />} />
                  <Route path="usuarios" element={<UsuariosPage />} />
                  <Route path="simulador" element={<SimuladorPage />} />
                  <Route path="precificacao" element={<PrecificacaoPage />} />
                </Route>
                <Route path="*" element={<Navigate to="/" replace />} />
              </Routes>
            </Suspense>
          </BrowserRouter>
        </QueryClientProvider>
      </ThemeProvider>
    </ErrorBoundary>
  );
}
