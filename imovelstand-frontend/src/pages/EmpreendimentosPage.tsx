import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  MenuItem,
  Paper,
  Stack,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  TextField,
  Typography
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  empreendimentosService,
  type EmpreendimentoCreateRequest
} from '@/services/empreendimentosService';
import { torresService, type TorreCreateRequest } from '@/services/torresService';
import { tipologiasService, type TipologiaCreateRequest } from '@/services/tipologiasService';
import { useAuthStore } from '@/stores/authStore';
import type { EmpreendimentoResponse, TorreResponse, TipologiaResponse } from '@/types/api';

const STATUS_EMP = ['PreLancamento', 'Lancamento', 'EmObra', 'Entregue', 'Encerrado'];

const empSchema = z.object({
  nome: z.string().min(2, 'Obrigatório'),
  slug: z.string().min(2, 'Obrigatório').regex(/^[a-z0-9-]+$/, 'Use letras minúsculas, números e hífen'),
  descricao: z.string().optional(),
  construtora: z.string().optional(),
  status: z.string(),
  vgvEstimado: z.coerce.number().optional(),
  dataLancamento: z.string().optional(),
  dataEntregaPrevista: z.string().optional(),
  logradouro: z.string().min(2),
  numero: z.string().min(1),
  complemento: z.string().optional(),
  bairro: z.string().min(1),
  cidade: z.string().min(1),
  uf: z.string().length(2, 'UF com 2 letras'),
  cep: z.string().min(8)
});

type EmpForm = z.infer<typeof empSchema>;

const torreSchema = z.object({
  nome: z.string().min(1, 'Obrigatório'),
  pavimentos: z.coerce.number().int().min(1),
  apartamentosPorPavimento: z.coerce.number().int().min(1)
});

type TorreForm = z.infer<typeof torreSchema>;

const tipSchema = z.object({
  nome: z.string().min(1),
  areaPrivativa: z.coerce.number().positive(),
  areaTotal: z.coerce.number().positive(),
  quartos: z.coerce.number().int().min(0),
  suites: z.coerce.number().int().min(0),
  banheiros: z.coerce.number().int().min(0),
  vagas: z.coerce.number().int().min(0),
  precoBase: z.coerce.number().positive(),
  plantaUrl: z.string().optional()
});

type TipForm = z.infer<typeof tipSchema>;

