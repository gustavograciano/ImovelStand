import { Link as RouterLink } from 'react-router-dom';
import { AppBar, Box, Button, Card, CardContent, Container, Grid, Stack, Toolbar, Typography } from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import DashboardIcon from '@mui/icons-material/Dashboard';
import GridViewIcon from '@mui/icons-material/GridView';
import PeopleIcon from '@mui/icons-material/People';
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf';
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong';
import SecurityIcon from '@mui/icons-material/Security';

const features = [
  { icon: <GridViewIcon color="primary" />, title: 'Espelho de vendas visual', desc: 'Grid por torre e pavimento, cores por status. Seu diretor comercial vai amar.' },
  { icon: <PeopleIcon color="primary" />, title: 'CRM com Kanban', desc: 'Do lead à venda: funil visual, timeline de interações, conformidade LGPD.' },
  { icon: <ReceiptLongIcon color="primary" />, title: 'Propostas com state machine', desc: 'Contraproposta versionada, calculadora financeira, expiração automática.' },
  { icon: <PictureAsPdfIcon color="primary" />, title: 'PDF e DOCX sob demanda', desc: 'Espelho executivo e contrato gerados com 1 clique, template customizável.' },
  { icon: <DashboardIcon color="primary" />, title: 'Dashboard + relatórios', desc: 'KPIs em tempo real, funil, ranking de corretores, export Excel.' },
  { icon: <SecurityIcon color="primary" />, title: 'Multi-tenant seguro', desc: 'Isolamento por tenant em todos os dados, BCrypt cost 12, rate limiting.' }
];

const pricing = [
  { name: 'Starter', price: 'R$ 299', features: ['1 empreendimento', '100 unidades', '3 usuários', 'Suporte por email'] },
  { name: 'Pro', price: 'R$ 899', features: ['5 empreendimentos', '500 unidades', '15 usuários', 'WhatsApp + SMTP', 'Webhooks'], highlight: true },
  { name: 'Business', price: 'R$ 2.499', features: ['50 empreendimentos', '10.000 unidades', '100 usuários', 'SLA + onboarding'] }
];

