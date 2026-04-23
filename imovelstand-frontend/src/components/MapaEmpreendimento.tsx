import { useMemo } from 'react';
import { Box, Paper, Stack, Tooltip, Typography } from '@mui/material';
import type { ApartamentoResponse, StatusApartamento } from '@/types/api';

const STATUS_COLORS: Record<StatusApartamento, string> = {
  Disponivel: '#16a34a',
  Reservado: '#f59e0b',
  Proposta: '#ea580c',
  Vendido: '#dc2626',
  Bloqueado: '#64748b'
};

interface MapaEmpreendimentoProps {
  apartamentos: ApartamentoResponse[];
  torreNome?: string;
  onApartamentoClick?: (apt: ApartamentoResponse) => void;
}

function formatBRL(v: number): string {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
}

/**
 * Grid visual tipo "espelho de vendas" de um empreendimento.
 * Organiza apartamentos por pavimento (linhas, de cima pra baixo) e
 * por unidade dentro do pavimento (colunas). Cada célula colorida pelo status.
 */
export function MapaEmpreendimento({ apartamentos, torreNome, onApartamentoClick }: MapaEmpreendimentoProps) {
  const porPavimento = useMemo(() => {
    const grupos = new Map<number, ApartamentoResponse[]>();
    for (const apt of apartamentos) {
      const arr = grupos.get(apt.pavimento) ?? [];
      arr.push(apt);
      grupos.set(apt.pavimento, arr);
    }
    // Do topo pro térreo
    return Array.from(grupos.entries())
      .sort(([a], [b]) => b - a)
      .map(([pav, apts]) => [pav, apts.sort((a, b) => a.numero.localeCompare(b.numero))] as const);
  }, [apartamentos]);

  if (porPavimento.length === 0) {
    return (
      <Paper sx={{ p: 3, textAlign: 'center' }}>
        <Typography color="text.secondary">Sem apartamentos para exibir.</Typography>
      </Paper>
    );
  }

  return (
    <Paper sx={{ p: 2 }}>
      <Stack spacing={2}>
        {torreNome ? (
          <Typography variant="h6" fontWeight={700}>
            {torreNome}
          </Typography>
        ) : null}

        <Box sx={{ overflowX: 'auto' }}>
          <Stack spacing={0.5}>
            {porPavimento.map(([pav, apts]) => (
              <Stack key={pav} direction="row" spacing={0.5} alignItems="center">
                <Box
                  sx={{
                    minWidth: 64,
                    px: 1,
                    py: 1,
                    textAlign: 'center',
                    bgcolor: (t) => (t.palette.mode === 'dark' ? 'rgba(255,255,255,0.05)' : 'grey.100'),
                    color: 'text.secondary',
                    border: '1px solid',
                    borderColor: 'divider',
                    borderRadius: 1,
                    fontSize: 12,
                    fontWeight: 700
                  }}
                >
                  Pav {pav.toString().padStart(2, '0')}
                </Box>
                {apts.map((apt) => (
                  <Tooltip
                    key={apt.id}
                    title={
                      <Box>
                        <Typography variant="caption" display="block">
                          <strong>Apto {apt.numero}</strong>
                        </Typography>
                        <Typography variant="caption" display="block">
                          {apt.tipologiaNome ?? 'Tipologia —'}
                        </Typography>
                        <Typography variant="caption" display="block">
                          {formatBRL(apt.precoAtual)}
                        </Typography>
                        <Typography variant="caption" display="block">
                          Status: {apt.status}
                        </Typography>
                      </Box>
                    }
                  >
                    <Box
                      onClick={() => onApartamentoClick?.(apt)}
                      sx={{
                        minWidth: 80,
                        px: 1.5,
                        py: 1,
                        bgcolor: STATUS_COLORS[apt.status],
                        color: 'white',
                        borderRadius: 1,
                        textAlign: 'center',
                        cursor: onApartamentoClick ? 'pointer' : 'default',
                        transition: 'transform 0.15s',
                        '&:hover': onApartamentoClick ? { transform: 'scale(1.04)' } : undefined
                      }}
                    >
                      <Typography variant="body2" fontWeight={700}>{apt.numero}</Typography>
                      <Typography variant="caption" sx={{ opacity: 0.9 }}>
                        {formatBRL(apt.precoAtual)}
                      </Typography>
                    </Box>
                  </Tooltip>
                ))}
              </Stack>
            ))}
          </Stack>
        </Box>

        <Stack direction="row" spacing={2} sx={{ pt: 1, borderTop: '1px solid', borderColor: 'divider' }}>
          {(Object.keys(STATUS_COLORS) as StatusApartamento[]).map((s) => (
            <Stack key={s} direction="row" alignItems="center" spacing={0.5}>
              <Box sx={{ width: 12, height: 12, bgcolor: STATUS_COLORS[s], borderRadius: 0.5 }} />
              <Typography variant="caption">{s}</Typography>
            </Stack>
          ))}
        </Stack>
      </Stack>
    </Paper>
  );
}
