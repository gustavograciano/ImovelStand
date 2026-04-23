import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Stack,
  Typography
} from '@mui/material';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import RefreshIcon from '@mui/icons-material/Refresh';
import { copilotoService } from '@/services/copilotoService';

interface Props {
  clienteId: number;
}

/**
 * Card "Briefing IA" exibido no topo da página de detalhes do cliente.
 * Carrega sob demanda (lazy) para evitar custos em navegação casual.
 */
export function BriefingClienteCard({ clienteId }: Props) {
  const [enabled, setEnabled] = useState(false);

  const q = useQuery({
    queryKey: ['briefing-cliente', clienteId],
    queryFn: () => copilotoService.briefing(clienteId),
    enabled,
    staleTime: 1000 * 60 * 5 // 5 min
  });

  if (!enabled) {
    return (
      <Card variant="outlined" sx={{ borderStyle: 'dashed' }}>
        <CardContent>
          <Stack direction="row" spacing={2} alignItems="center" justifyContent="space-between">
            <Stack direction="row" spacing={1.5} alignItems="center">
              <AutoAwesomeIcon color="primary" />
              <Box>
                <Typography variant="subtitle2" fontWeight={700}>
                  Briefing do Cliente (IA)
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Resumo de 3-5 linhas preparando você para a próxima interação.
                </Typography>
              </Box>
            </Stack>
            <Button variant="outlined" startIcon={<AutoAwesomeIcon />} onClick={() => setEnabled(true)}>
              Gerar briefing
            </Button>
          </Stack>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card
      variant="outlined"
      sx={{
        borderColor: 'primary.main',
        background: (t) =>
          t.palette.mode === 'dark'
            ? 'linear-gradient(135deg, rgba(99,102,241,0.08), rgba(139,92,246,0.04))'
            : 'linear-gradient(135deg, rgba(99,102,241,0.04), rgba(139,92,246,0.02))'
      }}
    >
      <CardContent>
        <Stack direction="row" spacing={1} alignItems="center" justifyContent="space-between" sx={{ mb: 1.5 }}>
          <Stack direction="row" spacing={1} alignItems="center">
            <AutoAwesomeIcon color="primary" fontSize="small" />
            <Typography variant="subtitle2" fontWeight={700}>
              Briefing IA
            </Typography>
            {q.data?.doCache ? <Chip size="small" label="cache" variant="outlined" /> : null}
          </Stack>
          <Button size="small" startIcon={<RefreshIcon />} onClick={() => q.refetch()} disabled={q.isFetching}>
            Atualizar
          </Button>
        </Stack>

        {q.isLoading || q.isFetching ? (
          <Box sx={{ py: 2, display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <CircularProgress size={18} />
            <Typography variant="caption" color="text.secondary">
              Analisando histórico do cliente…
            </Typography>
          </Box>
        ) : q.isError ? (
          <Alert severity="error">Não foi possível gerar o briefing. Tente novamente em alguns instantes.</Alert>
        ) : !q.data?.sucesso ? (
          <Alert severity="warning">{q.data?.mensagemErro ?? 'Serviço de IA indisponível.'}</Alert>
        ) : (
          <Typography variant="body2" sx={{ whiteSpace: 'pre-line', lineHeight: 1.7 }}>
            {q.data.briefing}
          </Typography>
        )}

        {q.data?.sucesso ? (
          <Typography variant="caption" color="text.secondary" sx={{ mt: 1.5, display: 'block' }}>
            Gerado em {new Date(q.data.geradoEm).toLocaleString('pt-BR')}
            {q.data.custoUsd > 0 ? ` · custo US$ ${q.data.custoUsd.toFixed(4)}` : ''}
          </Typography>
        ) : null}
      </CardContent>
    </Card>
  );
}
