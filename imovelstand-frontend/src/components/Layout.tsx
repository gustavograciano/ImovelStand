import { useMemo } from 'react';
import {
  Avatar,
  Box,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
  Tooltip,
  Typography
} from '@mui/material';
import { useTheme } from '@mui/material/styles';
import ApartmentIcon from '@mui/icons-material/Apartment';
import DashboardIcon from '@mui/icons-material/Dashboard';
import DescriptionIcon from '@mui/icons-material/Description';
import DarkModeIcon from '@mui/icons-material/DarkMode';
import LightModeIcon from '@mui/icons-material/LightMode';
import LogoutIcon from '@mui/icons-material/Logout';
import PeopleAltIcon from '@mui/icons-material/PeopleAlt';
import PointOfSaleIcon from '@mui/icons-material/PointOfSale';
import SpaceDashboardIcon from '@mui/icons-material/SpaceDashboard';
import { Link as RouterLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { authService } from '@/services/authService';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';

const DRAWER_WIDTH = 248;

interface NavItem {
  to: string;
  label: string;
  icon: React.ReactNode;
  end?: boolean;
}

const NAV: NavItem[] = [
  { to: '/', label: 'Início', icon: <SpaceDashboardIcon fontSize="small" />, end: true },
  { to: '/dashboard', label: 'Dashboard', icon: <DashboardIcon fontSize="small" /> },
  { to: '/apartamentos', label: 'Apartamentos', icon: <ApartmentIcon fontSize="small" /> },
  { to: '/clientes', label: 'Clientes', icon: <PeopleAltIcon fontSize="small" /> },
  { to: '/propostas', label: 'Propostas', icon: <DescriptionIcon fontSize="small" /> },
  { to: '/vendas', label: 'Vendas', icon: <PointOfSaleIcon fontSize="small" /> }
];

export function Layout() {
  const user = useAuthStore((s) => s.user);
  const mode = useUiStore((s) => s.mode);
  const toggleMode = useUiStore((s) => s.toggleMode);
  const navigate = useNavigate();
  const location = useLocation();
  const theme = useTheme();

  const avatarLabel = useMemo(() => {
    if (!user?.nome) return '??';
    const parts = user.nome.split(' ').filter(Boolean);
    return (parts[0]?.[0] ?? '') + (parts[parts.length - 1]?.[0] ?? '');
  }, [user]);

  const handleLogout = async () => {
    await authService.logout();
    navigate('/login', { replace: true });
  };

  const currentLabel =
    NAV.find((n) => (n.end ? location.pathname === n.to : location.pathname.startsWith(n.to)))?.label
    ?? (location.pathname.startsWith('/clientes/') ? 'Cliente' : 'ImovelStand');

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      <Drawer
        variant="permanent"
        open
        sx={{
          width: DRAWER_WIDTH,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            boxSizing: 'border-box',
            // Garante que a sidebar acompanha o modo do tema (dark/light)
            bgcolor: 'background.paper',
            borderRight: '1px solid',
            borderColor: 'divider'
          }
        }}
      >
        <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
          {/* Logo */}
          <Box sx={{ px: 2.5, py: 2.5 }}>
            <Stack direction="row" spacing={1.2} alignItems="center">
              <Box
                sx={{
                  width: 28,
                  height: 28,
                  borderRadius: 1.5,
                  background: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%)',
                  display: 'grid',
                  placeItems: 'center',
                  color: '#fff',
                  fontWeight: 800,
                  fontSize: 14
                }}
              >
                I
              </Box>
              <Typography variant="subtitle1" fontWeight={700} letterSpacing="-0.01em">
                ImovelStand
              </Typography>
            </Stack>
          </Box>

          <Divider />

          <List sx={{ px: 1.5, py: 1.5, flex: 1 }}>
            {NAV.map((item) => {
              const active = item.end
                ? location.pathname === item.to
                : location.pathname === item.to || location.pathname.startsWith(`${item.to}/`);
              return (
                <ListItem key={item.to} disablePadding sx={{ mb: 0.25 }}>
                  <ListItemButton
                    component={RouterLink}
                    to={item.to}
                    selected={active}
                    sx={{ py: 0.9, px: 1.25 }}
                  >
                    <ListItemIcon>{item.icon}</ListItemIcon>
                    <ListItemText
                      primary={item.label}
                      primaryTypographyProps={{ fontSize: '0.875rem', fontWeight: active ? 600 : 500 }}
                    />
                  </ListItemButton>
                </ListItem>
              );
            })}
          </List>

          <Divider />

          {/* User profile */}
          <Box sx={{ px: 1.5, py: 1.5 }}>
            <Stack direction="row" spacing={1.2} alignItems="center" sx={{ px: 1 }}>
              <Avatar
                sx={{
                  width: 32,
                  height: 32,
                  bgcolor: 'primary.main',
                  color: '#fff',
                  fontSize: '0.75rem',
                  fontWeight: 700
                }}
              >
                {avatarLabel.toUpperCase()}
              </Avatar>
              <Stack sx={{ flex: 1, minWidth: 0 }}>
                <Typography
                  variant="body2"
                  fontWeight={600}
                  noWrap
                  sx={{ lineHeight: 1.2 }}
                >
                  {user?.nome ?? '—'}
                </Typography>
                <Typography variant="caption" color="text.secondary" noWrap sx={{ lineHeight: 1.2 }}>
                  {user?.role ?? ''}
                </Typography>
              </Stack>
              <Tooltip title="Sair" arrow>
                <IconButton size="small" onClick={handleLogout}>
                  <LogoutIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Stack>
          </Box>
        </Box>
      </Drawer>

      {/* Main area */}
      <Box
        sx={{
          flex: 1,
          minWidth: 0,
          display: 'flex',
          flexDirection: 'column'
        }}
      >
        {/* Topbar */}
        <Box
          component="header"
          sx={{
            position: 'sticky',
            top: 0,
            zIndex: theme.zIndex.appBar,
            backdropFilter: 'blur(8px)',
            bgcolor: (t) =>
              t.palette.mode === 'dark'
                ? 'rgba(9,9,11,0.8)'
                : 'rgba(255,255,255,0.85)',
            borderBottom: '1px solid',
            borderColor: 'divider',
            px: 4,
            py: 2
          }}
        >
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Typography variant="h5" fontWeight={700} letterSpacing="-0.02em">
              {currentLabel}
            </Typography>
            <Stack direction="row" alignItems="center" spacing={0.5}>
              <Tooltip title={mode === 'dark' ? 'Modo claro' : 'Modo escuro'} arrow>
                <IconButton size="small" onClick={toggleMode}>
                  {mode === 'dark' ? <LightModeIcon fontSize="small" /> : <DarkModeIcon fontSize="small" />}
                </IconButton>
              </Tooltip>
            </Stack>
          </Stack>
        </Box>

        {/* Content */}
        <Box component="main" sx={{ flex: 1, p: 4, minWidth: 0 }}>
          <Outlet />
        </Box>
      </Box>
    </Box>
  );
}
