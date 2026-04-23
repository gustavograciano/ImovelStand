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

const TEMA_LABEL: Record<string, string> = {
  preco: 'Preço',
  'prazo-entrega': 'Prazo de entrega',
  localizacao: 'Localização',
  financiamento: 'Financiamento',
  tamanho: 'Tamanho',
  valorizacao: 'Valorização',
  'confianca-construtora': 'Confiança na construtora'
};

/**
 * Card de objeções detectadas: mostra padrões recorrentes no histórico
 * do cliente com sugestão de contorno.
 */
export function ObjecoesCard({ clienteId }: Props) {
  const [enabled, setEnabled] = useState(false);

  const q = useQuery({
    queryKey: ['objecoes-cliente', clienteId],
    queryFn: () => copilotoService.analisarObjecoes(clienteId),
    enabled,
    staleTime: 1000 * 60 * 10
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
                  Objeções detectadas (IA)
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Padrões recorrentes no histórico e sugestões de contorno.
                </Typography>
              </Box>
            </Stack>
            <Button variant="outlined" startIcon={<AutoAwesomeIcon />} onClick={() => setEnabled(true)}>
              Analisar
            </Button>
          </Stack>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card variant="outlined">
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1.5 }}>
          <Stack direction="row" spacing={1} alignItems="center">
            <AutoAwesomeIcon color="primary" fontSize="small" />
            <Typography variant="subtitle2" fontWeight={700}>Objeções detectadas</Typography>
            {q.data?.doCache ? <Chip size="small" label="cache" variant="outlined" /> : null}
          </Stack>
          <Button size="small" startIcon={<RefreshIcon />} onClick={() => q.refetch()} disabled={q.isFetching}>
            Atualizar
          </Button>
        </Stack>

        {q.isLoading || q.isFetching ? (
          <Box sx={{ py: 2, display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <CircularProgress size={18} />
            <Typography variant="caption" color="text.secondary">Analisando histórico…</Typography>
          </Box>
        ) : q.isError || !q.data?.sucesso ? (
          <Alert severity="error">Não foi possível analisar agora.</Alert>
        ) : q.data.objecoes.length === 0 ? (
          <Alert severity="success" variant="outlined">
            Nenhuma objeção recorrente detectada no histórico recente.
          </Alert>
        ) : (
          <Stack spacing={1.5}>
            {q.data.objecoes.map((o, i) => (
              <Box
                key={i}
                sx={{
                  p: 1.5,
                  borderLeft: 3,
                  borderColor: 'warning.main',
                  bgcolor: (t) => t.palette.mode === 'dark' ? 'rgba(255,193,7,0.04)' : 'grey.50',
                  borderRadius: 1
                }}
              >
                <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={1}>
                  <Stack direction="row" spacing={1} alignItems="center">
                    <Typography variant="body2" fontWeight={600}>
                      {TEMA_LABEL[o.tema] ?? o.tema}
                    </Typography>
                    <Chip size="small" label={`${o.ocorrencias}x`} variant="outlined" />
                  </Stack>
                  <Typography variant="caption" color="text.secondary">
                    {o.ultimaMencao}
                  </Typography>
                </Stack>
                <Typography variant="caption" color="text.primary" sx={{ mt: 0.75, display: 'block' }}>
                  <strong>Sugestão:</strong> {o.sugestaoContorno}
                </Typography>
              </Box>
            ))}
          </Stack>
        )}
      </CardContent>
    </Card>
  );
}