export function EmpreendimentosPage() {
  const role = useAuthStore((s) => s.user?.role);
  const canEdit = role === 'Admin' || role === 'Gerente';
  const canDelete = role === 'Admin';
  const queryClient = useQueryClient();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [tab, setTab] = useState(0);

  const empsQuery = useQuery({
    queryKey: ['empreendimentos'],
    queryFn: () => empreendimentosService.list()
  });

  const selecionado = useMemo(
    () => empsQuery.data?.find((e) => e.id === selectedId) ?? null,
    [empsQuery.data, selectedId]
  );

  const empForm = useForm<EmpForm>({ resolver: zodResolver(empSchema) });

  const createEmp = useMutation({
    mutationFn: (d: EmpForm) => {
      const payload: EmpreendimentoCreateRequest = {
        nome: d.nome,
        slug: d.slug,
        descricao: d.descricao,
        construtora: d.construtora,
        status: d.status,
        vgvEstimado: d.vgvEstimado,
        dataLancamento: d.dataLancamento || undefined,
        dataEntregaPrevista: d.dataEntregaPrevista || undefined,
        endereco: {
          logradouro: d.logradouro,
          numero: d.numero,
          complemento: d.complemento,
          bairro: d.bairro,
          cidade: d.cidade,
          uf: d.uf.toUpperCase(),
          cep: d.cep
        }
      };
      return empreendimentosService.create(payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['empreendimentos'] });
      setDialogOpen(false);
    }
  });

  const removeEmp = useMutation({
    mutationFn: (id: number) => empreendimentosService.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['empreendimentos'] });
      setSelectedId(null);
    }
  });

  return (
    <Stack spacing={3}>
      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
          <Typography variant="h6" fontWeight={700}>Empreendimentos</Typography>
          {canEdit ? (
            <Button
              startIcon={<AddIcon />}
              onClick={() => {
                empForm.reset({ status: 'PreLancamento', uf: 'SP' });
                setDialogOpen(true);
              }}
            >
              Novo empreendimento
            </Button>
          ) : null}
        </Stack>
      </Paper>

      {empsQuery.isError ? <Alert severity="error">Erro ao carregar empreendimentos.</Alert> : null}

      {empsQuery.isLoading ? (
        <Box sx={{ p: 4, display: 'grid', placeItems: 'center' }}>
          <CircularProgress />
        </Box>
      ) : (
        <Paper sx={{ width: '100%' }}>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Nome</TableCell>
                  <TableCell>Slug</TableCell>
                  <TableCell>Construtora</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">VGV estimado</TableCell>
                  <TableCell align="right">Ações</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {(empsQuery.data ?? []).map((e) => (
                  <TableRow
                    key={e.id}
                    hover
                    selected={selectedId === e.id}
                    onClick={() => { setSelectedId(e.id); setTab(0); }}
                    sx={{ cursor: 'pointer' }}
                  >
                    <TableCell>{e.nome}</TableCell>
                    <TableCell>{e.slug}</TableCell>
                    <TableCell>{e.construtora ?? '—'}</TableCell>
                    <TableCell><Chip size="small" label={e.status} /></TableCell>
                    <TableCell align="right">
                      {e.vgvEstimado ? e.vgvEstimado.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) : '—'}
                    </TableCell>
                    <TableCell align="right">
                      {canDelete ? (
                        <IconButton
                          size="small"
                          color="error"
                          onClick={(ev) => { ev.stopPropagation(); if (confirm(`Excluir ${e.nome}?`)) removeEmp.mutate(e.id); }}
                        >
                          <DeleteOutlineIcon fontSize="small" />
                        </IconButton>
                      ) : null}
                    </TableCell>
                  </TableRow>
                ))}
                {(empsQuery.data ?? []).length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6}>
                      <Typography color="text.secondary" textAlign="center" py={3}>
                        Nenhum empreendimento cadastrado.
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : null}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}

      {selecionado ? (
        <Paper sx={{ p: 2 }}>
          <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 1 }}>
            <Typography variant="subtitle1" fontWeight={700}>
              {selecionado.nome}
            </Typography>
            <Button size="small" variant="text" onClick={() => setSelectedId(null)}>Fechar</Button>
          </Stack>
          <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
            <Tab label="Torres" />
            <Tab label="Tipologias" />
          </Tabs>
          {tab === 0 ? <TorresTab empreendimentoId={selecionado.id} canEdit={canEdit} canDelete={canDelete} /> : null}
          {tab === 1 ? <TipologiasTab empreendimentoId={selecionado.id} canEdit={canEdit} canDelete={canDelete} /> : null}
        </Paper>
      ) : null}

      {/* Dialog novo empreendimento */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="md" fullWidth>
        <form onSubmit={empForm.handleSubmit((d) => createEmp.mutate(d))}>
          <DialogTitle>Novo empreendimento</DialogTitle>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 1 }}>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField label="Nome" fullWidth {...empForm.register('nome')} error={!!empForm.formState.errors.nome} helperText={empForm.formState.errors.nome?.message} />
                <TextField label="Slug" fullWidth placeholder="residencial-vista-verde" {...empForm.register('slug')} error={!!empForm.formState.errors.slug} helperText={empForm.formState.errors.slug?.message} />
              </Stack>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField label="Construtora" fullWidth {...empForm.register('construtora')} />
                <TextField select label="Status" fullWidth defaultValue="PreLancamento" {...empForm.register('status')}>
                  {STATUS_EMP.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
                </TextField>
                <TextField label="VGV estimado" type="number" fullWidth inputProps={{ step: '0.01' }} {...empForm.register('vgvEstimado')} />
              </Stack>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField label="Data lançamento" type="date" fullWidth InputLabelProps={{ shrink: true }} {...empForm.register('dataLancamento')} />
                <TextField label="Data entrega prevista" type="date" fullWidth InputLabelProps={{ shrink: true }} {...empForm.register('dataEntregaPrevista')} />
              </Stack>
              <TextField label="Descrição" multiline minRows={2} fullWidth {...empForm.register('descricao')} />

              <Typography variant="caption" color="text.secondary">Endereço</Typography>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField label="Logradouro" fullWidth {...empForm.register('logradouro')} error={!!empForm.formState.errors.logradouro} />
                <TextField label="Número" sx={{ maxWidth: 120 }} {...empForm.register('numero')} error={!!empForm.formState.errors.numero} />
                <TextField label="Complemento" fullWidth {...empForm.register('complemento')} />
              </Stack>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField label="Bairro" fullWidth {...empForm.register('bairro')} error={!!empForm.formState.errors.bairro} />
                <TextField label="Cidade" fullWidth {...empForm.register('cidade')} error={!!empForm.formState.errors.cidade} />
                <TextField label="UF" sx={{ maxWidth: 80 }} {...empForm.register('uf')} error={!!empForm.formState.errors.uf} />
                <TextField label="CEP" sx={{ maxWidth: 140 }} {...empForm.register('cep')} error={!!empForm.formState.errors.cep} />
              </Stack>

              {createEmp.isError ? <Alert severity="error">Erro ao criar. Slug já existe?</Alert> : null}
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDialogOpen(false)} variant="text">Cancelar</Button>
            <Button type="submit" disabled={createEmp.isPending}>
              {createEmp.isPending ? 'Salvando...' : 'Criar'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Stack>
  );
}

