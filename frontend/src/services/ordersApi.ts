import type { CreateOrderRequest, Order, OrderDetails } from '../types/order';

const apiUrl = import.meta.env.VITE_API_URL;

// Usa a mesma apiUrl já declarada neste arquivo.
export async function createOrder(
    request: CreateOrderRequest
): Promise<Order> {
    if (!apiUrl) {
        throw new Error('API URL is not configured.');
    }

    const response = await fetch(`${apiUrl}/orders`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        throw new Error(
            `Failed to create order. Status: ${response.status}`
        );
    }

    return (await response.json()) as Order;
}

export async function getOrders(
    signal?: AbortSignal
): Promise<Order[]> {
    if (!apiUrl) {
        throw new Error('API URL is not configured.');
    }

    const response = await fetch(`${apiUrl}/orders`, {
        signal,
    });

    if (!response.ok) {
        throw new Error(
            `Failed to fetch orders. Status: ${response.status}`
        );
    }

    return (await response.json()) as Order[];
}


/// Busca as informações completas de um pedido.
export async function getOrderById(
    id: number,
    signal?: AbortSignal
): Promise<OrderDetails> {
    if (!apiUrl) {
        throw new Error('API URL is not configured.');
    }

    const response = await fetch(`${apiUrl}/orders/${id}`, {
        signal,
    });

    if (response.status === 404) {
        throw new Error('Pedido não encontrado.');
    }

    if (!response.ok) {
        throw new Error(
            `Failed to fetch order. Status: ${response.status}`
        );
    }

    return (await response.json()) as OrderDetails;
}

