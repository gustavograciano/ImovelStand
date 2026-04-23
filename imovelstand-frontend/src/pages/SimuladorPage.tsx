import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Divider,
  Grid,
  InputAdornment,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography
} from '@mui/material';
import CalculateIcon from '@mui/icons-material/Calculate';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import WarningIcon from '@mui/icons-material/Warning';
import { simuladorService, type SimulacaoCompletaResult } from '@/services/simuladorService';

const UFS = ['SP', 'RJ', 'MG', 'PR', 'RS', 'BA', 'SC', 'PE', 'CE', 'DF', 'GO', 'ES'];

function brl(n: number): string {
  return n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function pct(n: number): string {
  return `${n.toFixed(2).replace('.', ',')}%`;
}

export function SimuladorPage() {
  const [valorImovel, setValorImovel] = useState<string>('500000');
  const [entrada, setEntrada] = useState<string>('100000');
  const [uf, setUf] = useState<string>('SP');
  const [rendaMensal, setRendaMensal] = useState<string>('12000');
  const [outrasDividas, setOutrasDividas] = useState<string>('0');
  const [aluguelAtual, setAluguelAtual] = useState<string>('2500');
  const [qtdParcelasDireto, setQtdParcelasDireto] = useState<string>('120');
  const [prazoAnos, setPrazoAnos] = useState<string>('30');

  const simular = useMutation({
    mutationFn: () =>
      simuladorService.simular({
        valorImovel: Number(valorImovel) || 0,
        entrada: Number(entrada) || 0,
        uf,
        rendaMensal: Number(rendaMensal) || 0,
        outrasDividas: Number(outrasDividas) || 0,
        aluguelAtual: Number(aluguelAtual) || 0,
        qtdParcelasDireto: Number(qtdParcelasDireto) || 0,
        prazoAnos: Number(prazoAnos) || 30
      })
  });

  const result = simular.data;

  return (
    <Stack spacing={3}>
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" fontWeight={700} gutterBottom>
          Dados da simulação
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          Informe os dados do cliente e do imóvel. Preenchimento opcional para aluguel e parcelamento direto.
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6} md={3}>
            <TextField
              label="Valor do imóvel"
              fullWidth
              type="number"
              value={valorImovel}
              onChange={(e) => setValorImovel(e.target.value)}
              InputProps={{ startAdornment: <InputAdornment position="start">R$</InputAdornment> }}
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <TextField
              label="Entrada"
              fullWidth
              type="number"
              value={entrada}
              onChange={(e) => setEntrada(e.target.value)}
              InputProps={{ startAdornment: <InputAdornment position="start">R$</InputAdornment> }}
            />
          </Grid>
          <Grid item xs={6} sm={3} md={2}>
            <TextField
              select
              label="UF"
              fullWidth
              value={uf}
              onChange={(e) => setUf(e.target.value)}
            >
              {UFS.map((u) => <MenuItem key={u} value={u}>{u}</MenuItem>)}
            </TextField>
          </Grid>
          <Grid item xs={6} sm={3} md={2}>
            <TextField
              label="Prazo (anos)"
              fullWidth
              type="number"
              value={prazoAnos}
              onChange={(e) => setPrazoAnos(e.target.value)}
            />
          </Grid>
          <Grid item xs={6} sm={3} md={2}>
            <TextField
              label="Parcelas direto"
              fullWidth
              type="number"
              value={qtdParcelasDireto}
              onChange={(e) => setQtdParcelasDireto(e.target.value)}
              helperText="Vezes no parcelamento direto"
            />
          </Grid>
          <Grid item xs={12} sm={6} md={4}>
            <TextField
              label="Renda mensal"
              fullWidth
              type="number"
              value={rendaMensal}
              onChange={(e) => setRendaMensal(e.target.value)}
              InputProps={{ startAdornment: <InputAdornment position="start">R$</InputAdornment> }}
              helperText="Para capacidade de pagamento"
            />
          </Grid>
          <Grid item xs={12} sm={6} md={4}>
            <TextField
              label="Outras dívidas (recorrentes)"
              fullWidth
              type="number"
              value={outrasDividas}
              onChange={(e) => setOutrasDividas(e.target.value)}
              InputProps={{ startAdornment: <InputAdornment position="start">R$</InputAdornment> }}
              helperText="Empréstimos, cartão, etc."
            />
          </Grid>
          <Grid item xs={12} sm={6} md={4}>
            <TextField
              label="Aluguel atual do cliente"
              fullWidth
              type="number"
              value={aluguelAtual}
              onChange={(e) => setAluguelAtual(e.target.value)}
              InputProps={{ startAdornment: <InputAdornment position="start">R$</InputAdornment> }}
              helperText="Para comparativo aluguel x compra"
            />
          </Grid>
        </Grid>

        <Box sx={{ mt: 3, display: 'flex', justifyContent: 'flex-end' }}>
          <Button
            variant="contained"
            size="large"
            startIcon={<CalculateIcon />}
            onClick={() => simular.mutate()}
            disabled={simular.isPending}
          >
            {simular.isPending ? 'Calculando...' : 'Simular'}
          </Button>
        </Box>

        {simular.isError ? (
          <Alert severity="error" sx={{ mt: 2 }}>Erro ao calcular. Verifique os dados.</Alert>
        ) : null}
      </Paper>

      {result ? <ResultadoSimulacao r={result} /> : null}
    </Stack>
  );
}

