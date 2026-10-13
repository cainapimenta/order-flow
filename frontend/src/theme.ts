
import { createTheme } from '@mui/material/styles';

const theme = createTheme({
    palette: {
        mode: 'light',

        primary: {
            main: '#2563eb',
            contrastText: '#ffffff',
        },

        background: {
            default: '#f8fafc',
            paper: '#ffffff',
        },

        text: {
            primary: '#0f172a',
            secondary: '#64748b',
        },

        divider: '#e2e8f0',
    },

    typography: {
        fontFamily: '"Inter", "Segoe UI", Arial, sans-serif',

        h4: {
            fontWeight: 700,
        },

        h6: {
            fontWeight: 700,
        },
    },

    shape: {
        borderRadius: 12,
    },

    components: {
        MuiButton: {
            styleOverrides: {
                root: {
                    textTransform: 'none',
                    fontWeight: 600,
                    borderRadius: 10,
                    boxShadow: 'none',
                },
            },
        },

        MuiPaper: {
            styleOverrides: {
                root: {
                    backgroundImage: 'none',
                },
            },
        },
    },
});

export default theme;
