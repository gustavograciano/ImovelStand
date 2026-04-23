import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link as RouterLink, useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Breadcrumbs,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Divider,
  Grid,
  Link,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import GppGoodIcon from '@mui/icons-material/GppGood';
import { clientesService } from '@/services/clientesService';
import { BriefingClienteCard } from '@/components/BriefingClienteCard';
import { ObjecoesCard } from '@/components/ObjecoesCard';
import type { TipoInteracao } from '@/types/api';

const TIPOS: TipoInteracao[] = ['Ligacao', 'Whatsapp', 'Email', 'ReuniaoPresencial', 'ReuniaoVideo', 'Visita', 'MensagemInterna'];

const FUNIL_COLORS: Record<string, 'default' | 'primary' | 'warning' | 'success' | 'info' | 'error'> = {
  Lead: 'default',
  Contato: 'info',
  Visita: 'primary',
  Proposta: 'warning',
  Negociacao: 'warning',
  Venda: 'success',
  Descarte: 'error'
};

export function ClienteDetailPage() {
  const { id } = useParams<{ id: string }>();
  const clienteId = Number(id);
  const queryClient = useQueryClient();

  const [tipo, setTipo] = useState<TipoInteracao>('Ligacao');
  const [conteudo, setConteudo] = useState('');

  const cliente = useQuery({
    queryKey: ['cliente', clienteId],
    queryFn: () => clientesService.get(clienteId),
    enabled: !isNaN(clienteId)
  });

  const interacoes = useQuery({
    queryKey: ['cliente-interacoes', clienteId],
    queryFn: () => clientesService.listInteracoes(clienteId),
    enabled: !isNaN(clienteId)
  });

  const addInteracao = useMutation({
    mutationFn: () => clientesService.addInteracao(clienteId, tipo, conteudo),
    onSuccess: () => {
      setConteudo('');
      queryClient.invalidateQueries({ queryKey: ['cliente-interacoes', clienteId] });
    }
  });

  const handleExportLgpd = async () => {
    const data = await clientesService.exportLgpd(clienteId);
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `cliente-${clienteId}-lgpd-export.json`;
    a.click();
    URL.revokeObjectURL(url);
  };

  if (cliente.isLoading) {
    return <Box sx={{ p: 4, display: 'grid', placeItems: 'center' }}><CircularProgress /></Box>;
  }
  if (cliente.isError || !cliente.data) {
    return <Alert severity="error">Cliente não encontrado.</Alert>;
  }

  const c = cliente.data;

  return (
    <Stack spacing={3}>
      <Breadcrumbs>
        <Link component={RouterLink} to="/clientes" underline="hover">Clientes</Link>
        <Typography color="text.primary">{c.nome}</Typography>
      </Breadcrumbs>

      <BriefingClienteCard clienteId={c.id} />

      <ObjecoesCard clienteId={c.id} />

      <Grid container spacing={2}>
        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" justifyContent="space-between" mb={2}>
                <Stack>
                  <Typography variant="h5" fontWeight={700}>{c.nome}</Typography>
                  <Typography color="text.secondary">{c.email} · {c.telefone}</Typography>
                </Stack>
                <Chip label={c.statusFunil} color={FUNIL_COLORS[c.statusFunil]} />
              </Stack>
              <Divider sx={{ my: 2 }} />
              <Grid container spacing={2}>
                <InfoRow label="CPF" value={c.cpf} />
                <InfoRow label="RG" value={c.rg ?? '—'} />
                <InfoRow label="Data nasc." value={c.dataNascimento ? new Date(c.dataNascimento).toLocaleDateString('pt-BR') : '—'} />
                <InfoRow label="Estado civil" value={c.estadoCivil ?? '—'} />
                <InfoRow label="Regime de bens" value={c.regimeBens ?? '—'} />
                <InfoRow label="Profissão" value={c.profissao ?? '—'} />
                <InfoRow label="Empresa" value={c.empresa ?? '—'} />
                <InfoRow label="Renda mensal" value={c.rendaMensal ? c.rendaMensal.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) : '—'} />
                <InfoRow label="WhatsApp" value={c.whatsapp ?? '—'} />
                <InfoRow label="Origem" value={c.origemLead ?? '—'} />
                <InfoRow label="Corretor responsável" value={c.corretorResponsavelId ? `#${c.corretorResponsavelId}` : '—'} />
                <InfoRow
                  label="Endereço"
                  value={
                    c.endereco
                      ? `${c.endereco.logradouro}, ${c.endereco.numero}${c.endereco.complemento ? ' ' + c.endereco.complemento : ''} — ${c.endereco.bairro}, ${c.endereco.cidade}/${c.endereco.uf} — CEP ${c.endereco.cep}`
                      : '—'
                  }
                />
                <InfoRow
                  label="Consentimento LGPD"
                  value={c.consentimentoLgpd ? `Sim (${c.consentimentoLgpdEm ? new Date(c.consentimentoLgpdEm).toLocaleDateString('pt-BR') : '-'})` : 'Não'}
                />
              </Grid>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight={700} gutterBottom>LGPD</Typography>
              <Typography variant="body2" color="text.secondary" paragraph>
                Exporte todos os dados do cliente (Art. 18 LGPD).
              </Typography>
              <Button startIcon={<DownloadIcon />} onClick={handleExportLgpd} fullWidth>
                Exportar dados (JSON)
              </Button>
              {c.consentimentoLgpd ? (
                <Stack direction="row" alignItems="center" spacing={0.5} mt={2}>
                  <GppGoodIcon color="success" fontSize="small" />
                  <Typography variant="caption" color="success.main">
                    Consentimento ativo
                  </Typography>
                </Stack>
              ) : null}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" fontWeight={700} gutterBottom>Timeline de interações</Typography>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems="flex-start" mb={2}>
          <TextField
            select
            size="small"
            label="Tipo"
            value={tipo}
            onChange={(e) => setTipo(e.target.value as TipoInteracao)}
            sx={{ minWidth: 180 }}
          >
            {TIPOS.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
          </TextField>
          <TextField
            size="small"
            label="Descreva o que aconteceu"
            value={conteudo}
            onChange={(e) => setConteudo(e.target.value)}
            fullWidth
            multiline
            minRows={1}
          />
          <Button
            onClick={() => addInteracao.mutate()}
            disabled={!conteudo.trim() || addInteracao.isPending}
          >
            Adicionar
          </Button>
        </Stack>
        <Stack spacing={1}>
          {(interacoes.data ?? []).map((i) => (
            <Paper key={i.id} variant="outlined" sx={{ p: 1.5 }}>
              <Stack direction="row" justifyContent="space-between" alignItems="flex-start" spacing={2}>
                <Stack>
                  <Typography variant="body2">{i.conteudo}</Typography>
                  <Typography variant="caption" color="text.secondary">
                    {i.usuarioNome ?? 'Sistema'} · {new Date(i.dataHora).toLocaleString('pt-BR')}
                  </Typography>
                </Stack>
                <Chip size="small" label={i.tipo} />
              </Stack>
            </Paper>
          ))}
          {(interacoes.data ?? []).length === 0 ? (
            <Typography color="text.secondary" py={2}>Sem interações registradas ainda.</Typography>
          ) : null}
        </Stack>
      </Paper>
    </Stack>
  );
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <Grid item xs={12} sm={6}>
      <Typography variant="caption" color="text.secondary">{label}</Typography>
      <Typography variant="body2" fontWeight={500}>{value}</Typography>
    </Grid>
  );
}
