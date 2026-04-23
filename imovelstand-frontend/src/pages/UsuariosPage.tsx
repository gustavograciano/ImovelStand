import { useState } from 'react';
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
import BlockIcon from '@mui/icons-material/Block';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { usuariosService, type UsuarioCreateRequest } from '@/services/usuariosService';
import { useAuthStore } from '@/stores/authStore';

const ROLES = ['Admin', 'Gerente', 'Corretor'];

const schema = z.object({
  nome: z.string().min(2),
  email: z.string().email(),
  senha: z.string().min(8, 'Mínimo 8 caracteres'),
  role: z.string(),
  creci: z.string().optional(),
  percentualComissao: z.string().optional()
});

type FormData = z.infer<typeof schema>;

export function UsuariosPage() {
  const role = useAuthStore((s) => s.user?.role);
  const isAdmin = role === 'Admin';
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);

  const q = useQuery({
    queryKey: ['usuarios'],
    queryFn: () => usuariosService.list()
  });

  const form = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { role: 'Corretor' }
  });

  const create = useMutation({
    mutationFn: (d: FormData) => {
      const payload: UsuarioCreateRequest = {
        nome: d.nome,
        email: d.email,
        senha: d.senha,
        role: d.role,
        creci: d.creci,
        percentualComissao: d.percentualComissao ? Number(d.percentualComissao) : undefined
      };
      return usuariosService.create(payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] });
      setOpen(false);
    }
  });

  const inativar = useMutation({
    mutationFn: (id: number) => usuariosService.inativar(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['usuarios'] })
  });

  if (!isAdmin) {
    return <Alert severity="warning">Somente administradores podem acessar esta área.</Alert>;
  }

  return (
    <Stack spacing={3}>
      <Paper sx={{ p: 2 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h6" fontWeight={700}>Usuários</Typography>
          <Button startIcon={<AddIcon />} onClick={() => { form.reset({ role: 'Corretor' }); setOpen(true); }}>
            Novo usuário
          </Button>
        </Stack>
      </Paper>

      {q.isError ? <Alert severity="error">Erro ao carregar usuários.</Alert> : null}

      {q.isLoading ? (
        <Box sx={{ p: 4, display: 'grid', placeItems: 'center' }}><CircularProgress /></Box>
      ) : (
        <Paper>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Nome</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Role</TableCell>
                  <TableCell>CRECI</TableCell>
                  <TableCell align="right">Comissão %</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Último login</TableCell>
                  <TableCell align="right">Ações</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {(q.data ?? []).map((u) => (
                  <TableRow key={u.id} hover>
                    <TableCell>{u.nome}</TableCell>
                    <TableCell>{u.email}</TableCell>
                    <TableCell><Chip size="small" label={u.role} /></TableCell>
                    <TableCell>{u.creci ?? '—'}</TableCell>
                    <TableCell align="right">{u.percentualComissao ?? '—'}</TableCell>
                    <TableCell>
                      <Chip size="small" label={u.ativo ? 'Ativo' : 'Inativo'} color={u.ativo ? 'success' : 'default'} />
                    </TableCell>
                    <TableCell>{u.ultimoLoginEm ? new Date(u.ultimoLoginEm).toLocaleDateString('pt-BR') : '—'}</TableCell>
                    <TableCell align="right">
                      {u.ativo ? (
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => { if (confirm(`Inativar ${u.nome}?`)) inativar.mutate(u.id); }}
                        >
                          <BlockIcon fontSize="small" />
                        </IconButton>
                      ) : null}
                    </TableCell>
                  </TableRow>
                ))}
                {(q.data ?? []).length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8}>
                      <Typography color="text.secondary" textAlign="center" py={3}>
                        Nenhum usuário cadastrado.
                      </Typography>
                    </TableCell>
                  </TableRow>
                ) : null}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <form onSubmit={form.handleSubmit((d) => create.mutate(d))}>
          <DialogTitle>Novo usuário</DialogTitle>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 1 }}>
              <TextField label="Nome" fullWidth {...form.register('nome')} error={!!form.formState.errors.nome} helperText={form.formState.errors.nome?.message} />
              <TextField label="Email" type="email" fullWidth {...form.register('email')} error={!!form.formState.errors.email} helperText={form.formState.errors.email?.message} />
              <TextField label="Senha" type="password" fullWidth {...form.register('senha')} error={!!form.formState.errors.senha} helperText={form.formState.errors.senha?.message ?? 'Mínimo 8 caracteres'} />
              <TextField select label="Role" fullWidth defaultValue="Corretor" {...form.register('role')}>
                {ROLES.map((r) => <MenuItem key={r} value={r}>{r}</MenuItem>)}
              </TextField>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField label="CRECI" fullWidth {...form.register('creci')} />
                <TextField label="Comissão %" type="number" fullWidth inputProps={{ step: '0.01' }} {...form.register('percentualComissao')} />
              </Stack>
              {create.isError ? <Alert severity="error">Erro ao criar. Email já existe?</Alert> : null}
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
