import { useState } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { Alert, Box, Button, Card, CardContent, Link, Stack, TextField, Typography } from '@mui/material';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { authService } from '@/services/authService';

const schema = z.object({
  email: z.string().email('Email inválido'),
  senha: z.string().min(1, 'Senha obrigatória')
});

type LoginFields = z.infer<typeof schema>;

export function LoginPage() {
  const navigate = useNavigate();
  const [erro, setErro] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<LoginFields>({ resolver: zodResolver(schema) });

  const onSubmit = async (values: LoginFields) => {
    setErro(null);
    try {
      await authService.login(values.email, values.senha);
      navigate('/', { replace: true });
    } catch {
      setErro('Email ou senha inválidos.');
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'grid',
        placeItems: 'center',
        bgcolor: 'background.default',
        backgroundImage: (t) =>
          t.palette.mode === 'dark'
            ? 'radial-gradient(ellipse at top, rgba(99,102,241,0.12), transparent 60%)'
            : 'radial-gradient(ellipse at top, rgba(99,102,241,0.08), transparent 60%)',
        px: 2
      }}
    >
      <Card sx={{ width: '100%', maxWidth: 420 }}>
        <CardContent sx={{ p: 4 }}>
          <Stack direction="row" spacing={1.25} alignItems="center" sx={{ mb: 3 }}>
            <Box
              sx={{
                width: 32,
                height: 32,
                borderRadius: 1.5,
                background: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%)',
                display: 'grid',
                placeItems: 'center',
                color: '#fff',
                fontWeight: 800,
                fontSize: 16
              }}
            >
              I
            </Box>
            <Typography variant="h6" fontWeight={700}>
              ImovelStand
            </Typography>
          </Stack>

          <Typography variant="h5" fontWeight={700} sx={{ letterSpacing: '-0.02em' }}>
            Entrar
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5, mb: 3 }}>
            Bem-vindo de volta. Use suas credenciais para continuar.
          </Typography>

          <form onSubmit={handleSubmit(onSubmit)} noValidate>
            <Stack spacing={2}>
              {erro ? <Alert severity="error" variant="outlined">{erro}</Alert> : null}
              <TextField
                label="Email"
                type="email"
                autoComplete="email"
                fullWidth
                {...register('email')}
                error={!!errors.email}
                helperText={errors.email?.message}
              />
              <TextField
                label="Senha"
                type="password"
                autoComplete="current-password"
                fullWidth
                {...register('senha')}
                error={!!errors.senha}
                helperText={errors.senha?.message}
              />
              <Button type="submit" size="large" disabled={isSubmitting} fullWidth>
                {isSubmitting ? 'Entrando...' : 'Entrar'}
              </Button>
              <Typography variant="body2" color="text.secondary" textAlign="center" sx={{ mt: 0.5 }}>
                Ainda não tem conta?{' '}
                <Link component={RouterLink} to="/onboarding" fontWeight={600} underline="hover">
                  Criar trial grátis
                </Link>
              </Typography>
            </Stack>
          </form>
        </CardContent>
      </Card>
    </Box>
  );
}
