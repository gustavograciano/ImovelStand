import { Card, CardContent, Grid, Typography } from '@mui/material';
import { useAuthStore } from '@/stores/authStore';

export function HomePage() {
  const user = useAuthStore((s) => s.user);

  return (
    <Grid container spacing={2}>
      <Grid item xs={12}>
        <Typography variant="h4" fontWeight={700}>
          Olá, {user?.nome ?? 'usuário'}
        </Typography>
        <Typography color="text.secondary">Resumo rápido do seu dia no ImovelStand.</Typography>
      </Grid>
      <Grid item xs={12} md={4}>
        <Card>
          <CardContent>
            <Typography variant="overline">Apartamentos</Typography>
            <Typography variant="h5">Ver catálogo</Typography>
            <Typography variant="body2" color="text.secondary">
              Consulte a grade completa do empreendimento.
            </Typography>
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12} md={4}>
        <Card>
          <CardContent>
            <Typography variant="overline">Clientes</Typography>
            <Typography variant="h5">CRM</Typography>
            <Typography variant="body2" color="text.secondary">
              Funil, histórico e LGPD.
            </Typography>
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12} md={4}>
        <Card>
          <CardContent>
            <Typography variant="overline">Propostas</Typography>
            <Typography variant="h5">Em aberto</Typography>
            <Typography variant="body2" color="text.secondary">
              Acompanhe o status do funil comercial.
            </Typography>
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
}
