export interface Order {
    id: number;
    customer: string;
    amount: number;
    status: 'Pending' | 'Processing' | 'Finished';
    createdAt: string;
}

export interface CreateOrderItemRequest {
    product: string;
    quantity: number;
    unitPrice: number;
}

export interface CreateOrderRequest {
    customer: string;
    items: CreateOrderItemRequest[];
}

export interface OrderItem {
    id: number;
    orderId: number;
    product: string;
    quantity: number;
    unitPrice: number;
    subtotal: number;
}

export interface OrderStatusHistory {
    status: Order['status'];
    changedAt: string;
}

export interface OrderDetails extends Order {
    items: OrderItem[];
    statusHistory: OrderStatusHistory[];
}