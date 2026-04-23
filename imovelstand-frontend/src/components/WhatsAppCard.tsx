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
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  MenuItem,
  Stack,
  TextField,
  Tooltip,
  Typography
} from '@mui/material';
import DoneAllIcon from '@mui/icons-material/DoneAll';
import DoneIcon from '@mui/icons-material/Done';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutlineOutlined';
import ScheduleIcon from '@mui/icons-material/Schedule';
import SendIcon from '@mui/icons-material/Send';
import WhatsAppIcon from '@mui/icons-material/WhatsApp';
import { whatsappService, type WhatsAppMensagem } from '@/services/whatsappService';

interface Props {
  clienteId: number;
}

function StatusIcon({ status }: { status: WhatsAppMensagem['status'] }) {
  switch (status) {
    case 'Lida':
      return <Tooltip title="Lida"><DoneAllIcon sx={{ fontSize: 14, color: '#34b7f1' }} /></Tooltip>;
    case 'Entregue':
      return <Tooltip title="Entregue"><DoneAllIcon sx={{ fontSize: 14 }} /></Tooltip>;
    case 'Aceita':
      return <Tooltip title="Enviada"><DoneIcon sx={{ fontSize: 14 }} /></Tooltip>;
    case 'Falhou':
      return <Tooltip title="Falhou"><ErrorOutlineIcon sx={{ fontSize: 14, color: 'error.main' }} /></Tooltip>;
    default:
      return <Tooltip title="Pendente"><ScheduleIcon sx={{ fontSize: 14, color: 'text.secondary' }} /></Tooltip>;
  }
}

