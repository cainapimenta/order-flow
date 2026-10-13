
import {
    useCallback,
    useEffect,
    useRef,
    useState,
} from 'react';

import { getOrders } from '../services/ordersApi';
import type { Order } from '../types/order';

const REFRESH_INTERVAL = 3000;

export function useOrders() {
    const [orders, setOrders] = useState<Order[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const requestRef = useRef<AbortController | null>(null);

    const loadOrders = useCallback(async () => {
        // Evita executar duas consultas simultâneas.
        if (requestRef.current) {
            return;
        }

        const controller = new AbortController();
        requestRef.current = controller;

        try {
            const data = await getOrders(controller.signal);

            if (!controller.signal.aborted) {
                setOrders(data);
                setError(null);
            }
        } catch {
            if (!controller.signal.aborted) {
                setError(
                    'Não foi possível atualizar os pedidos. ' +
                    'Verifique se a API está disponível.'
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
    }, []);

    useEffect(() => {
        // Agenda a primeira consulta após a configuração do efeito.
        const timeoutId = window.setTimeout(() => {
            void loadOrders();
        }, 0);

        // Atualiza os pedidos a cada três segundos.
        const intervalId = window.setInterval(() => {
            void loadOrders();
        }, REFRESH_INTERVAL);

        // Cancela timers e requisições ao desmontar.
        return () => {
            window.clearTimeout(timeoutId);
            window.clearInterval(intervalId);
            requestRef.current?.abort();
            requestRef.current = null;
        };
    }, [loadOrders]);

    return {
        orders,
        loading,
        error,
        refresh: loadOrders,
    };
}
