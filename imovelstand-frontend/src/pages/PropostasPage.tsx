import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  MenuItem,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { propostasService } from '@/services/propostasService';
import { NovaPropostaDialog } from '@/components/NovaPropostaDialog';
import type { PropostaResponse, StatusProposta } from '@/types/api';

const STATUS_COLORS: Record<StatusProposta, 'default' | 'primary' | 'warning' | 'success' | 'error' | 'info'> = {
  Rascunho: 'default',
  Enviada: 'primary',
  ContrapropostaCliente: 'warning',
  ContrapropostaCorretor: 'warning',
  Aceita: 'success',
  Reprovada: 'error',
  Expirada: 'error',
  Cancelada: 'default'
};

const STATUS_OPTIONS: Array<StatusProposta | ''> = ['', 'Rascunho', 'Enviada', 'Aceita', 'Reprovada', 'Expirada', 'Cancelada'];

function formatBRL(v: number): string {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

export function PropostasPage() {
  const [status, setStatus] = useState<StatusProposta | ''>('');
  const [novaOpen, setNovaOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data, isLoading, isError } = useQuery({
    queryKey: ['propostas', status],
    queryFn: () => propostasService.list({ ...(status ? { status } : {}), pageSize: 100 })
  });

  const enviarMutation = useMutation({
    mutationFn: (id: number) => propostasService.enviar(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['propostas'] })
  });

  const aceitarMutation = useMutation({
    mutationFn: (id: number) => propostasService.alterarStatus(id, 'Aceita', 'Aceita pelo cliente'),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['propostas'] })
  });

  const reprovarMutation = useMutation({
    mutationFn: (id: number) => propostasService.alterarStatus(id, 'Reprovada', 'Reprovada'),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['propostas'] })
  });

  const actionsFor = (p: PropostaResponse) => {
    const isDraft = p.status === 'Rascunho';
    const isActive = p.status === 'Enviada' || p.status === 'ContrapropostaCliente' || p.status === 'ContrapropostaCorretor';
    return (
      <Stack direction="row" spacing={1}>
        {isDraft ? (
          <Button
            size="small"
            variant="outlined"
            onClick={() => enviarMutation.mutate(p.id)}
            disabled={enviarMutation.isPending}
          >
            Enviar
          </Button>
        ) : null}
        {isActive ? (
          <>
            <Button
              size="small"
              color="success"
              onClick={() => aceitarMutation.mutate(p.id)}
              disabled={aceitarMutation.isPending}
            >
              Aceitar
            </Button>
            <Button
              size="small"
              color="error"
              variant="outlined"
              onClick={() => reprovarMutation.mutate(p.id)}
              disabled={reprovarMutation.isPending}
            >
              Reprovar
            </Button>
          </>
        ) : null}
      </Stack>
    );
  };

  return (
    <Stack spacing={3}>
      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="space-between" alignItems={{ sm: 'center' }}>
          <TextField
            select
            size="small"
            label="Status"
            value={status}
            onChange={(e) => setStatus(e.target.value as StatusProposta | '')}
            sx={{ minWidth: 200 }}
          >
            {STATUS_OPTIONS.map((s) => (
              <MenuItem key={s || 'all'} value={s}>{s || 'Todos'}</MenuItem>
            ))}
          </TextField>
          <Button startIcon={<AddIcon />} onClick={() => setNovaOpen(true)}>
            Nova proposta
          </Button>
        </Stack>
      </Paper>

      <NovaPropostaDialog open={novaOpen} onClose={() => setNovaOpen(false)} />

      {isError ? <Alert severity="error">Erro ao carregar propostas.</Alert> : null}

      <Paper sx={{ width: "100%" }}>
        {isLoading ? (
          <Box sx={{ p: 4, display: 'grid', placeItems: 'center' }}>
            <CircularProgress />
          </Box>
        ) : (
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Número</TableCell>
                  <TableCell>Cliente</TableCell>
                  <TableCell>Apto</TableCell>
                  <TableCell>Corretor</TableCell>
                  <TableCell align="right">Valor</TableCell>
                  <TableCell>Versão</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Validade</TableCell>
                  <TableCell align="right">Ações</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {(data?.items ?? []).map((p) => (
                  <TableRow key={p.id} hover>
                    <TableCell>{p.numero}</TableCell>
                    <TableCell>{p.clienteNome ?? p.clienteId}</TableCell>
                    <TableCell>{p.apartamentoNumero ?? p.apartamentoId}</TableCell>
                    <TableCell>{p.corretorNome ?? p.corretorId}</TableCell>
                    <TableCell align="right">{formatBRL(p.valorOferecido)}</TableCell>
                    <TableCell>v{p.versao}</TableCell>
                    <TableCell>
                      <Chip size="small" color={STATUS_COLORS[p.status]} label={p.status} />
                    </TableCell>
                    <TableCell>{p.dataValidade ? new Date(p.dataValidade).toLocaleDateString('pt-BR') : '—'}</TableCell>
                    <TableCell align="right">{actionsFor(p)}</TableCell>
                  </TableRow>
                ))}
                {(data?.items ?? []).length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={9}>
                      <Typography color="text.secondary" textAlign="center" py={3}>
                        Sem propostas para o filtro selecionado.
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : null}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Paper>
    </Stack>
  );
}
