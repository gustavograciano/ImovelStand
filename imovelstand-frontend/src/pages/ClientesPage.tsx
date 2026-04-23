import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link as RouterLink } from 'react-router-dom';
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
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { clientesService } from '@/services/clientesService';
import type { ClienteResponse, OrigemLead, StatusFunil } from '@/types/api';

const FUNIL_STAGES: StatusFunil[] = ['Lead', 'Contato', 'Visita', 'Proposta', 'Negociacao', 'Venda'];

const FUNIL_COLORS: Record<StatusFunil, string> = {
  Lead: '#94a3b8',
  Contato: '#60a5fa',
  Visita: '#818cf8',
  Proposta: '#a855f7',
  Negociacao: '#f59e0b',
  Venda: '#22c55e',
  Descarte: '#ef4444'
};

const ORIGENS: OrigemLead[] = ['Indicacao', 'Facebook', 'Instagram', 'Google', 'Plantao', 'Site', 'WhatsApp', 'Evento', 'Outros'];

const cpfRegex = /^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$/;

const schema = z.object({
  nome: z.string().min(3, 'Nome curto'),
  cpf: z.string().regex(cpfRegex, 'CPF no formato 000.000.000-00'),
  email: z.string().email('Email inválido'),
  telefone: z.string().min(8, 'Telefone inválido'),
  whatsapp: z.string().optional(),
  origemLead: z.string().optional(),
  consentimentoLgpd: z.boolean()
});

type FormData = z.infer<typeof schema>;

export function ClientesPage() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [filtroOrigem, setFiltroOrigem] = useState<OrigemLead | ''>('');
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useQuery({
    queryKey: ['clientes', filtroOrigem],
    queryFn: () =>
      clientesService.list({
        pageSize: 200,
        ...(filtroOrigem ? { origemLead: filtroOrigem } : {})
      })
  });

  const createMutation = useMutation({
    mutationFn: (data: FormData) =>
      clientesService.create({
        ...data,
        origemLead: (data.origemLead as OrigemLead) || undefined,
        consentimentoLgpd: data.consentimentoLgpd
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clientes'] });
      setDialogOpen(false);
    }
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting }
  } = useForm<FormData>({ resolver: zodResolver(schema), defaultValues: { consentimentoLgpd: true } });

  const agrupados = useMemo(() => {
    const por: Record<StatusFunil, ClienteResponse[]> = {
      Lead: [],
      Contato: [],
      Visita: [],
      Proposta: [],
      Negociacao: [],
      Venda: [],
      Descarte: []
    };
    for (const c of data?.items ?? []) {
      (por[c.statusFunil] ??= []).push(c);
    }
    return por;
  }, [data]);

  return (
    <Stack spacing={3}>
      <Paper sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
          <TextField
            select
            size="small"
            label="Origem"
            value={filtroOrigem}
            onChange={(e) => setFiltroOrigem(e.target.value as OrigemLead | '')}
            sx={{ minWidth: 220 }}
          >
            <MenuItem value="">Todas origens</MenuItem>
            {ORIGENS.map((o) => (
              <MenuItem key={o} value={o}>{o}</MenuItem>
            ))}
          </TextField>
          <Button
            startIcon={<AddIcon />}
            onClick={() => {
              reset({ nome: '', cpf: '', email: '', telefone: '', consentimentoLgpd: true });
              setDialogOpen(true);
            }}
          >
            Novo cliente
          </Button>
        </Stack>
      </Paper>

      {error ? <Alert severity="error">Erro ao carregar clientes.</Alert> : null}

      {isLoading ? (
        <Box sx={{ p: 4, display: 'grid', placeItems: 'center' }}>
          <CircularProgress />
        </Box>
      ) : (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: `repeat(${FUNIL_STAGES.length}, minmax(220px, 1fr))`,
            gap: 2,
            overflowX: 'auto',
            pb: 1
          }}
        >
          {FUNIL_STAGES.map((stage) => (
            <Paper
              key={stage}
              sx={{
                p: 1.5,
                bgcolor: (t) => (t.palette.mode === 'dark' ? 'rgba(255,255,255,0.02)' : 'grey.50'),
                minHeight: 360
              }}
              elevation={0}
            >
              <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
                <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: FUNIL_COLORS[stage] }} />
                <Typography variant="subtitle2" fontWeight={700}>
                  {stage}
                </Typography>
                <Chip size="small" label={agrupados[stage].length} />
              </Stack>
              <Stack spacing={1}>
                {agrupados[stage].map((cliente) => (
                  <Paper
                    key={cliente.id}
                    component={RouterLink}
                    to={`/clientes/${cliente.id}`}
                    sx={{
                      p: 1.5,
                      textDecoration: 'none',
                      color: 'inherit',
                      display: 'block',
                      transition: 'box-shadow 0.15s',
                      '&:hover': { boxShadow: 3 }
                    }}
                    elevation={1}
                  >
                    <Typography variant="body2" fontWeight={600}>{cliente.nome}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {cliente.telefone} · {cliente.origemLead ?? '—'}
                    </Typography>
                  </Paper>
                ))}
                {agrupados[stage].length === 0 ? (
                  <Typography variant="caption" color="text.secondary">Sem clientes.</Typography>
                ) : null}
              </Stack>
            </Paper>
          ))}
        </Box>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleSubmit((d) => createMutation.mutate(d))}>
          <DialogTitle>Novo cliente</DialogTitle>
          <DialogContent>
            <Stack spacing={2} sx={{ mt: 1 }}>
              <TextField
                label="Nome"
                fullWidth
                {...register('nome')}
                error={!!errors.nome}
                helperText={errors.nome?.message}
              />
              <TextField
                label="CPF"
                placeholder="000.000.000-00"
                fullWidth
                {...register('cpf')}
                error={!!errors.cpf}
                helperText={errors.cpf?.message}
              />
              <TextField
                label="Email"
                type="email"
                fullWidth
                {...register('email')}
                error={!!errors.email}
                helperText={errors.email?.message}
              />
              <TextField
                label="Telefone"
                fullWidth
                {...register('telefone')}
                error={!!errors.telefone}
                helperText={errors.telefone?.message}
              />
              <TextField
                label="WhatsApp"
                fullWidth
                {...register('whatsapp')}
              />
              <TextField
                select
                label="Origem do lead"
                fullWidth
                defaultValue=""
                {...register('origemLead')}
              >
                <MenuItem value="">-</MenuItem>
                {ORIGENS.map((o) => (
                  <MenuItem key={o} value={o}>{o}</MenuItem>
                ))}
              </TextField>
              {createMutation.isError ? (
                <Alert severity="error">Não foi possível cadastrar. Verifique se o CPF/email já não existe.</Alert>
              ) : null}
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDialogOpen(false)} variant="text">Cancelar</Button>
            <Button type="submit" disabled={isSubmitting || createMutation.isPending}>
              {createMutation.isPending ? 'Salvando...' : 'Criar'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Stack>
  );
}
