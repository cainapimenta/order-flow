import {
  Alert,
  AppBar,
  Box,
  Button,
  Chip,
  Container,
  Paper,
  Snackbar,
  Stack,
  Toolbar,
  Typography
} from "@mui/material";

import Inventory2OutlinedIcon from "@mui/icons-material/Inventory2Outlined";
import AddRoundedIcon from "@mui/icons-material/AddRounded";
import ScheduleRoundedIcon from "@mui/icons-material/ScheduleRounded";
import AutorenewRoundedIcon from "@mui/icons-material/AutorenewRounded";
import TaskAltRoundedIcon from "@mui/icons-material/TaskAltRounded";

import { useState } from 'react';
import OrdersTable from './components/OrdersTable';
import { useOrders } from './hooks/useOrders';
import CreateOrderDialog from './components/CreateOrderDialog';
import OrderDetailsDialog from './components/OrderDetailsDialog';

const orderStages = [
  {
    status: "Pending",
    title: "Pendente",
    description: "O pedido aguarda o início do processamento.",
    icon: <ScheduleRoundedIcon />,
    color: "#b45309",
    background: "#fef3c7",
  },
  {
    status: "Processing",
    title: "Processando",
    description: "O Worker está processando o pedido.",
    icon: <AutorenewRoundedIcon />,
    color: "#1d4ed8",
    background: "#dbeafe",
  },
  {
    status: "Finished",
    title: "Finalizado",
    description: "O processamento foi concluído.",
    icon: <TaskAltRoundedIcon />,
    color: "#15803d",
    background: "#dcfce7",
  },
];

function App() {
  const { orders, loading, error, refresh } = useOrders();
  const [createOpen, setCreateOpen] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [selectedOrderId, setSelectedOrderId] =
    useState<number | null>(null);

  return (
    <Box
      sx={{
        minHeight: "100vh",
        bgcolor: "background.default",
      }}
    >
      <AppBar
        position="static"
        color="transparent"
        elevation={0}
        sx={{
          bgcolor: "background.paper",
          borderBottom: 1,
          borderColor: "divider",
        }}
      >
        <Container maxWidth="lg">
          <Toolbar
            disableGutters
            sx={{
              minHeight: 72,
              justifyContent: "space-between",
            }}
          >
            <Stack
              direction="row"
              spacing={1.5}
              sx={{ alignItems: 'center' }}
            >
              <Box
                sx={{
                  width: 42,
                  height: 42,
                  borderRadius: 2,
                  bgcolor: "primary.main",
                  color: "primary.contrastText",
                  display: "grid",
                  placeItems: "center",
                }}
              >
                <Inventory2OutlinedIcon />
              </Box>

              <Typography
                variant="h6"
                sx={{ fontWeight: 700 }}
              >
                OrderFlow
              </Typography>
            </Stack>

            <Chip
              label="Ambiente de desenvolvimento"
              size="small"
              variant="outlined"
              sx={{
                display: {
                  xs: "none",
                  sm: "inline-flex",
                },
              }}
            />
          </Toolbar>
        </Container>
      </AppBar>

      <Container
        component="main"
        maxWidth="lg"
        sx={{
          py: { xs: 4, md: 7 },
        }}
      >
        <Stack
          direction={{ xs: 'column', md: 'row' }}
          spacing={3}
          sx={{
            justifyContent: 'space-between',
            alignItems: {
              xs: 'flex-start',
              md: 'center',
            },
            mb: 5,
          }}
        >
          <Box>
            <Typography
              variant="overline"
              color="primary.main"
              sx={{ fontWeight: 700 }}
            >
              PAINEL DE CONTROLE
            </Typography>

            <Typography variant="h4" component="h1" sx={{ mt: 1, mb: 1.5 }}>
              Gerenciamento de Pedidos
            </Typography>

            <Typography color="text.secondary" sx={{ maxWidth: 600 }}>
              Gerencie pedidos e acompanhe cada etapa do processamento em um só
              lugar.
            </Typography>
          </Box>

          <Button
            variant="contained"
            startIcon={<AddRoundedIcon />}
            onClick={() => setCreateOpen(true)}
          >
            Novo pedido
          </Button>
        </Stack>

        <Typography variant="h6" component="h2" sx={{ mb: 2 }}>
          Etapas de um pedido
        </Typography>

        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: {
              xs: "1fr",
              md: "repeat(3, minmax(0, 1fr))",
            },
            gap: 2.5,
            mb: 5,
          }}
        >
          {orderStages.map((stage) => (
            <Paper
              key={stage.status}
              variant="outlined"
              sx={{
                p: 3,
                borderRadius: 3,
              }}
            >
              <Stack
                direction="row"
                spacing={2}
                sx={{ alignItems: 'flex-start' }}
              >
                <Box
                  sx={{
                    width: 48,
                    height: 48,
                    borderRadius: 2,
                    bgcolor: stage.background,
                    color: stage.color,
                    display: "grid",
                    placeItems: "center",
                    flexShrink: 0,
                  }}
                >
                  {stage.icon}
                </Box>

                <Box>
                  <Typography
                    variant="subtitle1"
                    sx={{
                      fontWeight: 700,
                      mb: 0.5,
                    }}
                  >
                    {stage.title}
                  </Typography>

                  <Chip
                    label={stage.status}
                    size="small"
                    variant="outlined"
                    sx={{ mb: 1.5 }}
                  />

                  <Typography variant="body2" color="text.secondary">
                    {stage.description}
                  </Typography>
                </Box>
              </Stack>
            </Paper>
          ))}
        </Box>

        <Typography variant="h6" component="h2" sx={{ mb: 2 }}>
          Pedidos
        </Typography>

        <OrdersTable
          orders={orders}
          loading={loading}
          error={error}
          onRetry={() => {
            void refresh();
          }}
          onViewDetails={(orderId) => {
            setSelectedOrderId(orderId);
          }}
        />
      </Container >

      <CreateOrderDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={(orderId) => {
          setCreateOpen(false);
          setSuccessMessage(
            `Pedido #${orderId} criado com sucesso!`
          );
          void refresh();
        }}
      />

      <Snackbar
        open={Boolean(successMessage)}
        autoHideDuration={4000}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'right',
        }}
        onClose={() => setSuccessMessage('')}
      >
        <Alert
          severity="success"
          variant="filled"
          onClose={() => setSuccessMessage('')}
        >
          {successMessage}
        </Alert>
      </Snackbar>

      {selectedOrderId !== null && (
        <OrderDetailsDialog
          orderId={selectedOrderId}
          onClose={() => {
            setSelectedOrderId(null);
          }}
        />
      )}
    </Box >
  );
}

export default App;