function ResultadoSimulacao({ r }: { r: SimulacaoCompletaResult }) {
  return (
    <Stack spacing={2}>
      {/* Resumo executivo */}
      <Alert
        severity={r.parcelaCabe === false ? 'warning' : 'info'}
        icon={r.parcelaCabe === true ? <CheckCircleIcon /> : <WarningIcon />}
      >
        <Typography variant="body2" fontWeight={500}>{r.resumoExecutivo}</Typography>
      </Alert>

      <Grid container spacing={2}>
        {/* SFH */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight={700} gutterBottom>
                Financiamento {r.sfh.sistema.includes('SFH') ? 'SFH (Caixa)' : 'SFI'}
              </Typography>
              <KeyValue label="Primeira parcela" value={brl(r.sfh.primeiraParcela)} highlight />
              <KeyValue label="Última parcela" value={brl(r.sfh.ultimaParcela)} />
              <KeyValue label="Valor financiado" value={brl(r.sfh.valorFinanciado)} />
              <KeyValue label="Prazo" value={`${r.sfh.prazoMeses} meses (${r.sfh.prazoMeses / 12} anos)`} />
              <KeyValue label="Taxa anual" value={pct(r.sfh.taxaAnualPct)} />
              <KeyValue label="Juros totais" value={brl(r.sfh.jurosTotais)} />
              <KeyValue label="Custo total" value={brl(r.sfh.custoTotal)} />
              <KeyValue label="CET aproximado" value={pct(r.sfh.cet)} />
            </CardContent>
          </Card>
        </Grid>

        {/* SFI (comparação) */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight={700} gutterBottom>
                SFI (para comparar)
              </Typography>
              <KeyValue label="Primeira parcela" value={brl(r.sfi.primeiraParcela)} />
              <KeyValue label="Última parcela" value={brl(r.sfi.ultimaParcela)} />
              <KeyValue label="Taxa anual" value={pct(r.sfi.taxaAnualPct)} />
              <KeyValue label="Juros totais" value={brl(r.sfi.jurosTotais)} />
              <KeyValue label="Custo total" value={brl(r.sfi.custoTotal)} />
              <Divider sx={{ my: 1.5 }} />
              <Typography variant="caption" color="text.secondary">
                Diferença SFI-SFH: <strong>{brl(r.sfi.jurosTotais - r.sfh.jurosTotais)}</strong> em juros a mais.
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        {/* Impostos de compra */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight={700} gutterBottom>
                Impostos de compra ({r.impostos.uf})
              </Typography>
              <KeyValue label={`ITBI (${pct(r.impostos.itbiPct)})`} value={brl(r.impostos.itbi)} />
              <KeyValue label="Cartório + Registro (~1%)" value={brl(r.impostos.cartorio)} />
              <Divider sx={{ my: 1 }} />
              <KeyValue label="Total adicional" value={brl(r.impostos.total)} highlight />
              <Typography variant="caption" color="text.secondary">
                Equivale a {pct(r.impostos.pctSobreImovel)} do valor do imóvel.
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        {/* Capacidade */}
        {r.capacidade ? (
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="subtitle1" fontWeight={700} gutterBottom>
                  Capacidade de pagamento
                </Typography>
                <KeyValue label="Renda informada" value={brl(r.capacidade.rendaMensal)} />
                {r.capacidade.outrasDividas > 0 ? (
                  <KeyValue label="Dívidas recorrentes" value={brl(r.capacidade.outrasDividas)} />
                ) : null}
                <KeyValue label="Parcela máxima (regra 30%)" value={brl(r.capacidade.parcelaMaxima30Pct)} highlight />
                <KeyValue label="Limite agressivo (35%)" value={brl(r.capacidade.parcelaMaxima35Pct)} />
                <KeyValue label="Imóvel aproximado sustentável (SFH)" value={brl(r.capacidade.imovelAproximadoSFH)} />
                {r.capacidade.alerta ? (
                  <Alert severity="warning" sx={{ mt: 1.5 }}>{r.capacidade.alerta}</Alert>
                ) : null}
                <Chip
                  size="small"
                  sx={{ mt: 1.5 }}
                  color={r.parcelaCabe ? 'success' : 'error'}
                  label={r.parcelaCabe ? 'Parcela cabe no orçamento' : 'Parcela ultrapassa limite de 30%'}
                />
              </CardContent>
            </Card>
          </Grid>
        ) : null}

        {/* Aluguel vs Compra */}
        {r.aluguelVsCompra ? (
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="subtitle1" fontWeight={700} gutterBottom>
                  Aluguel vs Compra ({r.aluguelVsCompra.prazoAnos} anos)
                </Typography>
                <Alert severity="info" variant="outlined" sx={{ mb: 2 }}>
                  {r.aluguelVsCompra.recomendacao} (diferença: {brl(r.aluguelVsCompra.diferencaAbsoluta)})
                </Alert>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={6}>
                    <Typography variant="subtitle2" fontWeight={700} color="primary" gutterBottom>
                      Cenário: COMPRAR
                    </Typography>
                    <KeyValue label="Valor do imóvel hoje" value={brl(r.aluguelVsCompra.valorImovelInicial)} />
                    <KeyValue label="Valor após 30a (valorização)" value={brl(r.aluguelVsCompra.valorImovelFinal)} />
                    <KeyValue label="Gasto total (entrada+parcelas)" value={brl(r.aluguelVsCompra.gastoTotalComprar)} />
                    <KeyValue label="Patrimônio final" value={brl(r.aluguelVsCompra.patrimonioFinalComprar)} />
                    <KeyValue label="Saldo líquido" value={brl(r.aluguelVsCompra.saldoLiquidoComprar)} highlight />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <Typography variant="subtitle2" fontWeight={700} color="secondary" gutterBottom>
                      Cenário: ALUGAR + INVESTIR
                    </Typography>
                    <KeyValue label="Aluguel hoje" value={brl(r.aluguelVsCompra.aluguelInicial)} />
                    <KeyValue label="Aluguel após 30a (IPCA)" value={brl(r.aluguelVsCompra.aluguelFinal)} />
                    <KeyValue label="Gasto total em aluguel" value={brl(r.aluguelVsCompra.gastoTotalAlugar)} />
                    <KeyValue label="Patrimônio investido" value={brl(r.aluguelVsCompra.patrimonioFinalAlugar)} />
                    <KeyValue label="Saldo líquido" value={brl(r.aluguelVsCompra.saldoLiquidoAlugar)} highlight />
                  </Grid>
                </Grid>
                <Typography variant="caption" color="text.secondary" sx={{ mt: 2, display: 'block' }}>
                  Simulação com Selic 10,5% a.a., IPCA 4,5% a.a., valorização imóvel 6% a.a. — cenário base.
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ) : null}

        {/* Parcelamento direto */}
        {r.parcelamentoDireto ? (
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="subtitle1" fontWeight={700} gutterBottom>
                  Parcelamento direto com a incorporadora
                </Typography>
                <KeyValue label="Nº de parcelas" value={`${r.parcelamentoDireto.qtdParcelas}x`} />
                <KeyValue label="Parcela inicial" value={brl(r.parcelamentoDireto.parcelaInicial)} highlight />
                <KeyValue label="Parcela final (corrigida)" value={brl(r.parcelamentoDireto.parcelaFinal)} />
                <KeyValue label={`Reajuste (${pct(r.parcelamentoDireto.taxaReajusteAnualPct)} a.a.)`} value={brl(r.parcelamentoDireto.jurosTotais)} />
                <KeyValue label="Custo total" value={brl(r.parcelamentoDireto.custoTotal)} />
              </CardContent>
            </Card>
          </Grid>
        ) : null}
      </Grid>

      <Typography variant="caption" color="text.secondary" sx={{ mt: 2, textAlign: 'center', display: 'block' }}>
        Simulação indicativa. Valores reais dependem de análise do banco e perfil do comprador.
      </Typography>
    </Stack>
  );
}

function KeyValue({ label, value, highlight }: { label: string; value: string; highlight?: boolean }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 0.5 }}>
      <Typography variant="body2" color="text.secondary">{label}</Typography>
      <Typography variant="body2" fontWeight={highlight ? 700 : 500} color={highlight ? 'primary.main' : 'text.primary'}>
        {value}
      </Typography>
    </Box>
  );
}
