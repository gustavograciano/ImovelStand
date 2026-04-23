import React, { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Collapse,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
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
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import AddIcon from '@mui/icons-material/Add';
import { vendasService } from '@/services/vendasService';
import { useAuthStore } from '@/stores/authStore';
import { NovaVendaDialog } from '@/components/NovaVendaDialog';
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
  const [novaOpen, setNovaOpen] = useState(false);
  const [expanded, setExpanded] = useState<number | null>(null);
  const [contratoVenda, setContratoVenda] = useState<VendaResponse | null>(null);
  const [contratoUrl, setContratoUrl] = useState('');
  const queryClient = useQueryClient();
  const userRole = useAuthStore((s) => s.user?.role);
  const podeAprovar = userRole === 'Admin' || userRole === 'Gerente';
  const podeCriar = userRole === 'Admin' || userRole === 'Gerente' || userRole === 'Corretor';

  const { data, isLoading, isError } = useQuery({
    queryKey: ['vendas', status],
    queryFn: () => vendasService.list({ ...(status ? { status } : {}), pageSize: 100 })
  });

  const aprovarMutation = useMutation({
    mutationFn: (id: number) => vendasService.aprovar(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['vendas'] })
  });

  const assinarMutation = useMutation({
    mutationFn: ({ id, url }: { id: number; url: string }) => vendasService.marcarContratoAssinado(id, url),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vendas'] });
      setContratoVenda(null);
      setContratoUrl('');
    }
  });

  const actionsFor = (v: VendaResponse) => {
    return (
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        {v.status === 'Negociada' && podeAprovar ? (
          <Button
            size="small"
            color="success"
            onClick={() => aprovarMutation.mutate(v.id)}
            disabled={aprovarMutation.isPending}
          >
            Aprovar
          </Button>
        ) : null}
        {v.status === 'EmContrato' ? (
          <Button
            size="small"
            color="primary"
            onClick={() => { setContratoVenda(v); setContratoUrl(v.contratoUrl ?? ''); }}
          >
            Assinar contrato
          </Button>
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
            onChange={(e) => setStatus(e.target.value as StatusVenda | '')}
            sx={{ minWidth: 200 }}
          >
            {STATUS_OPTIONS.map((s) => (
              <MenuItem key={s || 'all'} value={s}>{s || 'Todos'}</MenuItem>
            ))}
          </TextField>
          {podeCriar ? (
            <Button startIcon={<AddIcon />} onClick={() => setNovaOpen(true)}>
              Nova venda
            </Button>
          ) : null}
        </Stack>
      </Paper>

      <NovaVendaDialog open={novaOpen} onClose={() => setNovaOpen(false)} />

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
                  <TableCell width={40} />
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
                  <React.Fragment key={v.id}>
                    <TableRow hover>
                      <TableCell>
                        <IconButton size="small" onClick={() => setExpanded(expanded === v.id ? null : v.id)}>
                          {expanded === v.id ? <KeyboardArrowUpIcon fontSize="small" /> : <KeyboardArrowDownIcon fontSize="small" />}
                        </IconButton>
                      </TableCell>
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
                    <TableRow>
                      <TableCell colSpan={9} sx={{ py: 0, border: 0 }}>
                        <Collapse in={expanded === v.id} timeout="auto" unmountOnExit>
                          <Box sx={{ py: 2, px: 1 }}>
                            <Typography variant="subtitle2" fontWeight={700} gutterBottom>Comissões</Typography>
                            {v.comissoes.length === 0 ? (
                              <Typography variant="caption" color="text.secondary">Sem comissões geradas.</Typography>
                            ) : (
                              <Table size="small">
                                <TableHead>
                                  <TableRow>
                                    <TableCell>Corretor</TableCell>
                                    <TableCell>Tipo</TableCell>
                                    <TableCell align="right">Percentual</TableCell>
                                    <TableCell align="right">Valor</TableCell>
                                    <TableCell>Status</TableCell>
                                  </TableRow>
                                </TableHead>
                                <TableBody>
                                  {v.comissoes.map((c) => (
                                    <TableRow key={c.id}>
                                      <TableCell>{c.usuarioNome ?? `#${c.usuarioId}`}</TableCell>
                                      <TableCell>{c.tipo}</TableCell>
                                      <TableCell align="right">{c.percentual}%</TableCell>
                                      <TableCell align="right">{formatBRL(c.valor)}</TableCell>
                                      <TableCell><Chip size="small" label={c.status} /></TableCell>
                                    </TableRow>
                                  ))}
                                </TableBody>
                              </Table>
                            )}
                            {v.contratoUrl ? (
                              <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                                Contrato: <a href={v.contratoUrl} target="_blank" rel="noreferrer">{v.contratoUrl}</a>
                              </Typography>
                            ) : null}
                          </Box>
                        </Collapse>
                      </TableCell>
                    </TableRow>
                  </React.Fragment>
                ))}
                {(data?.items ?? []).length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={9}>
                      <Typography color="text.secondary" textAlign="center" py={3}>
                        {status ? `Sem vendas com status "${status}". Tente limpar o filtro.` : 'Nenhuma venda registrada ainda.'}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : null}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Paper>

      <Dialog open={!!contratoVenda} onClose={() => setContratoVenda(null)} maxWidth="sm" fullWidth>
        <DialogTitle>Marcar contrato assinado</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <Typography variant="body2" color="text.secondary">
              Venda {contratoVenda?.numero} — {contratoVenda?.clienteNome}
            </Typography>
            <TextField
              label="URL do contrato"
              placeholder="https://..."
              fullWidth
              value={contratoUrl}
              onChange={(e) => setContratoUrl(e.target.value)}
              helperText="Link para o PDF do contrato assinado"
            />
            {assinarMutation.isError ? <Alert severity="error">Erro ao marcar assinado.</Alert> : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button variant="text" onClick={() => setContratoVenda(null)}>Cancelar</Button>
          <Button
            onClick={() => contratoVenda && contratoUrl && assinarMutation.mutate({ id: contratoVenda.id, url: contratoUrl })}
            disabled={!contratoUrl || assinarMutation.isPending}
          >
            {assinarMutation.isPending ? 'Salvando...' : 'Confirmar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
