import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#2563eb' },
    secondary: { main: '#0f172a' },
    background: { default: '#f8fafc', paper: '#ffffff' }
  },
  typography: {
    fontFamily: 'system-ui, -apple-system, Segoe UI, Roboto, sans-serif'
  },
  shape: { borderRadius: 10 },
  components: {
    MuiButton: { defaultProps: { variant: 'contained' } }
  }
});
