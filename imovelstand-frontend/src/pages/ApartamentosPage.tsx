import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
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
  TablePagination,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import ViewListIcon from '@mui/icons-material/ViewList';
import ViewModuleIcon from '@mui/icons-material/ViewModule';
import { apartamentosService, type ApartamentoFilter } from '@/services/apartamentosService';
import { MapaEmpreendimento } from '@/components/MapaEmpreendimento';
import { NovoApartamentoDialog } from '@/components/NovoApartamentoDialog';
import { useAuthStore } from '@/stores/authStore';
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

type ViewMode = 'list' | 'map';

export function ApartamentosPage() {
  const role = useAuthStore((s) => s.user?.role);
  const canCreate = role === 'Admin' || role === 'Gerente';
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [status, setStatus] = useState<StatusApartamento | ''>('');
  const [novoOpen, setNovoOpen] = useState(false);

  const listFilter = useMemo<ApartamentoFilter>(
    () => ({ page: page + 1, pageSize, ...(status ? { status } : {}) }),
    [page, pageSize, status]
  );
  const mapFilter = useMemo<ApartamentoFilter>(
    () => ({ page: 1, pageSize: 500, ...(status ? { status } : {}) }),
    [status]
  );

  const listQuery = useQuery({
    queryKey: ['apartamentos', 'list', listFilter],
    queryFn: () => apartamentosService.list(listFilter),
    enabled: viewMode === 'list'
  });

  const mapQuery = useQuery({
    queryKey: ['apartamentos', 'map', mapFilter],
    queryFn: () => apartamentosService.list(mapFilter),
    enabled: viewMode === 'map'
  });

  const activeQuery = viewMode === 'list' ? listQuery : mapQuery;

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={2}>
        {canCreate ? (
          <Button startIcon={<AddIcon />} onClick={() => setNovoOpen(true)}>
            Novo apartamento
          </Button>
        ) : <Box />}
        <ToggleButtonGroup
          value={viewMode}
          exclusive
          onChange={(_, v) => v && setViewMode(v)}
          size="small"
        >
          <ToggleButton value="list"><ViewListIcon fontSize="small" sx={{ mr: 0.5 }} />Lista</ToggleButton>
          <ToggleButton value="map"><ViewModuleIcon fontSize="small" sx={{ mr: 0.5 }} />Espelho</ToggleButton>
        </ToggleButtonGroup>
      </Stack>

      <NovoApartamentoDialog open={novoOpen} onClose={() => setNovoOpen(false)} />

      <Paper sx={{ p: 2 }}>
        <TextField
          select
          label="Status"
          value={status}
          sx={{ minWidth: 200 }}
          size="small"
          onChange={(e) => {
            setStatus(e.target.value as StatusApartamento | '');
            setPage(0);
          }}
        >
          {statusOptions.map((o) => (
            <MenuItem key={o.value || 'all'} value={o.value}>{o.label}</MenuItem>
          ))}
        </TextField>
      </Paper>

      {activeQuery.isError ? <Alert severity="error">Erro ao carregar apartamentos.</Alert> : null}

      {activeQuery.isLoading ? (
        <Box sx={{ p: 4, display: 'grid', placeItems: 'center' }}>
          <CircularProgress />
        </Box>
      ) : viewMode === 'map' ? (
        <MapaEmpreendimento apartamentos={mapQuery.data?.items ?? []} />
      ) : (
        <Paper sx={{ width: "100%" }}>
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
                {(listQuery.data?.items ?? []).map((apt) => (
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
          <TablePagination
            component="div"
            count={listQuery.data?.total ?? 0}
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
      )}
    </Stack>
  );
}
