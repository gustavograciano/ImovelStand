import { AppBar, Box, Button, Container, Stack, Toolbar, Typography } from '@mui/material';
import LogoutIcon from '@mui/icons-material/Logout';
import { Link as RouterLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';
import { authService } from '@/services/authService';

const NAV_ITEMS = [
  { to: '/', label: 'Início' },
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/apartamentos', label: 'Apartamentos' },
  { to: '/clientes', label: 'Clientes' },
  { to: '/propostas', label: 'Propostas' },
  { to: '/vendas', label: 'Vendas' }
];

export function Layout() {
  const user = useAuthStore((s) => s.user);
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = async () => {
    await authService.logout();
    navigate('/login', { replace: true });
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <AppBar position="sticky" color="primary" elevation={0}>
        <Toolbar>
          <Typography
            component={RouterLink}
            to="/"
            sx={{ color: 'inherit', textDecoration: 'none', fontWeight: 700, mr: 4 }}
            variant="h6"
          >
            ImovelStand
          </Typography>
          <Stack direction="row" spacing={1} sx={{ flexGrow: 1 }}>
            {NAV_ITEMS.map((item) => {
              const active = location.pathname === item.to
                || (item.to !== '/' && location.pathname.startsWith(item.to));
              return (
                <Button
                  key={item.to}
                  component={RouterLink}
                  to={item.to}
                  color="inherit"
                  variant="text"
                  sx={{
                    fontWeight: active ? 700 : 400,
                    textDecoration: active ? 'underline' : 'none',
                    textUnderlineOffset: 6
                  }}
                >
                  {item.label}
                </Button>
              );
            })}
          </Stack>
          {user ? (
            <Stack direction="row" alignItems="center" spacing={1}>
              <Typography variant="body2" color="inherit" sx={{ opacity: 0.9 }}>
                {user.nome} · {user.role}
              </Typography>
              <Button color="inherit" startIcon={<LogoutIcon />} onClick={handleLogout}>
                Sair
              </Button>
            </Stack>
          ) : null}
        </Toolbar>
      </AppBar>
      <Container maxWidth="xl" sx={{ py: 3, flex: 1 }}>
        <Outlet />
      </Container>
    </Box>
  );
}
