import { AppBar, Box, Button, Container, Toolbar, Typography } from '@mui/material';
import LogoutIcon from '@mui/icons-material/Logout';
import { Link as RouterLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';
import { authService } from '@/services/authService';

export function Layout() {
  const user = useAuthStore((s) => s.user);
  const navigate = useNavigate();

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
            sx={{ flexGrow: 1, color: 'inherit', textDecoration: 'none', fontWeight: 700 }}
            variant="h6"
          >
            ImovelStand
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, mr: 3 }}>
            <Button color="inherit" component={RouterLink} to="/apartamentos">Apartamentos</Button>
            <Button color="inherit" component={RouterLink} to="/clientes">Clientes</Button>
          </Box>
          {user ? (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Typography variant="body2" color="inherit">{user.nome}</Typography>
              <Button color="inherit" startIcon={<LogoutIcon />} onClick={handleLogout}>Sair</Button>
            </Box>
          ) : null}
        </Toolbar>
      </AppBar>
      <Container maxWidth="lg" sx={{ py: 3, flex: 1 }}>
        <Outlet />
      </Container>
    </Box>
  );
}
