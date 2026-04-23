import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link as RouterLink } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Link,
  Stack,
  Typography
} from '@mui/material';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import RefreshIcon from '@mui/icons-material/Refresh';
import { copilotoService, type AcaoSugerida } from '@/services/copilotoService';

const PRIORIDADE_COLOR: Record<AcaoSugerida['prioridade'], 'error' | 'warning' | 'default'> = {
  urgente: 'error',
  alta: 'warning',
  media: 'default'
};

const PRIORIDADE_ORDEM: Record<AcaoSugerida['prioridade'], number> = {
  urgente: 0,
  alta: 1,
  media: 2
};

/**
 * Widget "Sua fila de hoje" na HomePage: ações priorizadas pela IA
 * baseadas no portfólio do corretor (clientes + propostas + SLAs).
 */
export function FilaCorretorCard() {
  const [enabled, setEnabled] = useState(false);

  const q = useQuery({
    queryKey: ['proximas-acoes'],
    queryFn: () => copilotoService.proximasAcoes(),
    enabled,
    staleTime: 1000 * 60 * 10 // 10 min (ações não mudam tão rápido)
  });

  const acoes = (q.data?.acoes ?? [])
    .slice()
    .sort((a, b) => PRIORIDADE_ORDEM[a.prioridade] - PRIORIDADE_ORDEM[b.prioridade]);

  return (
    <Card>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
          <Stack direction="row" spacing={1} alignItems="center">
            <AutoAwesomeIcon color="primary" fontSize="small" />
            <Typography variant="subtitle1" fontWeight={700}>
              Sua fila de hoje
            </Typography>
          </Stack>
          {enabled ? (
            <Button size="small" startIcon={<RefreshIcon />} onClick={() => q.refetch()} disabled={q.isFetching}>
              Atualizar
            </Button>
          ) : null}
        </Stack>

        {!enabled ? (
          <Stack spacing={1.5}>
            <Typography variant="body2" color="text.secondary">
              A IA analisa seus clientes, propostas e últimas interações para sugerir ações prioritárias para hoje.
            </Typography>
            <Button variant="outlined" startIcon={<AutoAwesomeIcon />} onClick={() => setEnabled(true)}>
              Gerar fila
            </Button>
          </Stack>
        ) : q.isLoading || q.isFetching ? (
          <Box sx={{ py: 3, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1.5 }}>
            <CircularProgress size={20} />
            <Typography variant="caption" color="text.secondary">Analisando sua carteira…</Typography>
          </Box>
        ) : q.isError ? (
          <Alert severity="error">Não foi possível gerar a fila.</Alert>
        ) : !q.data?.sucesso ? (
          <Alert severity="warning">{q.data?.mensagemErro ?? 'IA indisponível.'}</Alert>
        ) : acoes.length === 0 ? (
          <Alert severity="success" variant="outlined">
            Tudo em dia! Nenhuma ação crítica detectada agora.
          </Alert>
        ) : (
          <Stack spacing={1.25}>
            {acoes.map((a, i) => (
              <Box
                key={`${a.clienteId}-${i}`}
                sx={{
                  p: 1.5,
                  borderLeft: 3,
                  borderColor: a.prioridade === 'urgente' ? 'error.main' : a.prioridade === 'alta' ? 'warning.main' : 'divider',
                  bgcolor: (t) => t.palette.mode === 'dark' ? 'rgba(255,255,255,0.02)' : 'grey.50',
                  borderRadius: 1
                }}
              >
                <Stack direction="row" alignItems="center" justifyContent="space-between" spacing={1}>
                  <Link component={RouterLink} to={`/clientes/${a.clienteId}`} variant="body2" fontWeight={600} underline="hover">
                    {a.acao}
                  </Link>
                  <Chip size="small" label={a.prioridade} color={PRIORIDADE_COLOR[a.prioridade]} variant="outlined" />
                </Stack>
                <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: 'block' }}>
                  {a.justificativa}
                </Typography>
              </Box>
            ))}
          </Stack>
        )}
      </CardContent>
    </Card>
  );
}