function TorresTab({ empreendimentoId, canEdit, canDelete }: { empreendimentoId: number; canEdit: boolean; canDelete: boolean }) {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);

  const q = useQuery({
    queryKey: ['torres', empreendimentoId],
    queryFn: () => torresService.list(empreendimentoId)
  });

  const form = useForm<TorreForm>({ resolver: zodResolver(torreSchema) });

  const create = useMutation({
    mutationFn: (d: TorreForm) => {
      const payload: TorreCreateRequest = { empreendimentoId, ...d };
      return torresService.create(payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['torres', empreendimentoId] });
      setOpen(false);
    }
  });

  const remove = useMutation({
    mutationFn: (id: number) => torresService.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['torres', empreendimentoId] })
  });

  return (
    <Stack spacing={2}>
      {canEdit ? (
        <Stack direction="row" justifyContent="flex-end">
          <Button size="small" startIcon={<AddIcon />} onClick={() => { form.reset(); setOpen(true); }}>
            Nova torre
          </Button>
        </Stack>
      ) : null}

      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Nome</TableCell>
              <TableCell align="right">Pavimentos</TableCell>
              <TableCell align="right">Aptos/pav</TableCell>
              <TableCell align="right">Total aptos</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {(q.data ?? []).map((t: TorreResponse) => (
              <TableRow key={t.id} hover>
                <TableCell>{t.nome}</TableCell>
                <TableCell align="right">{t.pavimentos}</TableCell>
                <TableCell align="right">{t.apartamentosPorPavimento}</TableCell>
                <TableCell align="right">{t.qtdApartamentos}</TableCell>
                <TableCell align="right">
                  {canDelete ? (
                    <IconButton size="small" color="error" onClick={() => { if (confirm(`Excluir torre ${t.nome}?`)) remove.mutate(t.id); }}>
                      <DeleteOutlineIcon fontSize="small" />
                    </IconButton>
                  ) : null}
                </TableCell>
              </TableRow>
            ))}
            {(q.data ?? []).length === 0 ? (
              <TableRow>
                <TableCell colSpan={5}>
                  <Typography variant="caption" color="text.secondary">Nenhuma torre cadastrada.</Typography>
                </TableCell>
              </TableRow>
            ) : null}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="xs" fullWidth>
        <form onSubmit={form.handleSubmit((d) => create.mutate(d))}>
          <DialogTitle>Nova torre</DialogTitle>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 1 }}>
              <TextField label="Nome" fullWidth {...form.register('nome')} error={!!form.formState.errors.nome} helperText={form.formState.errors.nome?.message} />
              <TextField label="Pavimentos" type="number" fullWidth {...form.register('pavimentos')} error={!!form.formState.errors.pavimentos} />
              <TextField label="Apartamentos por pavimento" type="number" fullWidth {...form.register('apartamentosPorPavimento')} error={!!form.formState.errors.apartamentosPorPavimento} />
              {create.isError ? <Alert severity="error">Erro ao criar torre.</Alert> : null}
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button variant="text" onClick={() => setOpen(false)}>Cancelar</Button>
            <Button type="submit" disabled={create.isPending}>{create.isPending ? 'Salvando...' : 'Criar'}</Button>
          </DialogActions>
        </form>
      </Dialog>
    </Stack>
  );
}