export function LandingPage() {
  return (
    <Box>
      <AppBar position="sticky" color="transparent" elevation={0} sx={{ backdropFilter: 'blur(6px)', bgcolor: 'rgba(255,255,255,0.8)' }}>
        <Toolbar>
          <Typography variant="h6" fontWeight={700} sx={{ flexGrow: 1, color: 'primary.main' }}>
            ImovelStand
          </Typography>
          <Button component={RouterLink} to="/login" variant="text">Entrar</Button>
          <Button component={RouterLink} to="/onboarding" sx={{ ml: 1 }}>
            Começar grátis
          </Button>
        </Toolbar>
      </AppBar>

      <Box sx={{ py: { xs: 6, md: 12 }, bgcolor: 'grey.50' }}>
        <Container maxWidth="lg">
          <Grid container spacing={6} alignItems="center">
            <Grid item xs={12} md={6}>
              <Typography variant="h2" fontWeight={800} sx={{ fontSize: { xs: 32, md: 48 }, lineHeight: 1.15 }}>
                Venda mais imóveis com menos planilha.
              </Typography>
              <Typography variant="h6" color="text.secondary" sx={{ mt: 2, fontWeight: 400 }}>
                O SaaS completo para incorporadoras gerenciarem vendas de empreendimentos de 50 a 500 unidades. Espelho visual, CRM com funil, propostas com state machine, relatórios.
              </Typography>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mt: 4 }}>
                <Button component={RouterLink} to="/onboarding" size="large">
                  Testar grátis por 14 dias
                </Button>
                <Button component={RouterLink} to="/login" variant="outlined" size="large">
                  Já tenho conta
                </Button>
              </Stack>
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 2 }}>
                Sem cartão de crédito. Cancele quando quiser.
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Card sx={{ overflow: 'visible', boxShadow: 6 }}>
                <CardContent>
                  <Typography variant="overline" color="primary">
                    Espelho de vendas · Torre A
                  </Typography>
                  <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 0.5, mt: 2 }}>
                    {Array.from({ length: 24 }).map((_, i) => {
                      const status = i % 4 === 0 ? '#dc2626' : i % 3 === 0 ? '#f59e0b' : '#16a34a';
                      return (
                        <Box
                          key={i}
                          sx={{
                            aspectRatio: '1 / 1',
                            bgcolor: status,
                            borderRadius: 1,
                            display: 'grid',
                            placeItems: 'center',
                            color: 'white',
                            fontSize: 10,
                            fontWeight: 700
                          }}
                        >
                          {String(Math.floor(i / 2) + 1).padStart(2, '0')}{(i % 2) + 1}
                        </Box>
                      );
                    })}
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </Container>
      </Box>

      <Box sx={{ py: { xs: 6, md: 10 } }}>
        <Container maxWidth="lg">
          <Typography variant="h3" fontWeight={800} sx={{ fontSize: { xs: 28, md: 36 } }} textAlign="center" gutterBottom>
            Tudo que sua incorporadora precisa
          </Typography>
          <Typography color="text.secondary" textAlign="center" sx={{ mb: 6 }}>
            Do primeiro lead à chave na mão.
          </Typography>
          <Grid container spacing={3}>
            {features.map((f) => (
              <Grid item xs={12} sm={6} md={4} key={f.title}>
                <Card sx={{ height: '100%' }}>
                  <CardContent>
                    <Box sx={{ mb: 1 }}>{f.icon}</Box>
                    <Typography variant="h6" fontWeight={700} gutterBottom>{f.title}</Typography>
                    <Typography color="text.secondary">{f.desc}</Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Container>
      </Box>

      <Box sx={{ py: { xs: 6, md: 10 }, bgcolor: 'grey.50' }}>
        <Container maxWidth="lg">
          <Typography variant="h3" fontWeight={800} sx={{ fontSize: { xs: 28, md: 36 } }} textAlign="center" gutterBottom>
            Planos
          </Typography>
          <Typography color="text.secondary" textAlign="center" sx={{ mb: 6 }}>
            14 dias grátis em qualquer plano. Pode mudar depois.
          </Typography>
          <Grid container spacing={3} justifyContent="center">
            {pricing.map((p) => (
              <Grid item xs={12} md={4} key={p.name}>
                <Card sx={{ height: '100%', border: p.highlight ? '2px solid' : undefined, borderColor: 'primary.main' }}>
                  <CardContent>
                    {p.highlight ? (
                      <Typography variant="overline" color="primary">Mais popular</Typography>
                    ) : null}
                    <Typography variant="h6" fontWeight={700}>{p.name}</Typography>
                    <Typography variant="h3" fontWeight={800} sx={{ my: 2 }}>
                      {p.price}<Typography component="span" variant="body2" color="text.secondary">/mês</Typography>
                    </Typography>
                    <Stack spacing={1} sx={{ my: 3 }}>
                      {p.features.map((item) => (
                        <Stack key={item} direction="row" spacing={1} alignItems="center">
                          <CheckCircleIcon color="success" fontSize="small" />
                          <Typography variant="body2">{item}</Typography>
                        </Stack>
                      ))}
                    </Stack>
                    <Button
                      component={RouterLink}
                      to="/onboarding"
                      fullWidth
                      variant={p.highlight ? 'contained' : 'outlined'}
                    >
                      Começar com {p.name}
                    </Button>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Container>
      </Box>

      <Box component="footer" sx={{ py: 4, bgcolor: 'grey.900', color: 'grey.300' }}>
        <Container maxWidth="lg">
          <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={2}>
            <Typography variant="body2">© {new Date().getFullYear()} ImovelStand — todos os direitos reservados.</Typography>
            <Stack direction="row" spacing={3}>
              <Typography variant="body2" component="a" href="#" sx={{ color: 'inherit', textDecoration: 'none' }}>Termos</Typography>
              <Typography variant="body2" component="a" href="#" sx={{ color: 'inherit', textDecoration: 'none' }}>Privacidade</Typography>
              <Typography variant="body2" component="a" href="mailto:suporte@imovelstand.com.br" sx={{ color: 'inherit', textDecoration: 'none' }}>Suporte</Typography>
            </Stack>
          </Stack>
        </Container>
      </Box>
    </Box>
  );
}
