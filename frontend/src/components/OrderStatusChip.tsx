
import { Chip } from '@mui/material';
import type { Order } from '../types/order';

type OrderStatus = Order['status'];

interface OrderStatusChipProps {
    status: OrderStatus;
}

const statusConfig = {
    Pending: {
        label: 'Pendente',
        color: 'warning',
    },
    Processing: {
        label: 'Processando',
        color: 'info',
    },
    Finished: {
        label: 'Finalizado',
        color: 'success',
    },
} as const;

export default function OrderStatusChip({
    status,
}: OrderStatusChipProps) {
    const config = statusConfig[status];

    return (
        <Chip
            label={config.label}
            color={config.color}
            size="small"
            variant="outlined"
        />
    );
}
