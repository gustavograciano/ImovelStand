import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Alert, Box, Button, Card, CardContent, Stack, TextField, Typography } from '@mui/material';
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
    <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', bgcolor: 'grey.100' }}>
      <Card sx={{ width: 400 }}>
        <CardContent>
          <Typography variant="h5" fontWeight={700} gutterBottom>
            Entrar no ImovelStand
          </Typography>
          <form onSubmit={handleSubmit(onSubmit)} noValidate>
            <Stack spacing={2}>
              {erro ? <Alert severity="error">{erro}</Alert> : null}
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
              <Button type="submit" size="large" disabled={isSubmitting}>
                {isSubmitting ? 'Entrando...' : 'Entrar'}
              </Button>
            </Stack>
          </form>
        </CardContent>
      </Card>
    </Box>
  );
}
