import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  LinearProgress,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import { precificacaoService, type SugestaoPreco } from '@/services/precificacaoService';

const STATUS_OPTIONS = ['pendente', 'aceita', 'rejeitada', 'expirada', 'todas'];

function brl(n: number): string {
  return n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

export function PrecificacaoPage() {
  const [status, setStatus] = useState('pendente');
  const [rejeitarTarget, setRejeitarTarget] = useState<SugestaoPreco | null>(null);
  const [motivoRejeicao, setMotivoRejeicao] = useState('');
  const queryClient = useQueryClient();

  const q = useQuery({
    queryKey: ['sugestoes-preco', status],
    queryFn: () => precificacaoService.listar(status)
  });

  const aceitar = useMutation({
    mutationFn: (id: number) => precificacaoService.aceitar(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sugestoes-preco'] });
      queryClient.invalidateQueries({ queryKey: ['apartamentos'] });
    }
  });

  const rejeitar = useMutation({
    mutationFn: ({ id, motivo }: { id: number; motivo: string }) =>
      precificacaoService.rejeitar(id, motivo),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sugestoes-preco'] });
      setRejeitarTarget(null);
      setMotivoRejeicao('');
    }
  });

  const recalcular = useMutation({
    mutationFn: () => precificacaoService.recalcularTenant(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sugestoes-preco'] });
    }
  });

  const dinheiroPotencial = q.data?.dinheiroPotencial ?? 0;
  const items = q.data?.items ?? [];

  return (
    <Stack spacing={3}>
      {/* Hero */}
      <Paper
        sx={{
          p: 3,
          background: (t) =>
            t.palette.mode === 'dark'
              ? 'linear-gradient(135deg, rgba(34,197,94,0.08), rgba(99,102,241,0.04))'
              : 'linear-gradient(135deg, rgba(34,197,94,0.05), rgba(99,102,241,0.02))',
          border: '1px solid',
          borderColor: 'divider'
        }}
      >
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="space-between" alignItems={{ sm: 'center' }}>
          <Stack>
            <Stack direction="row" spacing={1} alignItems="center">
              <AutoAwesomeIcon color="primary" />
              <Typography variant="h6" fontWeight={700}>Precificação Dinâmica</Typography>
            </Stack>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              Sugestões automáticas de ajuste baseadas em velocidade de venda e benchmarks de mercado.
            </Typography>
            {dinheiroPotencial > 0 ? (
              <Typography variant="h4" fontWeight={700} color="success.main" sx={{ mt: 2, letterSpacing: '-0.02em' }}>
                {brl(dinheiroPotencial)}
              </Typography>
            ) : null}
            {dinheiroPotencial > 0 ? (
              <Typography variant="caption" color="text.secondary">
                de potencial adicional com os aumentos pendentes
              </Typography>
            ) : null}
          </Stack>
          <Button
            variant="contained"
            size="large"
            onClick={() => recalcular.mutate()}
            disabled={recalcular.isPending}
          >
            {recalcular.isPending ? 'Calculando...' : 'Recalcular tudo'}
          </Button>
        </Stack>
        {recalcular.data ? (
          <Alert severity="success" sx={{ mt: 2 }}>
            {recalcular.data.geradas} nova{recalcular.data.geradas === 1 ? '' : 's'} sugestão{recalcular.data.geradas === 1 ? '' : 'ões'} gerada{recalcular.data.geradas === 1 ? '' : 's'}.
          </Alert>
        ) : null}
      </Paper>

      {/* Filtro */}
      <Paper sx={{ p: 2 }}>
        <TextField
          select
          size="small"
          label="Status"
          value={status}
          onChange={(e) => setStatus(e.target.value)}
          sx={{ minWidth: 200 }}
        >
          {STATUS_OPTIONS.map((s) => (
            <MenuItem key={s} value={s}>{s}</MenuItem>
          ))}
        </TextField>
      </Paper>

      {/* Cards */}
      {q.isLoading ? (
        <Box sx={{ p: 4, display: 'grid', placeItems: 'center' }}>
          <CircularProgress />
        </Box>
      ) : q.isError ? (
        <Alert severity="error">Erro ao carregar sugestões.</Alert>
      ) : items.length === 0 ? (
        <Alert severity="info" variant="outlined">
          Nenhuma sugestão {status !== 'todas' ? status : ''} no momento. Clique em "Recalcular tudo" para rodar o motor.
        </Alert>
      ) : (
        <Grid container spacing={2}>
          {items.map((s) => (
            <Grid item xs={12} md={6} lg={4} key={s.id}>
              <SugestaoCard
                s={s}
                onAceitar={() => aceitar.mutate(s.id)}
                onRejeitar={() => setRejeitarTarget(s)}
                disabled={aceitar.isPending || rejeitar.isPending || s.status !== 'pendente'}
              />
            </Grid>
          ))}
        </Grid>
      )}

      {/* Dialog rejeitar */}
      <Dialog open={!!rejeitarTarget} onClose={() => setRejeitarTarget(null)} maxWidth="sm" fullWidth>
        <DialogTitle>Rejeitar sugestão</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <Typography variant="body2" color="text.secondary">
              Por que não aplicar {rejeitarTarget?.variacaoPct && rejeitarTarget.variacaoPct > 0 ? 'o aumento' : 'o desconto'} em{' '}
              <strong>{rejeitarTarget?.torreNome} — {rejeitarTarget?.apartamentoNumero}</strong>?
            </Typography>
            <TextField
              label="Motivo (ajuda o motor a aprender)"
              multiline
              minRows={2}
              fullWidth
              value={motivoRejeicao}
              onChange={(e) => setMotivoRejeicao(e.target.value)}
              placeholder="Ex: estratégia comercial decidiu manter preço"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRejeitarTarget(null)} variant="text">Cancelar</Button>
          <Button
            color="error"
            onClick={() => rejeitarTarget && rejeitar.mutate({ id: rejeitarTarget.id, motivo: motivoRejeicao })}
            disabled={rejeitar.isPending}
          >
            Rejeitar
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}

