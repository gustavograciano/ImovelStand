import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Card,
  CardContent,
  CircularProgress,
  Grid,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography
} from '@mui/material';
import { dashboardService } from '@/services/dashboardService';

function formatBRL(v: number): string {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
}
function formatPct(v: number): string {
  return (v * 100).toFixed(1) + '%';
}

export function DashboardPage() {
  const [empreendimentoId, setEmpreendimentoId] = useState(1);

  const overview = useQuery({
    queryKey: ['dashboard-overview', empreendimentoId],
    queryFn: () => dashboardService.overview(empreendimentoId),
    enabled: empreendimentoId > 0
  });
  const funil = useQuery({
    queryKey: ['dashboard-funil'],
    queryFn: () => dashboardService.funil(90)
  });
  const ranking = useQuery({
    queryKey: ['dashboard-ranking'],
    queryFn: () => dashboardService.ranking()
  });

  return (
    <Stack spacing={3}>
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ sm: 'center' }} spacing={2}>
        <Typography variant="h4" fontWeight={700}>Dashboard</Typography>
        <TextField
          type="number"
          size="small"
          label="Empreendimento ID"
          value={empreendimentoId}
          onChange={(e) => setEmpreendimentoId(Number(e.target.value) || 0)}
          sx={{ maxWidth: 200 }}
        />
      </Stack>

      {overview.isError ? <Alert severity="error">Erro ao carregar overview.</Alert> : null}

      <Grid container spacing={2}>
        {overview.isLoading ? (
          <Grid item xs={12}>
            <Box sx={{ p: 4, display: 'grid', placeItems: 'center' }}>
              <CircularProgress />
            </Box>
          </Grid>
        ) : overview.data ? (
          <>
            <KpiCard title="Unidades totais" value={overview.data.unidadesTotal.toString()} color="#2563eb" />
            <KpiCard title="Vendidas" value={overview.data.unidadesVendidas.toString()} color="#dc2626" />
            <KpiCard title="VGV Total" value={formatBRL(overview.data.vgvTotal)} color="#7c3aed" />
            <KpiCard title="VGV Vendido" value={formatBRL(overview.data.vgvVendido)} color="#16a34a" />
            <KpiCard title="% Vendido" value={formatPct(overview.data.pctVendido)} color="#ea580c" />
            <KpiCard title="Preço médio / m²" value={formatBRL(overview.data.precoMedioM2)} color="#0891b2" />
            <KpiCard title="Vendas 30d" value={overview.data.vendasUltimos30Dias.toString()} color="#ca8a04" />
            <KpiCard title="Velocidade semanal" value={overview.data.velocidadeVendaSemanal.toFixed(1)} color="#6366f1" />
          </>
        ) : null}
      </Grid>

      <Grid container spacing={2}>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" fontWeight={700} gutterBottom>
              Funil (últimos 90 dias)
            </Typography>
            {funil.isLoading ? (
              <CircularProgress size={20} />
            ) : funil.data ? (
              <Stack spacing={1}>
                <FunnelRow label="Leads" value={funil.data.leads} pct={1} />
                <FunnelRow label="Visitas" value={funil.data.visitas} pct={funil.data.leads ? funil.data.visitas / funil.data.leads : 0} />
                <FunnelRow label="Propostas" value={funil.data.propostas} pct={funil.data.leads ? funil.data.propostas / funil.data.leads : 0} />
                <FunnelRow label="Vendas" value={funil.data.vendas} pct={funil.data.leads ? funil.data.vendas / funil.data.leads : 0} />
                <Box sx={{ pt: 1, borderTop: '1px solid', borderColor: 'divider' }}>
                  <Typography variant="caption" color="text.secondary">
                    Conversão global: <b>{formatPct(funil.data.conversaoGlobal)}</b>
                  </Typography>
                </Box>
              </Stack>
            ) : null}
          </Paper>
        </Grid>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" fontWeight={700} gutterBottom>
              Ranking de corretores (por VGV)
            </Typography>
            {ranking.isLoading ? (
              <CircularProgress size={20} />
            ) : (
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Corretor</TableCell>
                    <TableCell align="right">Vendas</TableCell>
                    <TableCell align="right">VGV</TableCell>
                    <TableCell align="right">Ticket médio</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {(ranking.data ?? []).slice(0, 10).map((r) => (
                    <TableRow key={r.corretorId}>
                      <TableCell>{r.nome}</TableCell>
                      <TableCell align="right">{r.vendasFechadas}</TableCell>
                      <TableCell align="right">{formatBRL(r.vgvVendido)}</TableCell>
                      <TableCell align="right">{formatBRL(r.ticketMedio)}</TableCell>
                    </TableRow>
                  ))}
                  {(ranking.data ?? []).length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={4}>
                        <Typography color="text.secondary" textAlign="center" py={2}>
                          Sem corretores cadastrados.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : null}
                </TableBody>
              </Table>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Stack>
  );
}

function KpiCard({ title, value, color }: { title: string; value: string; color: string }) {
  return (
    <Grid item xs={12} sm={6} md={3}>
      <Card>
        <CardContent>
          <Typography variant="caption" color="text.secondary">{title}</Typography>
          <Typography variant="h5" fontWeight={700} sx={{ color, mt: 0.5 }}>
            {value}
          </Typography>
        </CardContent>
      </Card>
    </Grid>
  );
}

function FunnelRow({ label, value, pct }: { label: string; value: number; pct: number }) {
  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="body2">{label}</Typography>
        <Typography variant="body2" fontWeight={600}>
          {value} <Typography component="span" variant="caption" color="text.secondary">({formatPct(pct)})</Typography>
        </Typography>
      </Stack>
      <Box
        sx={{
          mt: 0.5,
          height: 8,
          borderRadius: 4,
          bgcolor: 'grey.200',
          overflow: 'hidden'
        }}
      >
        <Box
          sx={{
            width: `${Math.min(100, pct * 100)}%`,
            height: '100%',
            bgcolor: 'primary.main',
            transition: 'width 0.3s'
          }}
        />
      </Box>
    </Box>
  );
}
