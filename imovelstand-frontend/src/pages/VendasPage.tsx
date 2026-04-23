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
import { vendasService } from '@/services/vendasService';
import { useAuthStore } from '@/stores/authStore';
import type { StatusVenda, VendaResponse } from '@/types/api';

const STATUS_COLORS: Record<StatusVenda, 'default' | 'primary' | 'warning' | 'success' | 'error'> = {
  Negociada: 'warning',
  EmContrato: 'primary',
  Assinada: 'success',
  Cancelada: 'error',
  Distratada: 'error'
};

const STATUS_OPTIONS: Array<StatusVenda | ''> = ['', 'Negociada', 'EmContrato', 'Assinada', 'Cancelada', 'Distratada'];

function formatBRL(v: number): string {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

export function VendasPage() {
  const [status, setStatus] = useState<StatusVenda | ''>('');
  const queryClient = useQueryClient();
  const userRole = useAuthStore((s) => s.user?.role);
  const podeAprovar = userRole === 'Admin' || userRole === 'Gerente';

  const { data, isLoading, isError } = useQuery({
    queryKey: ['vendas', status],
    queryFn: () => vendasService.list({ ...(status ? { status } : {}), pageSize: 100 })
  });

  const aprovarMutation = useMutation({
    mutationFn: (id: number) => vendasService.aprovar(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['vendas'] })
  });

  const actionsFor = (v: VendaResponse) => {
    if (v.status === 'Negociada' && podeAprovar) {
      return (
        <Button
          size="small"
          color="success"
          onClick={() => aprovarMutation.mutate(v.id)}
          disabled={aprovarMutation.isPending}
        >
          Aprovar
        </Button>
      );
    }
    return null;
  };

  return (
    <Stack spacing={3}>
      <Paper sx={{ p: 2 }}>
        <TextField
          select
          size="small"
          label="Status"
          value={status}
          onChange={(e) => setStatus(e.target.value as StatusVenda | '')}
          sx={{ minWidth: 200 }}
        >
          {STATUS_OPTIONS.map((s) => (
            <MenuItem key={s || 'all'} value={s}>{s || 'Todos'}</MenuItem>
          ))}
        </TextField>
      </Paper>

      {isError ? <Alert severity="error">Erro ao carregar vendas.</Alert> : null}

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
                  <TableCell>Fechamento</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Ações</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {(data?.items ?? []).map((v) => (
                  <TableRow key={v.id} hover>
                    <TableCell>{v.numero}</TableCell>
                    <TableCell>{v.clienteNome ?? v.clienteId}</TableCell>
                    <TableCell>{v.apartamentoNumero ?? v.apartamentoId}</TableCell>
                    <TableCell>{v.corretorNome ?? v.corretorId}</TableCell>
                    <TableCell align="right">{formatBRL(v.valorFinal)}</TableCell>
                    <TableCell>{new Date(v.dataFechamento).toLocaleDateString('pt-BR')}</TableCell>
                    <TableCell>
                      <Chip size="small" color={STATUS_COLORS[v.status]} label={v.status} />
                    </TableCell>
                    <TableCell align="right">{actionsFor(v)}</TableCell>
                  </TableRow>
                ))}
                {(data?.items ?? []).length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8}>
                      <Typography color="text.secondary" textAlign="center" py={3}>
                        Sem vendas.
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
