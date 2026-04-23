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
 * Agrupa por Torre (blocos separados) e dentro de cada torre, por pavimento
 * (linhas de cima pra baixo) e unidades (colunas). Cada célula colorida
 * pelo status.
 */
export function MapaEmpreendimento({ apartamentos, torreNome, onApartamentoClick }: MapaEmpreendimentoProps) {
  const porTorre = useMemo(() => {
    // Agrupa por torre (usa torreNome; fallback para torreId se o nome vier null)
    const porTorreMap = new Map<string, ApartamentoResponse[]>();
    for (const apt of apartamentos) {
      const chave = apt.torreNome ?? `Torre #${apt.torreId}`;
      const arr = porTorreMap.get(chave) ?? [];
      arr.push(apt);
      porTorreMap.set(chave, arr);
    }
    // Dentro de cada torre, agrupa por pavimento (topo -> térreo)
    return Array.from(porTorreMap.entries())
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([nomeTorre, apts]) => {
        const porPav = new Map<number, ApartamentoResponse[]>();
        for (const apt of apts) {
          const arr = porPav.get(apt.pavimento) ?? [];
          arr.push(apt);
          porPav.set(apt.pavimento, arr);
        }
        const pavimentos = Array.from(porPav.entries())
          .sort(([a], [b]) => b - a)
          .map(([pav, list]) => [pav, list.sort((a, b) => a.numero.localeCompare(b.numero))] as const);
        return [nomeTorre, pavimentos] as const;
      });
  }, [apartamentos]);

  if (porTorre.length === 0) {
    return (
      <Paper sx={{ p: 3, textAlign: 'center' }}>
        <Typography color="text.secondary">Sem apartamentos para exibir.</Typography>
      </Paper>
    );
  }

  return (
    <Stack spacing={3}>
      {porTorre.map(([nomeTorre, pavimentos]) => (
        <Paper key={nomeTorre} sx={{ p: 2 }}>
          <Stack spacing={2}>
            <Typography variant="h6" fontWeight={700}>
              {torreNome ?? nomeTorre}
            </Typography>

            <Box sx={{ overflowX: 'auto' }}>
              <Stack spacing={0.5}>
                {pavimentos.map(([pav, apts]) => (
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
          </Stack>
        </Paper>
      ))}

      {/* Legenda unica para todas as torres */}
      <Paper sx={{ p: 1.5 }}>
        <Stack direction="row" spacing={2} flexWrap="wrap">
          {(Object.keys(STATUS_COLORS) as StatusApartamento[]).map((s) => (
            <Stack key={s} direction="row" alignItems="center" spacing={0.5}>
              <Box sx={{ width: 12, height: 12, bgcolor: STATUS_COLORS[s], borderRadius: 0.5 }} />
              <Typography variant="caption">{s}</Typography>
            </Stack>
          ))}
        </Stack>
      </Paper>
    </Stack>
  );
}
