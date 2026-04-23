import { useEffect, useMemo } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  MenuItem,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { vendasService } from '@/services/vendasService';
import { propostasService } from '@/services/propostasService';
import { clientesService } from '@/services/clientesService';
import { apartamentosService } from '@/services/apartamentosService';
import { usuariosService } from '@/services/usuariosService';
import type { IndiceReajuste } from '@/types/api';

const INDICES: IndiceReajuste[] = ['SemReajuste', 'Incc', 'Ipca', 'Igpm', 'Tr', 'Selic'];

const schema = z.object({
  propostaId: z.string().optional(),
  clienteId: z.coerce.number().int().positive('Selecione cliente'),
  apartamentoId: z.coerce.number().int().positive('Selecione apartamento'),
  corretorId: z.coerce.number().int().positive('Selecione corretor'),
  corretorCaptacaoId: z.string().optional(),
  valorFinal: z.coerce.number().positive('Valor final obrigatório'),
  observacoes: z.string().optional(),
  valorTotal: z.coerce.number().positive('Valor total obrigatório'),
  entrada: z.coerce.number().min(0).default(0),
  sinal: z.coerce.number().min(0).default(0),
  qtdParcelasMensais: z.coerce.number().int().min(0).default(0),
  valorParcelaMensal: z.coerce.number().min(0).default(0),
  qtdSemestrais: z.coerce.number().int().min(0).default(0),
  valorSemestral: z.coerce.number().min(0).default(0),
  valorChaves: z.coerce.number().min(0).default(0),
  qtdPosChaves: z.coerce.number().int().min(0).default(0),
  valorPosChaves: z.coerce.number().min(0).default(0),
  indice: z.string().default('SemReajuste'),
  taxaJurosAnual: z.coerce.number().min(0).default(0)
});

type FormData = z.infer<typeof schema>;

interface Props {
  open: boolean;
  onClose: () => void;
}

