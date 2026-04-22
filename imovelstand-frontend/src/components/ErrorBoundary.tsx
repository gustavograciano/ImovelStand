import { Alert, Box, Button, Stack, Typography } from '@mui/material';
import { Component, type ReactNode } from 'react';

interface Props { children: ReactNode; }
interface State { error: Error | null; }

export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: React.ErrorInfo) {
    // Próxima sprint: enviar para Sentry.captureException(error, { extra: info })
    console.error('ErrorBoundary capturou:', error, info);
  }

  render() {
    if (this.state.error) {
      return (
        <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', p: 4 }}>
          <Stack spacing={2} sx={{ maxWidth: 600 }}>
            <Typography variant="h5" fontWeight={700}>
              Algo deu errado
            </Typography>
            <Alert severity="error">{this.state.error.message}</Alert>
            <Typography variant="body2" color="text.secondary">
              A equipe foi notificada. Você pode recarregar a página ou tentar novamente.
            </Typography>
            <Button
              onClick={() => {
                this.setState({ error: null });
                window.location.reload();
              }}
            >
              Recarregar
            </Button>
          </Stack>
        </Box>
      );
    }
    return this.props.children;
  }
}