export function WhatsAppCard({ clienteId }: Props) {
  const queryClient = useQueryClient();
  const [templateDialogOpen, setTemplateDialogOpen] = useState(false);
  const [selectedTemplateId, setSelectedTemplateId] = useState<number | ''>('');
  const [variaveis, setVariaveis] = useState<string[]>([]);
  const [textoLivre, setTextoLivre] = useState('');

  const msgsQuery = useQuery({
    queryKey: ['whatsapp-mensagens', clienteId],
    queryFn: () => whatsappService.listarMensagens(clienteId),
    refetchInterval: 1000 * 30 // poll a cada 30s
  });

  const templatesQuery = useQuery({
    queryKey: ['whatsapp-templates'],
    queryFn: () => whatsappService.listarTemplates(),
    enabled: templateDialogOpen
  });

  const enviarTemplate = useMutation({
    mutationFn: () => whatsappService.enviarTemplate(clienteId, Number(selectedTemplateId), variaveis),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['whatsapp-mensagens', clienteId] });
      setTemplateDialogOpen(false);
      setSelectedTemplateId('');
      setVariaveis([]);
    }
  });

  const enviarTexto = useMutation({
    mutationFn: () => whatsappService.enviarTexto(clienteId, textoLivre),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['whatsapp-mensagens', clienteId] });
      setTextoLivre('');
    }
  });

  const templateSelecionado = templatesQuery.data?.find((t) => t.id === Number(selectedTemplateId));

  return (
    <>
      <Card>
        <CardContent>
          <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
            <Stack direction="row" spacing={1} alignItems="center">
              <WhatsAppIcon sx={{ color: '#25D366' }} />
              <Typography variant="subtitle1" fontWeight={700}>WhatsApp</Typography>
            </Stack>
            <Button
              size="small"
              variant="outlined"
              startIcon={<SendIcon />}
              onClick={() => setTemplateDialogOpen(true)}
            >
              Enviar template
            </Button>
          </Stack>

          {/* Timeline de mensagens */}
          <Box
            sx={{
              maxHeight: 340,
              overflowY: 'auto',
              bgcolor: (t) => t.palette.mode === 'dark' ? 'rgba(255,255,255,0.02)' : 'grey.50',
              borderRadius: 1,
              p: 1.5,
              mb: 2
            }}
          >
            {msgsQuery.isLoading ? (
              <Box sx={{ py: 2, textAlign: 'center' }}><CircularProgress size={18} /></Box>
            ) : (msgsQuery.data ?? []).length === 0 ? (
              <Typography variant="caption" color="text.secondary">
                Nenhuma mensagem trocada com este cliente ainda.
              </Typography>
            ) : (
              <Stack spacing={1}>
                {(msgsQuery.data ?? []).slice().reverse().map((m) => (
                  <Box
                    key={m.id}
                    sx={{
                      display: 'flex',
                      justifyContent: m.direcao === 'Enviada' ? 'flex-end' : 'flex-start'
                    }}
                  >
                    <Box
                      sx={{
                        maxWidth: '75%',
                        p: 1.2,
                        borderRadius: 2,
                        bgcolor: m.direcao === 'Enviada' ? '#dcf8c6' : '#ffffff',
                        border: '1px solid',
                        borderColor: 'divider',
                        color: 'text.primary'
                      }}
                    >
                      <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', mb: 0.5 }}>
                        {m.conteudo}
                      </Typography>
                      <Stack direction="row" spacing={0.5} alignItems="center" justifyContent="flex-end">
                        <Typography variant="caption" color="text.secondary">
                          {new Date(m.createdAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                        </Typography>
                        {m.direcao === 'Enviada' ? <StatusIcon status={m.status} /> : null}
                      </Stack>
                    </Box>
                  </Box>
                ))}
              </Stack>
            )}
          </Box>

          {/* Texto livre (requer janela 24h) */}
          <Stack direction="row" spacing={1}>
            <TextField
              size="small"
              fullWidth
              placeholder="Responder (apenas dentro da janela de 24h após resposta do cliente)"
              value={textoLivre}
              onChange={(e) => setTextoLivre(e.target.value)}
              disabled={enviarTexto.isPending}
              multiline
              maxRows={3}
            />
            <Button
              onClick={() => enviarTexto.mutate()}
              disabled={!textoLivre.trim() || enviarTexto.isPending}
              endIcon={<SendIcon />}
            >
              Enviar
            </Button>
          </Stack>
          {enviarTexto.isError ? (
            <Alert severity="warning" sx={{ mt: 1 }}>
              Fora da janela de 24h. Use um template aprovado.
            </Alert>
          ) : null}
        </CardContent>
      </Card>

      {/* Dialog de seleção de template */}
      <Dialog open={templateDialogOpen} onClose={() => setTemplateDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Enviar template WhatsApp</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              select
              fullWidth
              label="Template"
              value={selectedTemplateId}
              onChange={(e) => {
                const id = e.target.value ? Number(e.target.value) : '';
                setSelectedTemplateId(id);
                const tpl = templatesQuery.data?.find((t) => t.id === id);
                setVariaveis(tpl ? new Array(tpl.qtdVariaveis).fill('') : []);
              }}
            >
              <MenuItem value="">Selecione...</MenuItem>
              {(templatesQuery.data ?? []).filter((t) => t.ativo).map((t) => (
                <MenuItem key={t.id} value={t.id}>
                  {t.descricao ?? t.nome}
                </MenuItem>
              ))}
            </TextField>

            {templateSelecionado ? (
              <Box sx={{ p: 1.5, bgcolor: 'background.default', borderRadius: 1, border: '1px solid', borderColor: 'divider' }}>
                <Typography variant="caption" color="text.secondary">Preview:</Typography>
                <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', mt: 0.5 }}>
                  {variaveis.reduce((txt, v, i) => txt.replace(`{{${i + 1}}}`, v || `{{${i + 1}}}`), templateSelecionado.corpo)}
                </Typography>
              </Box>
            ) : null}

            {variaveis.map((v, i) => (
              <TextField
                key={i}
                label={`Variável {{${i + 1}}}`}
                value={v}
                onChange={(e) => {
                  const novas = [...variaveis];
                  novas[i] = e.target.value;
                  setVariaveis(novas);
                }}
                fullWidth
              />
            ))}

            {enviarTemplate.isError ? <Alert severity="error">Erro ao enviar.</Alert> : null}
            {enviarTemplate.data?.mensagemErro ? (
              <Alert severity="error">{enviarTemplate.data.mensagemErro}</Alert>
            ) : null}

            {enviarTemplate.data?.status ? (
              <Chip
                size="small"
                label={`Status: ${enviarTemplate.data.status}`}
                color={enviarTemplate.data.status === 'Falhou' ? 'error' : 'success'}
              />
            ) : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button variant="text" onClick={() => setTemplateDialogOpen(false)}>Cancelar</Button>
          <Button
            onClick={() => enviarTemplate.mutate()}
            disabled={!selectedTemplateId || variaveis.some((v) => !v.trim()) || enviarTemplate.isPending}
          >
            {enviarTemplate.isPending ? 'Enviando...' : 'Enviar'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
