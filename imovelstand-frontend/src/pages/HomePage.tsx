import { Link as RouterLink } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  Grid,
  Stack,
  Typography
} from '@mui/material';
import { alpha, useTheme } from '@mui/material/styles';
import ApartmentIcon from '@mui/icons-material/Apartment';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import DescriptionIcon from '@mui/icons-material/Description';
import PeopleAltIcon from '@mui/icons-material/PeopleAlt';
import PointOfSaleIcon from '@mui/icons-material/PointOfSale';
import ShowChartIcon from '@mui/icons-material/ShowChart';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import { useAuthStore } from '@/stores/authStore';

interface ShortcutProps {
  to: string;
  icon: React.ReactNode;
  title: string;
  description: string;
  accent: string;
}

function Shortcut({ to, icon, title, description, accent }: ShortcutProps) {
  const theme = useTheme();
  return (
    <Card
      component={RouterLink}
      to={to}
      sx={{
        textDecoration: 'none',
        display: 'block',
        height: '100%',
        transition: 'all 160ms ease',
        '&:hover': {
          borderColor: alpha(accent, 0.6),
          transform: 'translateY(-2px)',
          boxShadow: `0 8px 24px -12px ${alpha(accent, 0.35)}`
        }
      }}
    >
      <CardContent sx={{ p: 2.5 }}>
        <Stack spacing={1.25}>
          <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
            <Box
              sx={{
                width: 36,
                height: 36,
                borderRadius: 1.5,
                display: 'grid',
                placeItems: 'center',
                bgcolor: alpha(accent, theme.palette.mode === 'dark' ? 0.18 : 0.12),
                color: accent
              }}
            >
              {icon}
            </Box>
            <ArrowForwardIcon
              sx={{ color: 'text.disabled', fontSize: 18, transition: 'color 160ms' }}
            />
          </Stack>
          <Typography variant="subtitle1" fontWeight={600} color="text.primary">
            {title}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.5 }}>
            {description}
          </Typography>
        </Stack>
      </CardContent>
    </Card>
  );
}

function StatCard({ label, value, hint, accent }: { label: string; value: string; hint?: string; accent?: string }) {
  const theme = useTheme();
  return (
    <Card sx={{ height: '100%' }}>
      <CardContent sx={{ p: 2.5 }}>
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1, letterSpacing: '0.04em', textTransform: 'uppercase', fontWeight: 600 }}>
          {label}
        </Typography>
        <Typography
          variant="h4"
          fontWeight={700}
          sx={{
            color: accent ?? 'text.primary',
            fontVariantNumeric: 'tabular-nums',
            letterSpacing: '-0.02em'
          }}
        >
          {value}
        </Typography>
        {hint ? (
          <Stack direction="row" spacing={0.5} alignItems="center" sx={{ mt: 1 }}>
            <TrendingUpIcon sx={{ fontSize: 14, color: theme.palette.success.main }} />
            <Typography variant="caption" color="text.secondary">
              {hint}
            </Typography>
          </Stack>
        ) : null}
      </CardContent>
    </Card>
  );
}

export function HomePage() {
  const user = useAuthStore((s) => s.user);
  const firstName = user?.nome?.split(' ')[0] ?? 'usuário';
  const hora = new Date().getHours();
  const saudacao = hora < 6 ? 'Boa madrugada' : hora < 12 ? 'Bom dia' : hora < 18 ? 'Boa tarde' : 'Boa noite';

  return (
    <Stack spacing={4} sx={{ maxWidth: 1440 }}>
      {/* Hero */}
      <Box>
        <Stack direction="row" spacing={1.5} alignItems="center" sx={{ mb: 0.75 }}>
          <Chip
            label={user?.role ?? 'Membro'}
            size="small"
            variant="outlined"
            sx={{ height: 22, fontSize: 11 }}
          />
          <Typography variant="caption" color="text.secondary">
            {new Date().toLocaleDateString('pt-BR', { weekday: 'long', day: '2-digit', month: 'long' })}
          </Typography>
        </Stack>
        <Typography variant="h3" fontWeight={700} sx={{ letterSpacing: '-0.03em', lineHeight: 1.1 }}>
          {saudacao}, {firstName}.
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mt: 1, maxWidth: 600 }}>
          Um resumo rápido do seu dia — navegue direto pros módulos ou veja os indicadores do empreendimento.
        </Typography>
      </Box>

      {/* Stats */}
      <Grid container spacing={2}>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard label="Unidades ativas" value="48" hint="2 torres, 3 tipologias" />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard label="Disponíveis" value="48" accent="#10b981" hint="100% livres" />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard label="Em negociação" value="0" accent="#f59e0b" hint="Nenhuma proposta aberta" />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard label="VGV Total" value="R$ 23M" accent="#6366f1" hint="estimado para o empreendimento" />
        </Grid>
      </Grid>

      <Divider />

      {/* Shortcuts */}
      <Box>
        <Typography variant="overline" color="text.secondary" sx={{ fontWeight: 700, letterSpacing: '0.1em' }}>
          Atalhos
        </Typography>
        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          <Grid item xs={12} sm={6} md={4} lg={3}>
            <Shortcut
              to="/dashboard"
              icon={<ShowChartIcon fontSize="small" />}
              title="Dashboard"
              description="KPIs, funil de conversão, ranking de corretores e alertas em tempo real."
              accent="#6366f1"
            />
          </Grid>
          <Grid item xs={12} sm={6} md={4} lg={3}>
            <Shortcut
              to="/apartamentos"
              icon={<ApartmentIcon fontSize="small" />}
              title="Apartamentos"
              description="Catálogo completo em lista ou espelho visual por torre e pavimento."
              accent="#3b82f6"
            />
          </Grid>
          <Grid item xs={12} sm={6} md={4} lg={3}>
            <Shortcut
              to="/clientes"
              icon={<PeopleAltIcon fontSize="small" />}
              title="Clientes"
              description="Kanban de funil, timeline de interações, consentimento LGPD."
              accent="#10b981"
            />
          </Grid>
          <Grid item xs={12} sm={6} md={4} lg={3}>
            <Shortcut
              to="/propostas"
              icon={<DescriptionIcon fontSize="small" />}
              title="Propostas"
              description="State machine: Rascunho → Enviada → Contraproposta → Aceita."
              accent="#f59e0b"
            />
          </Grid>
          <Grid item xs={12} sm={6} md={4} lg={3}>
            <Shortcut
              to="/vendas"
              icon={<PointOfSaleIcon fontSize="small" />}
              title="Vendas"
              description="Workflow de aprovação, comissões por corretor, contrato assinado."
              accent="#ef4444"
            />
          </Grid>
        </Grid>
      </Box>

      {/* CTA */}
      <Card sx={{ p: 0 }}>
        <CardContent sx={{ p: 3 }}>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={3} alignItems={{ md: 'center' }} justifyContent="space-between">
            <Stack spacing={0.5} sx={{ flex: 1 }}>
              <Typography variant="subtitle1" fontWeight={600}>
                Pronto pra gerar o espelho executivo?
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Baixe o relatório em PDF com KPIs + distribuição por status + breakdown por torre.
              </Typography>
            </Stack>
            <Stack direction="row" spacing={1}>
              <Button component={RouterLink} to="/dashboard" variant="outlined">
                Abrir dashboard
              </Button>
              <Button component={RouterLink} to="/apartamentos" endIcon={<ArrowForwardIcon fontSize="small" />}>
                Ver espelho
              </Button>
            </Stack>
          </Stack>
        </CardContent>
      </Card>
    </Stack>
  );
}
