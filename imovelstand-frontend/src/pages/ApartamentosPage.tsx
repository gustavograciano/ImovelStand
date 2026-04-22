import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Alert,
  Box,
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
  TablePagination,
  TextField,
  Typography
} from '@mui/material';
import { apartamentosService, type ApartamentoFilter } from '@/services/apartamentosService';
import type { StatusApartamento } from '@/types/api';

const statusOptions: Array<{ value: StatusApartamento | ''; label: string }> = [
  { value: '', label: 'Todos' },
  { value: 'Disponivel', label: 'Disponível' },
  { value: 'Reservado', label: 'Reservado' },
  { value: 'Proposta', label: 'Em proposta' },
  { value: 'Vendido', label: 'Vendido' }
];

const statusColors: Record<StatusApartamento, 'success' | 'warning' | 'info' | 'error' | 'default'> = {
  Disponivel: 'success',
  Reservado: 'warning',
  Proposta: 'info',
  Vendido: 'error',
  Bloqueado: 'default'
};

export function ApartamentosPage() {
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [status, setStatus] = useState<StatusApartamento | ''>('');

  const filter = useMemo<ApartamentoFilter>(
    () => ({
      page: page + 1,
      pageSize,
      ...(status ? { status } : {})
    }),
    [page, pageSize, status]
  );

  const { data, isLoading, isError } = useQuery({
    queryKey: ['apartamentos', filter],
    queryFn: () => apartamentosService.list(filter)
  });

  return (
    <Stack spacing={2}>
      <Typography variant="h4" fontWeight={700}>
        Apartamentos
      </Typography>

      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            select
            label="Status"
            value={status}
            sx={{ minWidth: 200 }}
            onChange={(e) => {
              setStatus(e.target.value as StatusApartamento | '');
              setPage(0);
            }}
          >
            {statusOptions.map((o) => (
              <MenuItem key={o.value || 'all'} value={o.value}>
                {o.label}
              </MenuItem>
            ))}
          </TextField>
        </Stack>
      </Paper>

      {isError ? <Alert severity="error">Erro ao carregar apartamentos.</Alert> : null}

      <Paper>
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
                  <TableCell>Torre</TableCell>
                  <TableCell>Tipologia</TableCell>
                  <TableCell>Pav</TableCell>
                  <TableCell align="right">Preço</TableCell>
                  <TableCell>Status</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {(data?.items ?? []).map((apt) => (
                  <TableRow key={apt.id} hover>
                    <TableCell>{apt.numero}</TableCell>
                    <TableCell>{apt.torreNome ?? '-'}</TableCell>
                    <TableCell>{apt.tipologiaNome ?? '-'}</TableCell>
                    <TableCell>{apt.pavimento}</TableCell>
                    <TableCell align="right">
                      {apt.precoAtual.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                    </TableCell>
                    <TableCell>
                      <Chip size="small" color={statusColors[apt.status]} label={apt.status} />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
        <TablePagination
          component="div"
          count={data?.total ?? 0}
          page={page}
          onPageChange={(_, p) => setPage(p)}
          rowsPerPage={pageSize}
          onRowsPerPageChange={(e) => {
            setPageSize(Number(e.target.value));
            setPage(0);
          }}
          rowsPerPageOptions={[10, 20, 50, 100]}
          labelRowsPerPage="Linhas por página"
        />
      </Paper>
    </Stack>
  );
}
