import { createTheme, type Theme, alpha } from '@mui/material/styles';
import type { ThemeMode } from '@/stores/uiStore';

const fontFamily = '"Inter", "system-ui", -apple-system, "Segoe UI", Roboto, Helvetica, Arial, sans-serif';

const brandHue = {
  primary: '#6366f1',
  primaryHover: '#818cf8',
  primaryDim: '#4f46e5'
};

const darkPalette = {
  bg: '#09090b',
  bgElevated: '#18181b',
  surface: '#111113',
  surfaceElevated: '#18181b',
  surfaceHover: '#1f1f23',
  border: '#27272a',
  borderStrong: '#3f3f46',
  textPrimary: '#fafafa',
  textSecondary: '#a1a1aa',
  textDisabled: '#52525b',
  divider: '#27272a'
};

const lightPalette = {
  bg: '#fafafa',
  bgElevated: '#ffffff',
  surface: '#ffffff',
  surfaceElevated: '#ffffff',
  surfaceHover: '#f4f4f5',
  border: '#e4e4e7',
  borderStrong: '#d4d4d8',
  textPrimary: '#09090b',
  textSecondary: '#52525b',
  textDisabled: '#a1a1aa',
  divider: '#e4e4e7'
};

export function buildTheme(mode: ThemeMode): Theme {
  const isDark = mode === 'dark';
  const p = isDark ? darkPalette : lightPalette;

  return createTheme({
    palette: {
      mode,
      primary: {
        main: brandHue.primary,
        light: brandHue.primaryHover,
        dark: brandHue.primaryDim,
        contrastText: '#ffffff'
      },
      secondary: { main: isDark ? '#e4e4e7' : '#27272a' },
      background: {
        default: p.bg,
        paper: p.surface
      },
      text: {
        primary: p.textPrimary,
        secondary: p.textSecondary,
        disabled: p.textDisabled
      },
      divider: p.divider,
      success: { main: '#10b981' },
      warning: { main: '#f59e0b' },
      error: { main: '#ef4444' },
      info: { main: '#3b82f6' }
    },
    typography: {
      fontFamily,
      fontSize: 14,
      h1: { fontWeight: 700, letterSpacing: '-0.02em' },
      h2: { fontWeight: 700, letterSpacing: '-0.02em' },
      h3: { fontWeight: 700, letterSpacing: '-0.02em' },
      h4: { fontWeight: 700, letterSpacing: '-0.02em' },
      h5: { fontWeight: 600, letterSpacing: '-0.01em' },
      h6: { fontWeight: 600, letterSpacing: '-0.01em' },
      button: { fontWeight: 600, textTransform: 'none', letterSpacing: 0 },
      body2: { fontSize: '0.8125rem' },
      caption: { fontSize: '0.75rem', color: p.textSecondary }
    },
    shape: { borderRadius: 8 },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundColor: p.bg,
            color: p.textPrimary,
            scrollbarColor: `${p.border} transparent`
          },
          '*::-webkit-scrollbar': { width: 10, height: 10 },
          '*::-webkit-scrollbar-track': { background: 'transparent' },
          '*::-webkit-scrollbar-thumb': {
            background: p.border,
            borderRadius: 8
          },
          '*::-webkit-scrollbar-thumb:hover': { background: p.borderStrong }
        }
      },
      MuiPaper: {
        defaultProps: { elevation: 0 },
        styleOverrides: {
          root: {
            backgroundImage: 'none',
            backgroundColor: p.surface,
            border: `1px solid ${p.border}`,
            borderRadius: 10
          }
        }
      },
      MuiCard: {
        defaultProps: { elevation: 0 },
        styleOverrides: {
          root: {
            backgroundImage: 'none',
            backgroundColor: p.surfaceElevated,
            border: `1px solid ${p.border}`,
            borderRadius: 10
          }
        }
      },
      MuiAppBar: {
        defaultProps: { elevation: 0, color: 'transparent' },
        styleOverrides: {
          root: {
            backgroundColor: isDark ? alpha(p.bg, 0.8) : alpha('#ffffff', 0.8),
            backdropFilter: 'blur(8px)',
            borderBottom: `1px solid ${p.border}`,
            color: p.textPrimary
          }
        }
      },
      MuiDrawer: {
        styleOverrides: {
          paper: {
            backgroundColor: p.surface,
            borderRight: `1px solid ${p.border}`,
            backgroundImage: 'none'
          }
        }
      },
      MuiButton: {
        defaultProps: { variant: 'contained', disableElevation: true },
        styleOverrides: {
          root: { borderRadius: 8, paddingInline: 14 },
          containedPrimary: {
            boxShadow: 'none',
            '&:hover': { boxShadow: 'none' }
          },
          outlined: {
            borderColor: p.border,
            color: p.textPrimary,
            '&:hover': {
              borderColor: p.borderStrong,
              backgroundColor: p.surfaceHover
            }
          },
          text: {
            color: p.textPrimary,
            '&:hover': { backgroundColor: p.surfaceHover }
          }
        }
      },
      MuiIconButton: {
        styleOverrides: {
          root: {
            color: p.textSecondary,
            '&:hover': {
              backgroundColor: p.surfaceHover,
              color: p.textPrimary
            }
          }
        }
      },
      MuiTextField: {
        defaultProps: { variant: 'outlined', size: 'small' },
        styleOverrides: {
          root: {
            '& .MuiOutlinedInput-root': {
              backgroundColor: isDark ? p.bg : p.surface,
              '& fieldset': { borderColor: p.border },
              '&:hover fieldset': { borderColor: p.borderStrong },
              '&.Mui-focused fieldset': { borderColor: brandHue.primary, borderWidth: 1 }
            }
          }
        }
      },
      MuiChip: {
        styleOverrides: {
          root: { fontWeight: 500, borderRadius: 6 },
          outlined: { borderColor: p.border }
        }
      },
      MuiTableCell: {
        styleOverrides: {
          root: { borderBottomColor: p.divider },
          head: {
            fontWeight: 600,
            color: p.textSecondary,
            fontSize: '0.75rem',
            textTransform: 'uppercase',
            letterSpacing: '0.04em',
            backgroundColor: isDark ? p.surface : p.bg
          }
        }
      },
      MuiDivider: {
        styleOverrides: { root: { borderColor: p.divider } }
      },
      MuiListItemButton: {
        styleOverrides: {
          root: {
            borderRadius: 8,
            '&.Mui-selected': {
              backgroundColor: isDark ? alpha(brandHue.primary, 0.15) : alpha(brandHue.primary, 0.08),
              color: isDark ? '#ffffff' : brandHue.primaryDim,
              '&:hover': {
                backgroundColor: isDark ? alpha(brandHue.primary, 0.2) : alpha(brandHue.primary, 0.12)
              },
              '& .MuiListItemIcon-root': {
                color: isDark ? '#ffffff' : brandHue.primaryDim
              }
            },
            '&:hover': { backgroundColor: p.surfaceHover }
          }
        }
      },
      MuiListItemIcon: {
        styleOverrides: { root: { color: p.textSecondary, minWidth: 40 } }
      },
      MuiTooltip: {
        styleOverrides: {
          tooltip: {
            backgroundColor: isDark ? p.bgElevated : '#27272a',
            color: isDark ? p.textPrimary : '#fafafa',
            border: `1px solid ${p.border}`,
            fontSize: '0.75rem'
          },
          arrow: { color: isDark ? p.bgElevated : '#27272a' }
        }
      }
    }
  });
}

export const palettes = { dark: darkPalette, light: lightPalette };
