
import {
    useCallback,
    useEffect,
    useRef,
    useState,
} from 'react';

import {
    Alert,
    Box,
    Button,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Divider,
    Stack,
    Step,
    StepLabel,
    Stepper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Typography,
} from '@mui/material';

import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded';

import { getOrderById } from '../services/ordersApi';
import type { OrderDetails } from '../types/order';
import OrderStatusChip from './OrderStatusChip';

interface OrderDetailsDialogProps {
    orderId: number;
    onClose: () => void;
}

const REFRESH_INTERVAL = 3000;

const currencyFormatter = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
});

const dateFormatter = new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'medium',
});

const statusLabels: Record<OrderDetails['status'], string> = {
    Pending: 'Pendente',
    Processing: 'Processando',
    Finished: 'Finalizado',
};

export default function OrderDetailsDialog({
    orderId,
    onClose,
}: OrderDetailsDialogProps) {
    const [order, setOrder] = useState<OrderDetails | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const requestRef = useRef<AbortController | null>(null);

    const loadOrder = useCallback(async () => {
        // Evita consultas simultâneas ao mesmo pedido.
        if (requestRef.current) {
            return;
        }

        const controller = new AbortController();
        requestRef.current = controller;

        try {
            const data = await getOrderById(
                orderId,
                controller.signal
            );

            if (!controller.signal.aborted) {
                setOrder(data);
                setError(null);
            }
        } catch (err: unknown) {
            if (!controller.signal.aborted) {
                setError(
                    err instanceof Error &&
                        err.message === 'Pedido não encontrado.'
                        ? err.message
                        : 'Não foi possível carregar os detalhes do pedido.'
                );
            }
        } finally {
            if (requestRef.current === controller) {
                requestRef.current = null;

                if (!controller.signal.aborted) {
                    setLoading(false);
                }
            }
        }
    }, [orderId]);

    useEffect(() => {
        // Agenda a primeira consulta.
        const timeoutId = window.setTimeout(() => {
            void loadOrder();
        }, 0);

        // Mantém o histórico atualizado enquanto a janela está aberta.
        const intervalId = window.setInterval(() => {
            void loadOrder();
        }, REFRESH_INTERVAL);

        return () => {
            window.clearTimeout(timeoutId);
            window.clearInterval(intervalId);

            requestRef.current?.abort();
            requestRef.current = null;
        };
    }, [loadOrder]);

    const history = order
        ? [...order.statusHistory].sort(
            (a, b) =>
                new Date(a.changedAt).getTime() -
                new Date(b.changedAt).getTime()
        )
        : [];

    return (
        <Dialog
            open
            onClose={onClose}
            fullWidth
            maxWidth="md"
            aria-labelledby="order-details-title"
        >
            <DialogTitle id="order-details-title">
                <Stack
                    direction="row"
                    spacing={2}
                    sx={{
                        justifyContent: 'space-between',
                        alignItems: 'center',
                    }}
                >
                    <Typography
                        variant="h6"
                        component="span"
                        sx={{ fontWeight: 700 }}
                    >
                        Pedido #{orderId}
                    </Typography>

                    {order && (
                        <OrderStatusChip status={order.status} />
                    )}
                </Stack>
            </DialogTitle>

            <DialogContent dividers>
                {loading && !order && (
                    <Box sx={{ py: 6, textAlign: 'center' }}>
                        <CircularProgress />

                        <Typography
                            color="text.secondary"
                            sx={{ mt: 2 }}
                        >
                            Carregando detalhes...
                        </Typography>
                    </Box>
                )}

                {error && (
                    <Alert
                        severity="error"
                        sx={{ mb: 2 }}
                        action={
                            <Button
                                color="inherit"
                                size="small"
                                startIcon={<RefreshRoundedIcon />}
                                onClick={() => void loadOrder()}
                            >
                                Tentar novamente
                            </Button>
                        }
                    >
                        {error}
                    </Alert>
                )}

                {order && (
                    <Stack spacing={3}>
                        <Box>
                            <Typography
                                variant="subtitle1"
                                sx={{ fontWeight: 700, mb: 2 }}
                            >
                                Informações do pedido
                            </Typography>

                            <Box
                                sx={{
                                    display: 'grid',
                                    gridTemplateColumns: {
                                        xs: '1fr',
                                        sm: '1fr 1fr',
                                    },
                                    gap: 2,
                                }}
                            >
                                <Box>
                                    <Typography
                                        variant="caption"
                                        color="text.secondary"
                                    >
                                        Cliente
                                    </Typography>

                                    <Typography>
                                        {order.customer}
                                    </Typography>
                                </Box>

                                <Box>
                                    <Typography
                                        variant="caption"
                                        color="text.secondary"
                                    >
                                        Criado em
                                    </Typography>

                                    <Typography>
                                        {dateFormatter.format(
                                            new Date(order.createdAt)
                                        )}
                                    </Typography>
                                </Box>
                            </Box>
                        </Box>

                        <Divider />

                        <Box>
                            <Typography
                                variant="subtitle1"
                                sx={{ fontWeight: 700, mb: 2 }}
                            >
                                Produtos
                            </Typography>

                            <TableContainer sx={{ overflowX: 'auto' }}>
                                <Table
                                    size="small"
                                    aria-label="Produtos do pedido"
                                    sx={{ minWidth: 520 }}
                                >
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>Produto</TableCell>
                                            <TableCell align="right">
                                                Qtd.
                                            </TableCell>
                                            <TableCell align="right">
                                                Preço unitário
                                            </TableCell>
                                            <TableCell align="right">
                                                Subtotal
                                            </TableCell>
                                        </TableRow>
                                    </TableHead>

                                    <TableBody>
                                        {order.items.map((item) => (
                                            <TableRow key={item.id}>
                                                <TableCell>
                                                    {item.product}
                                                </TableCell>

                                                <TableCell align="right">
                                                    {item.quantity}
                                                </TableCell>

                                                <TableCell align="right">
                                                    {currencyFormatter.format(
                                                        item.unitPrice
                                                    )}
                                                </TableCell>

                                                <TableCell align="right">
                                                    {currencyFormatter.format(
                                                        item.subtotal
                                                    )}
                                                </TableCell>
                                            </TableRow>
                                        ))}

                                        <TableRow>
                                            <TableCell
                                                colSpan={3}
                                                align="right"
                                                sx={{ fontWeight: 700 }}
                                            >
                                                Total
                                            </TableCell>

                                            <TableCell
                                                align="right"
                                                sx={{ fontWeight: 700 }}
                                            >
                                                {currencyFormatter.format(
                                                    order.amount
                                                )}
                                            </TableCell>
                                        </TableRow>
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        </Box>

                        <Divider />

                        <Box>
                            <Typography
                                variant="subtitle1"
                                sx={{ fontWeight: 700, mb: 2 }}
                            >
                                Histórico de processamento
                            </Typography>

                            {history.length === 0 ? (
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                >
                                    Este pedido não possui registros
                                    de histórico disponíveis.
                                </Typography>
                            ) : (
                                <Stepper
                                    orientation="vertical"
                                    activeStep={history.length - 1}
                                >
                                    {history.map((entry, index) => (
                                        <Step
                                            key={`${entry.status}-${entry.changedAt}-${index}`}
                                            completed={
                                                index < history.length - 1
                                            }
                                        >
                                            <StepLabel>
                                                <Box>
                                                    <Typography
                                                        variant="body2"
                                                        sx={{
                                                            fontWeight:
                                                                index === history.length - 1
                                                                    ? 700
                                                                    : 500,
                                                        }}
                                                    >
                                                        {statusLabels[entry.status]}
                                                    </Typography>

                                                    <Typography
                                                        variant="caption"
                                                        color="text.secondary"
                                                    >
                                                        {dateFormatter.format(
                                                            new Date(entry.changedAt)
                                                        )}
                                                    </Typography>
                                                </Box>
                                            </StepLabel>
                                        </Step>
                                    ))}
                                </Stepper>
                            )}
                        </Box>
                    </Stack>
                )}
            </DialogContent>

            <DialogActions sx={{ p: 2 }}>
                <Button onClick={onClose}>
                    Fechar
                </Button>
            </DialogActions>
        </Dialog>
    );
}
