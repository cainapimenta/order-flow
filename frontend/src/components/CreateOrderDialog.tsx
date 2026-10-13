
import { useRef, useState } from 'react';
import type { FormEvent } from 'react';

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
    Paper,
    Stack,
    TextField,
    Typography,
} from '@mui/material';

import AddRoundedIcon from '@mui/icons-material/AddRounded';
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded';

import { createOrder } from '../services/ordersApi';
import type { CreateOrderRequest } from '../types/order';

interface CreateOrderDialogProps {
    open: boolean;
    onClose: () => void;
    onCreated: (orderId: number) => void;
}

interface DraftItem {
    id: number;
    product: string;
    quantity: string;
    unitPrice: string;
}

const currencyFormatter = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
});

function parsePrice(value: string): number {
    const normalized = value.trim().replace(',', '.');

    if (!/^\d+(\.\d{1,2})?$/.test(normalized)) {
        return NaN;
    }

    return Number(normalized);
}

export default function CreateOrderDialog({
    open,
    onClose,
    onCreated,
}: CreateOrderDialogProps) {
    const nextItemId = useRef(2);

    const [customer, setCustomer] = useState('');

    const [items, setItems] = useState<DraftItem[]>([
        {
            id: 1,
            product: '',
            quantity: '1',
            unitPrice: '',
        },
    ]);

    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const total = items.reduce((sum, item) => {
        const quantity = Number(item.quantity);
        const price = parsePrice(item.unitPrice);

        if (!Number.isFinite(quantity) || !Number.isFinite(price)) {
            return sum;
        }

        return sum + quantity * price;
    }, 0);

    function resetForm() {
        setCustomer('');
        setItems([
            {
                id: 1,
                product: '',
                quantity: '1',
                unitPrice: '',
            },
        ]);
        nextItemId.current = 2;
        setError(null);
    }

    function handleClose() {
        if (saving) return;

        resetForm();
        onClose();
    }

    function addItem() {
        const id = nextItemId.current++;

        setItems((current) => [
            ...current,
            {
                id,
                product: '',
                quantity: '1',
                unitPrice: '',
            },
        ]);
    }

    function removeItem(id: number) {
        setItems((current) =>
            current.filter((item) => item.id !== id)
        );
    }

    function updateItem(
        id: number,
        field: 'product' | 'quantity' | 'unitPrice',
        value: string
    ) {
        setItems((current) =>
            current.map((item) =>
                item.id === id
                    ? { ...item, [field]: value }
                    : item
            )
        );
    }

    async function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        if (saving) return;

        setError(null);

        if (!customer.trim()) {
            setError('Informe o nome do cliente.');
            return;
        }

        const validItems = items.every((item) => {
            const quantity = Number(item.quantity);
            const price = parsePrice(item.unitPrice);

            return (
                item.product.trim().length > 0 &&
                /^\d+$/.test(item.quantity) &&
                Number.isSafeInteger(quantity) &&
                quantity > 0 &&
                Number.isFinite(price) &&
                price > 0
            );
        });

        if (!validItems) {
            setError(
                'Preencha todos os produtos com quantidade ' +
                'inteira positiva e preço maior que zero, ' +
                'com até duas casas decimais.'
            );
            return;
        }

        const request: CreateOrderRequest = {
            customer: customer.trim(),
            items: items.map((item) => ({
                product: item.product.trim(),
                quantity: Number(item.quantity),
                unitPrice: parsePrice(item.unitPrice),
            })),
        };

        setSaving(true);

        try {
            const createdOrder = await createOrder(request);

            resetForm();
            onCreated(createdOrder.id);
        } catch {
            setError(
                'Não foi possível criar o pedido. ' +
                'Verifique os dados e se a API está disponível.'
            );
        } finally {
            setSaving(false);
        }
    }

    return (
        <Dialog
            open={open}
            onClose={handleClose}
            fullWidth
            maxWidth="md"
            aria-labelledby="create-order-title"
        >
            <DialogTitle id="create-order-title">
                Novo pedido
            </DialogTitle>

            <Box
                component="form"
                onSubmit={handleSubmit}
            >
                <DialogContent dividers>
                    <Stack spacing={3}>
                        <TextField
                            label="Nome do cliente"
                            value={customer}
                            onChange={(event) =>
                                setCustomer(event.target.value)
                            }
                            required
                            fullWidth
                            autoFocus
                            disabled={saving}
                        />

                        <Divider />

                        <Stack
                            direction="row"
                            sx={{
                                alignItems: 'center',
                                justifyContent: 'space-between',
                            }}
                        >
                            <Typography variant="h6">
                                Produtos
                            </Typography>

                            <Button
                                type="button"
                                startIcon={<AddRoundedIcon />}
                                onClick={addItem}
                                disabled={saving}
                            >
                                Adicionar produto
                            </Button>
                        </Stack>

                        {items.map((item, index) => (
                            <Paper
                                key={item.id}
                                variant="outlined"
                                sx={{ p: 2, borderRadius: 2 }}
                            >
                                <Stack spacing={2}>
                                    <Stack
                                        direction="row"
                                        sx={{
                                            justifyContent: 'space-between',
                                            alignItems: 'center',
                                        }}
                                    >
                                        <Typography sx={{ fontWeight: 600 }}>
                                            Produto {index + 1}
                                        </Typography>

                                        <Button
                                            type="button"
                                            color="error"
                                            size="small"
                                            disabled={saving || items.length === 1}
                                            startIcon={<DeleteOutlineRoundedIcon />}
                                            onClick={() => removeItem(item.id)}
                                        >
                                            Remover
                                        </Button>
                                    </Stack>

                                    <TextField
                                        label="Produto"
                                        value={item.product}
                                        onChange={(event) =>
                                            updateItem(
                                                item.id,
                                                'product',
                                                event.target.value
                                            )
                                        }
                                        required
                                        fullWidth
                                        disabled={saving}
                                    />

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
                                        <TextField
                                            label="Quantidade"
                                            value={item.quantity}
                                            onChange={(event) =>
                                                updateItem(
                                                    item.id,
                                                    'quantity',
                                                    event.target.value
                                                )
                                            }
                                            slotProps={{
                                                htmlInput: { inputMode: 'numeric' },
                                            }}
                                            required
                                            disabled={saving}
                                        />

                                        <TextField
                                            label="Preço unitário (R$)"
                                            value={item.unitPrice}
                                            onChange={(event) =>
                                                updateItem(
                                                    item.id,
                                                    'unitPrice',
                                                    event.target.value
                                                )
                                            }
                                            placeholder="Ex.: 199,90"
                                            slotProps={{
                                                htmlInput: { inputMode: 'decimal' },
                                            }}
                                            required
                                            disabled={saving}
                                        />
                                    </Box>
                                </Stack>
                            </Paper>
                        ))}

                        <Divider />

                        <Stack
                            direction="row"
                            sx={{
                                justifyContent: 'space-between',
                                alignItems: 'center',
                            }}
                        >
                            <Typography color="text.secondary">
                                Total estimado
                            </Typography>

                            <Typography variant="h5" color="primary.main">
                                {currencyFormatter.format(total)}
                            </Typography>
                        </Stack>

                        {error && (
                            <Alert severity="error">
                                {error}
                            </Alert>
                        )}
                    </Stack>
                </DialogContent>

                <DialogActions sx={{ p: 2 }}>
                    <Button
                        type="button"
                        onClick={handleClose}
                        disabled={saving}
                    >
                        Cancelar
                    </Button>

                    <Button
                        type="submit"
                        variant="contained"
                        disabled={saving}
                        startIcon={
                            saving
                                ? <CircularProgress size={18} color="inherit" />
                                : undefined
                        }
                    >
                        {saving ? 'Salvando...' : 'Criar pedido'}
                    </Button>
                </DialogActions>
            </Box>
        </Dialog>
    );
}
