import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Stack,
  TextField
} from '@mui/material';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { apartamentosService, type ApartamentoCreateRequest } from '@/services/apartamentosService';
import { empreendimentosService } from '@/services/empreendimentosService';
import { torresService } from '@/services/torresService';
import { tipologiasService } from '@/services/tipologiasService';

const ORIENTACOES = ['Norte', 'Sul', 'Leste', 'Oeste', 'Nordeste', 'Noroeste', 'Sudeste', 'Sudoeste'];

const schema = z.object({
  torreId: z.coerce.number().int().positive('Selecione torre'),
  tipologiaId: z.coerce.number().int().positive('Selecione tipologia'),
  numero: z.string().min(1),
  pavimento: z.coerce.number().int().min(0),
  orientacao: z.string().optional(),
  precoAtual: z.coerce.number().positive(),
  observacoes: z.string().optional()
});

type FormData = z.infer<typeof schema>;

interface Props {
  open: boolean;
  onClose: () => void;
}

export function NovoApartamentoDialog({ open, onClose }: Props) {
  const queryClient = useQueryClient();
  const [empreendimentoId, setEmpreendimentoId] = useState<number | ''>('');

  const empsQuery = useQuery({
    queryKey: ['empreendimentos', 'select'],
    queryFn: () => empreendimentosService.list(),
    enabled: open
  });

  const torresQuery = useQuery({
    queryKey: ['torres', empreendimentoId],
    queryFn: () => torresService.list(Number(empreendimentoId)),
    enabled: open && !!empreendimentoId
  });

  const tipologiasQuery = useQuery({
    queryKey: ['tipologias', empreendimentoId],
    queryFn: () => tipologiasService.list(Number(empreendimentoId)),
    enabled: open && !!empreendimentoId
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  useEffect(() => {
    if (open) {
      reset({
        torreId: 0,
        tipologiaId: 0,
        numero: '',
        pavimento: 1,
        orientacao: '',
        precoAtual: 0,
        observacoes: ''
      });
      setEmpreendimentoId('');
    }
  }, [open, reset]);

  const create = useMutation({
    mutationFn: (d: FormData) => {
      const payload: ApartamentoCreateRequest = {
        torreId: d.torreId,
        tipologiaId: d.tipologiaId,
        numero: d.numero,
        pavimento: d.pavimento,
        orientacao: d.orientacao || undefined,
        precoAtual: d.precoAtual,
        observacoes: d.observacoes
      };
      return apartamentosService.create(payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['apartamentos'] });
      onClose();
    }
  });

  const torres = useMemo(() => torresQuery.data ?? [], [torresQuery.data]);
  const tipologias = useMemo(() => tipologiasQuery.data ?? [], [tipologiasQuery.data]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit((d) => create.mutate(d))}>
        <DialogTitle>Novo apartamento</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              select
              label="Empreendimento"
              fullWidth
              value={empreendimentoId}
              onChange={(e) => setEmpreendimentoId(e.target.value ? Number(e.target.value) : '')}
            >
              <MenuItem value="">Selecione...</MenuItem>
              {(empsQuery.data ?? []).map((e) => (
                <MenuItem key={e.id} value={e.id}>{e.nome}</MenuItem>
              ))}
            </TextField>

            <TextField
              select
              label="Torre"
              fullWidth
              defaultValue=""
              disabled={!empreendimentoId}
              {...register('torreId')}
              error={!!errors.torreId}
              helperText={errors.torreId?.message}
            >
              <MenuItem value="">Selecione...</MenuItem>
              {torres.map((t) => (
                <MenuItem key={t.id} value={t.id}>{t.nome}</MenuItem>
              ))}
            </TextField>

            <TextField
              select
              label="Tipologia"
              fullWidth
              defaultValue=""
              disabled={!empreendimentoId}
              {...register('tipologiaId')}
              error={!!errors.tipologiaId}
              helperText={errors.tipologiaId?.message}
            >
              <MenuItem value="">Selecione...</MenuItem>
              {tipologias.map((t) => (
                <MenuItem key={t.id} value={t.id}>
                  {t.nome} — {t.areaPrivativa}m² — {t.precoBase.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                </MenuItem>
              ))}
            </TextField>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Número" fullWidth {...register('numero')} error={!!errors.numero} helperText={errors.numero?.message} />
              <TextField label="Pavimento" type="number" fullWidth {...register('pavimento')} error={!!errors.pavimento} />
              <TextField select label="Orientação" fullWidth defaultValue="" {...register('orientacao')}>
                <MenuItem value="">-</MenuItem>
                {ORIENTACOES.map((o) => (
                  <MenuItem key={o} value={o}>{o}</MenuItem>
                ))}
              </TextField>
            </Stack>

            <TextField label="Preço atual" type="number" fullWidth inputProps={{ step: '0.01' }} {...register('precoAtual')} error={!!errors.precoAtual} helperText={errors.precoAtual?.message} />

            <TextField label="Observações" multiline minRows={2} fullWidth {...register('observacoes')} />

            {create.isError ? <Alert severity="error">Erro ao criar apartamento.</Alert> : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button variant="text" onClick={onClose}>Cancelar</Button>
          <Button type="submit" disabled={create.isPending}>
            {create.isPending ? 'Salvando...' : 'Criar'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
