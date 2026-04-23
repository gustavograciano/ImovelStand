import { useEffect, useMemo, useState } from 'react';
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
  Divider,
  MenuItem,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { propostasService } from '@/services/propostasService';
import { clientesService } from '@/services/clientesService';
import { apartamentosService } from '@/services/apartamentosService';
import { usuariosService } from '@/services/usuariosService';
import { copilotoService } from '@/services/copilotoService';
import type { IndiceReajuste } from '@/types/api';

const INDICES: IndiceReajuste[] = ['SemReajuste', 'Incc', 'Ipca', 'Igpm', 'Tr', 'Selic'];

const schema = z.object({
  clienteId: z.coerce.number().int().positive('Selecione o cliente'),
  apartamentoId: z.coerce.number().int().positive('Selecione o apartamento'),
  corretorId: z.coerce.number().int().positive('Selecione o corretor'),
  valorOferecido: z.coerce.number().positive('Valor obrigatório'),
  dataValidade: z.string().optional(),
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
  apartamentoIdInicial?: number;
  clienteIdInicial?: number;
}

export function NovaPropostaDialog({ open, onClose, apartamentoIdInicial, clienteIdInicial }: Props) {
  const queryClient = useQueryClient();
  const [extrairOpen, setExtrairOpen] = useState(false);
  const [conversa, setConversa] = useState('');
  const [camposFaltantes, setCamposFaltantes] = useState<string[]>([]);

  const clientesQuery = useQuery({
    queryKey: ['clientes', 'select'],
    queryFn: () => clientesService.list({ pageSize: 500 }),
    enabled: open
  });

  const apartamentosQuery = useQuery({
    queryKey: ['apartamentos', 'select', 'disponiveis'],
    queryFn: () => apartamentosService.list({ status: 'Disponivel', pageSize: 500 }),
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
        clienteId: clienteIdInicial ?? 0,
        apartamentoId: apartamentoIdInicial ?? 0,
        corretorId: 0,
        valorOferecido: 0,
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
        dataValidade: '',
        observacoes: ''
      });
    }
  }, [open, apartamentoIdInicial, clienteIdInicial, reset]);

  const apartamentoSelecionadoId = watch('apartamentoId');
  const apartamentoSelecionado = useMemo(
    () => apartamentosQuery.data?.items.find((a) => a.id === Number(apartamentoSelecionadoId)),
    [apartamentosQuery.data, apartamentoSelecionadoId]
  );

  useEffect(() => {
    if (apartamentoSelecionado) {
      setValue('valorOferecido', apartamentoSelecionado.precoAtual);
      setValue('valorTotal', apartamentoSelecionado.precoAtual);
    }
  }, [apartamentoSelecionado, setValue]);

  const createMutation = useMutation({
    mutationFn: (d: FormData) =>
      propostasService.create({
        clienteId: d.clienteId,
        apartamentoId: d.apartamentoId,
        corretorId: d.corretorId,
        valorOferecido: d.valorOferecido,
        dataValidade: d.dataValidade || undefined,
        observacoes: d.observacoes,
        condicao: {
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
      queryClient.invalidateQueries({ queryKey: ['propostas'] });
      onClose();
    }
  });

  const extrairMutation = useMutation({
    mutationFn: ({ apartamentoId, conversa }: { apartamentoId: number; conversa: string }) =>
      copilotoService.extrairProposta(apartamentoId, conversa),
    onSuccess: (resp) => {
      if (!resp.sucesso || !resp.proposta) return;
      const p = resp.proposta;
      setValue('valorOferecido', p.valorOferecido || 0);
      if (p.observacoes) setValue('observacoes', p.observacoes);
      setValue('valorTotal', p.condicao.valorTotal || 0);
      setValue('entrada', p.condicao.entrada || 0);
      setValue('sinal', p.condicao.sinal || 0);
      setValue('qtdParcelasMensais', p.condicao.qtdParcelasMensais || 0);
      setValue('valorParcelaMensal', p.condicao.valorParcelaMensal || 0);
      setValue('qtdSemestrais', p.condicao.qtdSemestrais || 0);
      setValue('valorSemestral', p.condicao.valorSemestral || 0);
      setValue('valorChaves', p.condicao.valorChaves || 0);
      setValue('qtdPosChaves', p.condicao.qtdPosChaves || 0);
      setValue('valorPosChaves', p.condicao.valorPosChaves || 0);
      setValue('indice', p.condicao.indice || 'SemReajuste');
      setValue('taxaJurosAnual', p.condicao.taxaJurosAnual || 0);
      setCamposFaltantes(p.camposFaltantes ?? []);
      setExtrairOpen(false);
      setConversa('');
    }
  });

  const corretores = useMemo(
    () => (usuariosQuery.data ?? []).filter((u) => u.role === 'Corretor' || u.role === 'Gerente' || u.role === 'Admin'),
    [usuariosQuery.data]
  );

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <form onSubmit={handleSubmit((d) => createMutation.mutate(d))}>
        <DialogTitle>Nova proposta</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              select
              label="Cliente"
              fullWidth
              defaultValue=""
              {...register('clienteId')}
              error={!!errors.clienteId}
              helperText={errors.clienteId?.message}
            >
              <MenuItem value="">Selecione...</MenuItem>
              {(clientesQuery.data?.items ?? []).map((c) => (
                <MenuItem key={c.id} value={c.id}>{c.nome} — {c.cpf}</MenuItem>
              ))}
            </TextField>

            <TextField
              select
              label="Apartamento (disponíveis)"
              fullWidth
              defaultValue=""
              {...register('apartamentoId')}
              error={!!errors.apartamentoId}
              helperText={errors.apartamentoId?.message}
            >
              <MenuItem value="">Selecione...</MenuItem>
              {(apartamentosQuery.data?.items ?? []).map((a) => (
                <MenuItem key={a.id} value={a.id}>
                  {a.torreNome ?? '?'} — {a.numero} (pav {a.pavimento}) — {a.precoAtual.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                </MenuItem>
              ))}
            </TextField>

            <TextField
              select
              label="Corretor"
              fullWidth
              defaultValue=""
              {...register('corretorId')}
              error={!!errors.corretorId}
              helperText={errors.corretorId?.message}
            >
              <MenuItem value="">Selecione...</MenuItem>
              {corretores.map((u) => (
                <MenuItem key={u.id} value={u.id}>{u.nome} ({u.role})</MenuItem>
              ))}
            </TextField>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField
                label="Valor oferecido"
                type="number"
                fullWidth
                inputProps={{ step: '0.01' }}
                {...register('valorOferecido')}
                error={!!errors.valorOferecido}
                helperText={errors.valorOferecido?.message}
              />
              <TextField
                label="Data validade"
                type="date"
                fullWidth
                InputLabelProps={{ shrink: true }}
                {...register('dataValidade')}
              />
            </Stack>

            <Divider textAlign="left"><Typography variant="caption">Condição de pagamento</Typography></Divider>

            <Stack direction="row" spacing={1} alignItems="center" justifyContent="space-between" flexWrap="wrap">
              <Button
                size="small"
                variant="outlined"
                startIcon={<AutoAwesomeIcon />}
                onClick={() => setExtrairOpen(true)}
                disabled={!apartamentoSelecionado}
              >
                Colar conversa (extrair com IA)
              </Button>
              {camposFaltantes.length > 0 ? (
                <Stack direction="row" spacing={0.5} alignItems="center" flexWrap="wrap">
                  <Typography variant="caption" color="warning.main">Campos incertos:</Typography>
                  {camposFaltantes.map((c) => (
                    <Chip key={c} size="small" label={c} color="warning" variant="outlined" />
                  ))}
                </Stack>
              ) : null}
            </Stack>

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
              <TextField
                select
                label="Índice de reajuste"
                fullWidth
                defaultValue="SemReajuste"
                {...register('indice')}
              >
                {INDICES.map((i) => (
                  <MenuItem key={i} value={i}>{i}</MenuItem>
                ))}
              </TextField>
              <TextField
                label="Taxa juros anual (%)"
                type="number"
                fullWidth
                inputProps={{ step: '0.01' }}
                {...register('taxaJurosAnual')}
              />
            </Stack>

            <TextField
              label="Observações"
              multiline
              minRows={2}
              fullWidth
              {...register('observacoes')}
            />

            {createMutation.isError ? (
              <Alert severity="error">Erro ao criar proposta. Verifique os dados.</Alert>
            ) : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} variant="text">Cancelar</Button>
          <Button type="submit" disabled={isSubmitting || createMutation.isPending}>
            {createMutation.isPending ? 'Salvando...' : 'Criar proposta'}
          </Button>
        </DialogActions>
      </form>

      <Dialog open={extrairOpen} onClose={() => setExtrairOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Stack direction="row" spacing={1} alignItems="center">
            <AutoAwesomeIcon color="primary" fontSize="small" />
            <span>Extrair proposta de conversa</span>
          </Stack>
        </DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <Typography variant="body2" color="text.secondary">
              Cole aqui a conversa com o cliente (WhatsApp, email, anotações). A IA vai extrair valores, parcelas, entrada e demais campos automaticamente. Você revisa antes de salvar.
            </Typography>
            {!apartamentoSelecionado ? (
              <Alert severity="warning">Selecione um apartamento antes de usar o extrator — o valor tabela ancora a análise.</Alert>
            ) : null}
            <TextField
              multiline
              minRows={10}
              maxRows={20}
              fullWidth
              placeholder="Cliente: Aceito pagar 800 mil com entrada de 80 mil e 120 parcelas de 6 mil..."
              value={conversa}
              onChange={(e) => setConversa(e.target.value)}
              disabled={extrairMutation.isPending}
            />
            {extrairMutation.isPending ? (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, py: 1 }}>
                <CircularProgress size={18} />
                <Typography variant="caption" color="text.secondary">Analisando conversa…</Typography>
              </Box>
            ) : null}
            {extrairMutation.data && !extrairMutation.data.sucesso ? (
              <Alert severity="error">{extrairMutation.data.mensagemErro ?? 'Erro ao extrair.'}</Alert>
            ) : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button variant="text" onClick={() => setExtrairOpen(false)}>Cancelar</Button>
          <Button
            onClick={() => apartamentoSelecionado && extrairMutation.mutate({ apartamentoId: apartamentoSelecionado.id, conversa })}
            disabled={!apartamentoSelecionado || !conversa.trim() || extrairMutation.isPending}
          >
            Extrair
          </Button>
        </DialogActions>
      </Dialog>
    </Dialog>
  );
}