function SugestaoCard({
  s,
  onAceitar,
  onRejeitar,
  disabled
}: {
  s: SugestaoPreco;
  onAceitar: () => void;
  onRejeitar: () => void;
  disabled: boolean;
}) {
  const isAumento = s.variacaoPct > 0;
  const isActive = s.status === 'pendente';

  return (
    <Card
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        borderLeft: 4,
        borderLeftColor: isAumento ? 'success.main' : 'warning.main',
        opacity: isActive ? 1 : 0.7
      }}
    >
      <CardContent sx={{ flex: 1 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start" sx={{ mb: 1.5 }}>
          <Box>
            <Typography variant="caption" color="text.secondary">
              {s.torreNome ?? '—'} · {s.tipologiaNome ?? '—'}
            </Typography>
            <Typography variant="h6" fontWeight={700}>
              Apto {s.apartamentoNumero ?? `#${s.apartamentoId}`}
            </Typography>
          </Box>
          <Chip
            size="small"
            icon={isAumento ? <TrendingUpIcon /> : <TrendingDownIcon />}
            label={`${s.variacaoPct > 0 ? '+' : ''}${s.variacaoPct.toFixed(2)}%`}
            color={isAumento ? 'success' : 'warning'}
          />
        </Stack>

        <Stack direction="row" justifyContent="space-between" sx={{ my: 1.5 }}>
          <Box>
            <Typography variant="caption" color="text.secondary">Preço atual</Typography>
            <Typography variant="body2" sx={{ textDecoration: 'line-through', color: 'text.secondary' }}>
              {brl(s.precoAtual)}
            </Typography>
          </Box>
          <Box sx={{ textAlign: 'right' }}>
            <Typography variant="caption" color="text.secondary">Sugerido</Typography>
            <Typography variant="body1" fontWeight={700} color={isAumento ? 'success.main' : 'warning.main'}>
              {brl(s.precoSugerido)}
            </Typography>
          </Box>
        </Stack>

        <Typography variant="body2" sx={{ mb: 1.5, lineHeight: 1.5 }}>
          {s.justificativa}
        </Typography>

        <Stack direction="row" spacing={1} alignItems="center" sx={{ mb: 1 }}>
          <Typography variant="caption" color="text.secondary">Confiança:</Typography>
          <Box sx={{ flex: 1 }}>
            <LinearProgress
              variant="determinate"
              value={s.confianca}
              sx={{ height: 6, borderRadius: 3 }}
              color={s.confianca >= 70 ? 'success' : s.confianca >= 50 ? 'primary' : 'warning'}
            />
          </Box>
          <Typography variant="caption" fontWeight={600}>{s.confianca}%</Typography>
        </Stack>

        <Stack direction="row" spacing={1} flexWrap="wrap">
          <Chip size="small" variant="outlined" label={s.motivo} />
          <Chip size="small" variant="outlined" label={`${s.velocidadeVendaSemanal.toFixed(1)}/sem`} />
          {!isActive ? <Chip size="small" label={s.status} color="default" /> : null}
        </Stack>
      </CardContent>

      {isActive ? (
        <Box sx={{ px: 2, pb: 2, display: 'flex', gap: 1 }}>
          <Button
            fullWidth
            variant="contained"
            color="success"
            startIcon={<CheckCircleIcon />}
            onClick={onAceitar}
            disabled={disabled}
          >
            Aceitar
          </Button>
          <Button
            fullWidth
            variant="outlined"
            color="error"
            startIcon={<CancelIcon />}
            onClick={onRejeitar}
            disabled={disabled}
          >
            Rejeitar
          </Button>
        </Box>
      ) : null}
    </Card>
  );
}
