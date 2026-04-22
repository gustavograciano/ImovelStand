import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  FormControlLabel,
  Stack,
  Step,
  StepLabel,
  Stepper,
  TextField,
  Typography
} from '@mui/material';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { api } from '@/services/api';
import { authService } from '@/services/authService';

const steps = ['Empresa', 'Administrador', 'Começar'];

const schema = z.object({
  nomeEmpresa: z.string().min(3, 'Nome muito curto'),
  cnpj: z.string().optional(),
  adminNome: z.string().min(3, 'Nome muito curto'),
  adminEmail: z.string().email('Email inválido'),
  adminSenha: z
    .string()
    .min(8, 'Mínimo 8 caracteres')
    .regex(/[A-Z]/, '1 maiúscula')
    .regex(/[a-z]/, '1 minúscula')
    .regex(/[0-9]/, '1 dígito')
    .regex(/[^A-Za-z0-9]/, '1 especial'),
  criarEmpreendimentoDemo: z.boolean()
});

type FormData = z.infer<typeof schema>;

export function OnboardingPage() {
  const navigate = useNavigate();
  const [step, setStep] = useState(0);
  const [serverError, setServerError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const {
    register,
    trigger,
    handleSubmit,
    watch,
    formState: { errors }
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { criarEmpreendimentoDemo: true }
  });

  const handleNext = async () => {
    const fields: Array<keyof FormData> =
      step === 0 ? ['nomeEmpresa', 'cnpj']
      : step === 1 ? ['adminNome', 'adminEmail', 'adminSenha']
      : [];
    const ok = await trigger(fields);
    if (ok) setStep(step + 1);
  };

  const onSubmit = async (values: FormData) => {
    setServerError(null);
    setSubmitting(true);
    try {
      await api.post('/onboarding/start', values);
      await authService.login(values.adminEmail, values.adminSenha);
      navigate('/', { replace: true });
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? 'Não foi possível concluir o onboarding. Verifique os dados.';
      setServerError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', bgcolor: 'grey.100', p: 2 }}>
      <Card sx={{ width: '100%', maxWidth: 600 }}>
        <CardContent sx={{ p: 4 }}>
          <Typography variant="h5" fontWeight={700} gutterBottom>
            Comece agora — trial grátis de 14 dias
          </Typography>
          <Typography color="text.secondary" paragraph>
            Sem cartão. Configure sua imobiliária em 2 minutos.
          </Typography>

          <Stepper activeStep={step} sx={{ my: 3 }}>
            {steps.map((label) => (
              <Step key={label}><StepLabel>{label}</StepLabel></Step>
            ))}
          </Stepper>

          <form onSubmit={handleSubmit(onSubmit)}>
            <Stack spacing={2}>
              {step === 0 ? (
                <>
                  <TextField
                    label="Nome da empresa"
                    fullWidth
                    {...register('nomeEmpresa')}
                    error={!!errors.nomeEmpresa}
                    helperText={errors.nomeEmpresa?.message}
                  />
                  <TextField
                    label="CNPJ (opcional)"
                    fullWidth
                    placeholder="00.000.000/0001-00"
                    {...register('cnpj')}
                  />
                </>
              ) : null}

              {step === 1 ? (
                <>
                  <TextField
                    label="Seu nome"
                    fullWidth
                    {...register('adminNome')}
                    error={!!errors.adminNome}
                    helperText={errors.adminNome?.message}
                  />
                  <TextField
                    label="Email de login"
                    type="email"
                    fullWidth
                    {...register('adminEmail')}
                    error={!!errors.adminEmail}
                    helperText={errors.adminEmail?.message}
                  />
                  <TextField
                    label="Senha"
                    type="password"
                    fullWidth
                    {...register('adminSenha')}
                    error={!!errors.adminSenha}
                    helperText={errors.adminSenha?.message ?? 'Mínimo 8 chars, com maiúscula/minúscula/dígito/especial'}
                  />
                </>
              ) : null}

              {step === 2 ? (
                <>
                  <Typography>
                    Vamos criar sua conta como <b>{watch('nomeEmpresa')}</b> e já deixar um empreendimento demo com 48 unidades pronto pra você testar.
                  </Typography>
                  <FormControlLabel
                    control={<Checkbox defaultChecked {...register('criarEmpreendimentoDemo')} />}
                    label="Criar empreendimento demo (48 unidades)"
                  />
                  {serverError ? <Alert severity="error">{serverError}</Alert> : null}
                </>
              ) : null}

              <Stack direction="row" justifyContent="space-between" sx={{ pt: 1 }}>
                <Button
                  variant="text"
                  onClick={() => setStep(Math.max(0, step - 1))}
                  disabled={step === 0 || submitting}
                >
                  Voltar
                </Button>
                {step < steps.length - 1 ? (
                  <Button onClick={handleNext}>Próximo</Button>
                ) : (
                  <Button type="submit" disabled={submitting}>
                    {submitting ? 'Criando conta...' : 'Criar conta e começar'}
                  </Button>
                )}
              </Stack>
            </Stack>
          </form>
        </CardContent>
      </Card>
    </Box>
  );
}