export function NovaVendaDialog({ open, onClose }: Props) {
  const queryClient = useQueryClient();

  const propostasQuery = useQuery({
    queryKey: ['propostas', 'aceitas'],
    queryFn: () => propostasService.list({ status: 'Aceita', pageSize: 200 }),
    enabled: open
  });

  const clientesQuery = useQuery({
    queryKey: ['clientes', 'select'],
    queryFn: () => clientesService.list({ pageSize: 500 }),
    enabled: open
  });

  const apartamentosQuery = useQuery({
    queryKey: ['apartamentos', 'select', 'all'],
    queryFn: () => apartamentosService.list({ pageSize: 500 }),
    enabled: open
  });

  const usuariosQuery = useQuery({
    queryKey: ['usuarios', 'select'],
    queryFn: () => usuariosService.list(),
    enabled: open
  });

  const {
    register,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors, isSubmitting }
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      indice: 'SemReajuste',
      taxaJurosAnual: 0,
      entrada: 0,
      sinal: 0,
      qtdParcelasMensais: 0,
      valorParcelaMensal: 0,
      qtdSemestrais: 0,
      valorSemestral: 0,
      valorChaves: 0,
      qtdPosChaves: 0,
      valorPosChaves: 0
    }
  });

  useEffect(() => {
    if (open) {
      reset({
        propostaId: '',
        clienteId: 0,
        apartamentoId: 0,
        corretorId: 0,
        corretorCaptacaoId: '',
        valorFinal: 0,
        valorTotal: 0,
        entrada: 0,
        sinal: 0,
        qtdParcelasMensais: 0,
        valorParcelaMensal: 0,
        qtdSemestrais: 0,
        valorSemestral: 0,
        valorChaves: 0,
        qtdPosChaves: 0,
        valorPosChaves: 0,
        indice: 'SemReajuste',
        taxaJurosAnual: 0,
        observacoes: ''
      });
    }
  }, [open, reset]);

  const propostaIdSel = watch('propostaId');
  const propostaSelecionada = useMemo(
    () => propostasQuery.data?.items.find((p) => String(p.id) === propostaIdSel),
    [propostasQuery.data, propostaIdSel]
  );

  // Auto-preenche TODOS os campos quando uma proposta aceita eh selecionada.
  // Usamos reset() (em vez de setValue individual) para forçar re-render dos
  // inputs numericos uncontrolled — setValue so atualiza o state RHF, nao o DOM.
  useEffect(() => {
    if (propostaSelecionada) {
      const c = propostaSelecionada.condicao;
      reset({
        propostaId: String(propostaSelecionada.id),
        clienteId: propostaSelecionada.clienteId,
        apartamentoId: propostaSelecionada.apartamentoId,
        corretorId: propostaSelecionada.corretorId,
        corretorCaptacaoId: '',
        valorFinal: propostaSelecionada.valorOferecido,
        valorTotal: c.valorTotal,
        entrada: c.entrada,
        sinal: c.sinal,
        qtdParcelasMensais: c.qtdParcelasMensais,
        valorParcelaMensal: c.valorParcelaMensal,
        qtdSemestrais: c.qtdSemestrais,
        valorSemestral: c.valorSemestral,
        valorChaves: c.valorChaves,
        qtdPosChaves: c.qtdPosChaves,
        valorPosChaves: c.valorPosChaves,
        indice: c.indice,
        taxaJurosAnual: c.taxaJurosAnual,
        observacoes: ''
      });
    }
  }, [propostaSelecionada, reset]);

  const createMutation = useMutation({
    mutationFn: (d: FormData) =>
      vendasService.create({
        propostaId: d.propostaId ? Number(d.propostaId) : undefined,
        clienteId: d.clienteId,
        apartamentoId: d.apartamentoId,
        corretorId: d.corretorId,
        corretorCaptacaoId: d.corretorCaptacaoId ? Number(d.corretorCaptacaoId) : undefined,
        valorFinal: d.valorFinal,
        observacoes: d.observacoes,
        condicaoFinal: {
          valorTotal: d.valorTotal,
          entrada: d.entrada,
          sinal: d.sinal,
          qtdParcelasMensais: d.qtdParcelasMensais,
          valorParcelaMensal: d.valorParcelaMensal,
          qtdSemestrais: d.qtdSemestrais,
          valorSemestral: d.valorSemestral,
          valorChaves: d.valorChaves,
          qtdPosChaves: d.qtdPosChaves,
          valorPosChaves: d.valorPosChaves,
          indice: d.indice as IndiceReajuste,
          taxaJurosAnual: d.taxaJurosAnual
        }
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vendas'] });
      queryClient.invalidateQueries({ queryKey: ['apartamentos'] });
      onClose();
    }
  });

  const corretores = useMemo(
    () => (usuariosQuery.data ?? []).filter((u) => u.role === 'Corretor' || u.role === 'Gerente' || u.role === 'Admin'),
    [usuariosQuery.data]
  );

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <form onSubmit={handleSubmit((d) => createMutation.mutate(d))}>
        <DialogTitle>Nova venda</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            {/*
              MUI TextField select + RHF register() não dispara onChange de forma
              confiável — precisa modo controlado explicito via value+onChange
              com watch/setValue. Sem isso, setValue (auto-preencher a partir da
              proposta) não reflete na UI e form submit falha silenciosamente.
            */}
            <TextField
              select
              label="Proposta (aceita) — opcional"
              fullWidth
              value={watch('propostaId') ?? ''}
              onChange={(e) => setValue('propostaId', e.target.value, { shouldValidate: true })}
              helperText="Selecione para preencher automaticamente"
            >
              <MenuItem value="">Sem proposta vinculada</MenuItem>
              {(propostasQuery.data?.items ?? []).map((p) => (
                <MenuItem key={p.id} value={String(p.id)}>
                  {p.numero} — {p.clienteNome ?? p.clienteId} — {p.valorOferecido.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                </MenuItem>
              ))}
            </TextField>

            <TextField
              select
              label="Cliente"
              fullWidth
              value={watch('clienteId') || ''}
              onChange={(e) => setValue('clienteId', Number(e.target.value) || 0, { shouldValidate: true })}
              error={!!errors.clienteId}
              helperText={errors.clienteId?.message}
            >
              <MenuItem value="">Selecione...</MenuItem>
              {(clientesQuery.data?.items ?? []).map((c) => (
                <MenuItem key={c.id} value={c.id}>{c.nome}</MenuItem>
              ))}
            </TextField>

            <TextField
              select
              label="Apartamento"
              fullWidth
              value={watch('apartamentoId') || ''}
              onChange={(e) => setValue('apartamentoId', Number(e.target.value) || 0, { shouldValidate: true })}
              error={!!errors.apartamentoId}
              helperText={errors.apartamentoId?.message}
            >
              <MenuItem value="">Selecione...</MenuItem>
              {(apartamentosQuery.data?.items ?? []).map((a) => (
                <MenuItem key={a.id} value={a.id}>
                  {a.torreNome ?? '?'} — {a.numero} (pav {a.pavimento})
                </MenuItem>
              ))}
            </TextField>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField
                select
                label="Corretor de venda"
                fullWidth
                value={watch('corretorId') || ''}
                onChange={(e) => setValue('corretorId', Number(e.target.value) || 0, { shouldValidate: true })}
                error={!!errors.corretorId}
                helperText={errors.corretorId?.message}
              >
                <MenuItem value="">Selecione...</MenuItem>
                {corretores.map((u) => (
                  <MenuItem key={u.id} value={u.id}>{u.nome}</MenuItem>
                ))}
              </TextField>

              <TextField
                select
                label="Corretor de captação (opcional)"
                fullWidth
                value={watch('corretorCaptacaoId') ?? ''}
                onChange={(e) => setValue('corretorCaptacaoId', e.target.value)}
              >
                <MenuItem value="">-</MenuItem>
                {corretores.map((u) => (
                  <MenuItem key={u.id} value={u.id}>{u.nome}</MenuItem>
                ))}
              </TextField>
            </Stack>

            <TextField
              label="Valor final"
              type="number"
              fullWidth
              inputProps={{ step: '0.01' }}
              {...register('valorFinal')}
              error={!!errors.valorFinal}
              helperText={errors.valorFinal?.message}
            />

            <Divider textAlign="left"><Typography variant="caption">Condição final</Typography></Divider>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Valor total" type="number" fullWidth inputProps={{ step: '0.01' }} {...register('valorTotal')} error={!!errors.valorTotal} helperText={errors.valorTotal?.message} />
              <TextField label="Entrada" type="number" fullWidth inputProps={{ step: '0.01' }} {...register('entrada')} />
              <TextField label="Sinal" type="number" fullWidth inputProps={{ step: '0.01' }} {...register('sinal')} />
            </Stack>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Qtd parcelas mensais" type="number" fullWidth {...register('qtdParcelasMensais')} />
              <TextField label="Valor parcela mensal" type="number" fullWidth inputProps={{ step: '0.01' }} {...register('valorParcelaMensal')} />
            </Stack>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Qtd semestrais" type="number" fullWidth {...register('qtdSemestrais')} />
              <TextField label="Valor semestral" type="number" fullWidth inputProps={{ step: '0.01' }} {...register('valorSemestral')} />
            </Stack>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Valor chaves" type="number" fullWidth inputProps={{ step: '0.01' }} {...register('valorChaves')} />
              <TextField label="Qtd pós-chaves" type="number" fullWidth {...register('qtdPosChaves')} />
              <TextField label="Valor pós-chaves" type="number" fullWidth inputProps={{ step: '0.01' }} {...register('valorPosChaves')} />
            </Stack>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField select label="Índice de reajuste" fullWidth defaultValue="SemReajuste" {...register('indice')}>
                {INDICES.map((i) => (
                  <MenuItem key={i} value={i}>{i}</MenuItem>
                ))}
              </TextField>
              <TextField label="Taxa juros anual (%)" type="number" fullWidth inputProps={{ step: '0.01' }} {...register('taxaJurosAnual')} />
            </Stack>

            <TextField label="Observações" multiline minRows={2} fullWidth {...register('observacoes')} />

            {createMutation.isError ? (
              <Alert severity="error">Erro ao registrar venda. Verifique os dados.</Alert>
            ) : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} variant="text">Cancelar</Button>
          <Button type="submit" disabled={isSubmitting || createMutation.isPending}>
            {createMutation.isPending ? 'Salvando...' : 'Criar venda'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