function TipologiasTab({ empreendimentoId, canEdit, canDelete }: { empreendimentoId: number; canEdit: boolean; canDelete: boolean }) {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);

  const q = useQuery({
    queryKey: ['tipologias', empreendimentoId],
    queryFn: () => tipologiasService.list(empreendimentoId)
  });

  const form = useForm<TipForm>({ resolver: zodResolver(tipSchema) });

  const create = useMutation({
    mutationFn: (d: TipForm) => {
      const payload: TipologiaCreateRequest = { empreendimentoId, ...d };
      return tipologiasService.create(payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tipologias', empreendimentoId] });
      setOpen(false);
    }
  });

  const remove = useMutation({
    mutationFn: (id: number) => tipologiasService.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tipologias', empreendimentoId] })
  });

  return (
    <Stack spacing={2}>
      {canEdit ? (
        <Stack direction="row" justifyContent="flex-end">
          <Button size="small" startIcon={<AddIcon />} onClick={() => { form.reset(); setOpen(true); }}>
            Nova tipologia
          </Button>
        </Stack>
      ) : null}

      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Nome</TableCell>
              <TableCell align="right">Priv. (m²)</TableCell>
              <TableCell align="right">Total (m²)</TableCell>
              <TableCell align="right">Q</TableCell>
              <TableCell align="right">S</TableCell>
              <TableCell align="right">B</TableCell>
              <TableCell align="right">V</TableCell>
              <TableCell align="right">Preço base</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {(q.data ?? []).map((t: TipologiaResponse) => (
              <TableRow key={t.id} hover>
                <TableCell>{t.nome}</TableCell>
                <TableCell align="right">{t.areaPrivativa}</TableCell>
                <TableCell align="right">{t.areaTotal}</TableCell>
                <TableCell align="right">{t.quartos}</TableCell>
                <TableCell align="right">{t.suites}</TableCell>
                <TableCell align="right">{t.banheiros}</TableCell>
                <TableCell align="right">{t.vagas}</TableCell>
                <TableCell align="right">{t.precoBase.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</TableCell>
                <TableCell align="right">
                  {canDelete ? (
                    <IconButton size="small" color="error" onClick={() => { if (confirm(`Excluir tipologia ${t.nome}?`)) remove.mutate(t.id); }}>
                      <DeleteOutlineIcon fontSize="small" />
                    </IconButton>
                  ) : null}
                </TableCell>
              </TableRow>
            ))}
            {(q.data ?? []).length === 0 ? (
              <TableRow>
                <TableCell colSpan={9}>
                  <Typography variant="caption" color="text.secondary">Nenhuma tipologia cadastrada.</Typography>
                </TableCell>
              </TableRow>
            ) : null}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <form onSubmit={form.handleSubmit((d) => create.mutate(d))}>
          <DialogTitle>Nova tipologia</DialogTitle>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 1 }}>
              <TextField label="Nome" fullWidth {...form.register('nome')} error={!!form.formState.errors.nome} />
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField label="Área privativa (m²)" type="number" fullWidth inputProps={{ step: '0.01' }} {...form.register('areaPrivativa')} />
                <TextField label="Área total (m²)" type="number" fullWidth inputProps={{ step: '0.01' }} {...form.register('areaTotal')} />
              </Stack>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField label="Quartos" type="number" fullWidth {...form.register('quartos')} />
                <TextField label="Suítes" type="number" fullWidth {...form.register('suites')} />
                <TextField label="Banheiros" type="number" fullWidth {...form.register('banheiros')} />
                <TextField label="Vagas" type="number" fullWidth {...form.register('vagas')} />
              </Stack>
              <TextField label="Preço base" type="number" fullWidth inputProps={{ step: '0.01' }} {...form.register('precoBase')} />
              <TextField label="URL da planta (opcional)" fullWidth {...form.register('plantaUrl')} />
              {create.isError ? <Alert severity="error">Erro ao criar tipologia.</Alert> : null}
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button variant="text" onClick={() => setOpen(false)}>Cancelar</Button>
            <Button type="submit" disabled={create.isPending}>{create.isPending ? 'Salvando...' : 'Criar'}</Button>
          </DialogActions>
        </form>
      </Dialog>
    </Stack>
  );
}
