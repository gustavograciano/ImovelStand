import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Divider,
  LinearProgress,
  Stack,
  Typography
} from '@mui/material';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import { analiseCreditoService, type AnaliseCredito } from '@/services/analiseCreditoService';

interface Props {
  clienteId: number;
}

function brl(n?: number | null): string {
  if (n == null) return '—';
  return n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

const STATUS_COLORS: Record<AnaliseCredito['status'], 'default' | 'info' | 'warning' | 'success' | 'error'> = {
  Pendente: 'warning',
  EmProcessamento: 'info',
  Concluida: 'success',
  Revogada: 'default',
  Falhou: 'error'
};

export function AnaliseCreditoCard({ clienteId }: Props) {
  const queryClient = useQueryClient();
  const [linkCopiado, setLinkCopiado] = useState<string | null>(null);

  const q = useQuery({
    queryKey: ['analises-credito', clienteId],
    queryFn: () => analiseCreditoService.listarDoCliente(clienteId)
  });

  const solicitar = useMutation({
    mutationFn: () => analiseCreditoService.solicitar(clienteId),
    onSuccess: (data) => {
      setLinkCopiado(data.connectUrl);
      queryClient.invalidateQueries({ queryKey: ['analises-credito', clienteId] });
    }
  });

  const autorizarStub = useMutation({
    mutationFn: (id: number) => analiseCreditoService.autorizarStub(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['analises-credito', clienteId] })
  });

  const revogar = useMutation({
    mutationFn: (id: number) => analiseCreditoService.revogar(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['analises-credito', clienteId] })
  });

  const ativa = (q.data ?? []).find((a) => a.status === 'Concluida' || a.status === 'EmProcessamento' || a.status === 'Pendente');

  return (
    <Card>
      <CardContent>
        <Stack direction="row" spacing={1} alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
          <Stack direction="row" spacing={1} alignItems="center">
            <AccountBalanceIcon color="primary" />
            <Typography variant="subtitle1" fontWeight={700}>Análise de Crédito (Open Finance)</Typography>
          </Stack>
          {!ativa ? (
            <Button
              variant="outlined"
              onClick={() => solicitar.mutate()}
              disabled={solicitar.isPending}
            >
              {solicitar.isPending ? 'Criando...' : 'Solicitar análise'}
            </Button>
          ) : null}
        </Stack>

        {solicitar.data && linkCopiado ? (
          <Alert severity="info" sx={{ mb: 2 }}
            action={
              <Button
                size="small"
                startIcon={<ContentCopyIcon fontSize="small" />}
                onClick={() => {
                  navigator.clipboard.writeText(linkCopiado);
                  setLinkCopiado(null);
                }}
              >
                Copiar
              </Button>
            }
          >
            Link gerado. Envie ao cliente via WhatsApp/email:
            <Typography variant="caption" component="div" sx={{ mt: 0.5, wordBreak: 'break-all', fontFamily: 'monospace' }}>
              {linkCopiado}
            </Typography>
          </Alert>
        ) : null}

        {q.isLoading ? (
          <Box sx={{ py: 2, textAlign: 'center' }}><CircularProgress size={18} /></Box>
        ) : (q.data ?? []).length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            Nenhuma análise solicitada ainda. Cliente autoriza via Open Finance e recebemos os extratos em minutos.
          </Typography>
        ) : (
          <Stack spacing={2}>
            {(q.data ?? []).map((a) => (
              <Box
                key={a.id}
                sx={{
                  p: 2,
                  border: '1px solid',
                  borderColor: 'divider',
                  borderRadius: 1.5,
                  bgcolor: (t) => t.palette.mode === 'dark' ? 'rgba(255,255,255,0.02)' : 'grey.50'
                }}
              >
                <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
                  <Stack direction="row" spacing={1} alignItems="center">
                    <Chip size="small" label={a.status} color={STATUS_COLORS[a.status]} />
                    <Typography variant="caption" color="text.secondary">
                      {new Date(a.createdAt).toLocaleDateString('pt-BR')} · {a.provedor}
                    </Typography>
                  </Stack>
                  {a.status === 'Pendente' ? (
                    <Button
                      size="small"
                      variant="contained"
                      startIcon={<OpenInNewIcon />}
                      onClick={() => autorizarStub.mutate(a.id)}
                      disabled={autorizarStub.isPending}
                    >
                      Simular autorização (dev)
                    </Button>
                  ) : null}
                  {a.status === 'Concluida' ? (
                    <Button
                      size="small"
                      color="error"
                      startIcon={<DeleteOutlineIcon fontSize="small" />}
                      onClick={() => { if (confirm('Revogar consentimento apaga todos os dados financeiros coletados. Continuar?')) revogar.mutate(a.id); }}
                    >
                      Revogar (LGPD)
                    </Button>
                  ) : null}
                </Stack>

                {a.status === 'Concluida' && a.score != null ? (
                  <>
                    <Stack direction="row" spacing={2} sx={{ mb: 1.5 }}>
                      <Box sx={{ flex: 1 }}>
                        <Typography variant="caption" color="text.secondary">Score ImovelStand</Typography>
                        <Typography variant="h4" fontWeight={700} color={
                          a.score >= 700 ? 'success.main' : a.score >= 500 ? 'primary.main' : 'warning.main'
                        }>
                          {a.score}
                        </Typography>
                        <LinearProgress
                          variant="determinate"
                          value={a.score / 10}
                          color={a.score >= 700 ? 'success' : a.score >= 500 ? 'primary' : 'warning'}
                          sx={{ height: 5, borderRadius: 3, mt: 0.5 }}
                        />
                      </Box>
                    </Stack>

                    <Divider sx={{ my: 1.5 }} />

                    <Stack spacing={0.5}>
                      <Row label="Renda média comprovada" value={brl(a.rendaMediaComprovada)} highlight />
                      <Row label="Volatilidade da renda" value={brl(a.volatilidadeRenda)} />
                      <Row label="Dívidas recorrentes/mês" value={brl(a.dividasRecorrentes)} />
                      <Row label="Capacidade de pagamento (30%)" value={brl(a.capacidadePagamento)} highlight />
                    </Stack>

                    {a.alertas.length > 0 ? (
                      <Alert severity="warning" sx={{ mt: 1.5 }}>
                        <Typography variant="caption" fontWeight={700} component="div" sx={{ mb: 0.5 }}>
                          Pontos de atenção:
                        </Typography>
                        <Box component="ul" sx={{ m: 0, pl: 2 }}>
                          {a.alertas.map((al, i) => (
                            <li key={i}><Typography variant="caption">{al}</Typography></li>
                          ))}
                        </Box>
                      </Alert>
                    ) : null}

                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1.5 }}>
                      Dados expiram automaticamente em {new Date(a.expiraEm).toLocaleDateString('pt-BR')} (12 meses, conforme Bacen/LGPD).
                    </Typography>
                  </>
                ) : a.status === 'EmProcessamento' ? (
                  <Stack direction="row" spacing={1} alignItems="center">
                    <CircularProgress size={14} />
                    <Typography variant="caption">Aguardando dados do banco…</Typography>
                  </Stack>
                ) : a.status === 'Falhou' ? (
                  <Alert severity="error">{a.mensagemErro}</Alert>
                ) : null}
              </Box>
            ))}
          </Stack>
        )}
      </CardContent>
    </Card>
  );
}

function Row({ label, value, highlight }: { label: string; value: string; highlight?: boolean }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', py: 0.25 }}>
      <Typography variant="body2" color="text.secondary">{label}</Typography>
      <Typography variant="body2" fontWeight={highlight ? 700 : 500} color={highlight ? 'primary.main' : 'text.primary'}>
        {value}
      </Typography>
    </Box>
  );
}
