
import {
    Alert,
    Box,
    Button,
    CircularProgress,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Typography,
} from '@mui/material';
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined';
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded';

import type { Order } from '../types/order';
import OrderStatusChip from './OrderStatusChip';

interface OrdersTableProps {
    orders: Order[];
    loading: boolean;
    error: string | null;
    onRetry: () => void;
    onViewDetails: (orderId: number) => void;
}

const currencyFormatter = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
});

const dateFormatter = new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
});

export default function OrdersTable({
    orders,
    loading,
    error,
    onRetry,
    onViewDetails
}: OrdersTableProps) {
    return (
        <Paper
            variant="outlined"
            sx={{
                borderRadius: 3,
                overflow: 'hidden',
            }}
        >
            {error && (
                <Alert
                    severity="error"
                    sx={{ m: 2 }}
                    action={
                        <Button
                            color="inherit"
                            size="small"
                            onClick={onRetry}
                            startIcon={<RefreshRoundedIcon />}
                        >
                            Tentar novamente
                        </Button>
                    }
                >
                    {error}
                </Alert>
            )}

            {loading ? (
                <Box sx={{ p: 6, textAlign: 'center' }}>
                    <CircularProgress />

                    <Typography
                        color="text.secondary"
                        sx={{ mt: 2 }}
                    >
                        Carregando pedidos...
                    </Typography>
                </Box>
            ) : orders.length === 0 ? (
                !error && (
                    <Box sx={{ p: 6, textAlign: 'center' }}>
                        <Typography variant="h6">
                            Nenhum pedido encontrado
                        </Typography>

                        <Typography
                            color="text.secondary"
                            sx={{ mt: 1 }}
                        >
                            Ainda não existem pedidos cadastrados.
                        </Typography>
                    </Box>
                )
            ) : (
                <TableContainer>
                    <Table
                        size="small"
                        aria-label="Lista de pedidos"
                        sx={{ minWidth: 650 }}
                    >
                        <TableHead>
                            <TableRow>
                                <TableCell>ID</TableCell>
                                <TableCell>Cliente</TableCell>
                                <TableCell>Valor</TableCell>
                                <TableCell>Status</TableCell>
                                <TableCell>Data de criação</TableCell>
                                <TableCell align="right">
                                    Ações
                                </TableCell>
                            </TableRow>
                        </TableHead>

                        <TableBody>
                            {orders.map((order) => (
                                <TableRow key={order.id} hover>
                                    <TableCell>
                                        #{order.id}
                                    </TableCell>

                                    <TableCell>
                                        {order.customer}
                                    </TableCell>

                                    <TableCell>
                                        {currencyFormatter.format(order.amount)}
                                    </TableCell>

                                    <TableCell>
                                        <OrderStatusChip status={order.status} />
                                    </TableCell>

                                    <TableCell>
                                        {dateFormatter.format(
                                            new Date(order.createdAt)
                                        )}
                                    </TableCell>

                                    <TableCell align="right">
                                        <Button
                                            size="small"
                                            variant="text"
                                            startIcon={<VisibilityOutlinedIcon />}
                                            onClick={() => onViewDetails(order.id)}
                                        >
                                            Detalhes
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}
        </Paper>
    );
}
