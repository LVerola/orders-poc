import React, { createContext, useContext, useEffect, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import connection from '../services/signalr';
import api from '../services/api';

export type Status = 'Pendente' | 'Processando' | 'Finalizado';

export type StatusHistory = {
  id: string;
  orderId: string;
  status: Status;
  dataAlteracao: string;
};

export type Order = {
  id: string;
  cliente: string;
  produto: string;
  valor: number;
  status: Status;
  dataCriacao: string;
  statusHistories?: StatusHistory[];
};

type NewOrder = {
  cliente: string;
  produto: string;
  valor: number;
};

type OrdersContextType = {
  orders: Order[] | undefined;
  isLoading: boolean;
  refetchOrders: () => void;
  createOrder: (order: NewOrder) => Promise<Order>;
  selectedOrderId: string | null;
  setSelectedOrderId: (id: string | null) => void;
};

const OrdersContext = createContext<OrdersContextType>({} as OrdersContextType);

export function OrdersProvider({ children }: { children: React.ReactNode }) {
  const [selectedOrderId, setSelectedOrderId] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const { data: orders, isLoading, refetch } = useQuery({
    queryKey: ['orders'],
    queryFn: async () => {
      const res = await api.get<Order[]>('/orders');
      return res.data;
    },
  });

  const mutation = useMutation({
    mutationFn: async (order: NewOrder) => {
      const res = await api.post<Order>('/orders', order);
      return res.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
  });

  useEffect(() => {
  if (connection.state === "Disconnected") {
    connection.start()
      .then(() => console.log("SignalR conectado!"))
      .catch(err => console.error("Erro ao conectar SignalR:", err));
  }

  const handleOrderUpdated = (order: Order) => {
    refetch();
    queryClient.invalidateQueries({ queryKey: ['orders'] });
    queryClient.invalidateQueries({ queryKey: ['order', order.id] });
  };

  connection.on("OrderUpdated", handleOrderUpdated);

  return () => {
    connection.off("OrderUpdated", handleOrderUpdated);
  };
}, [queryClient, refetch]);

  return (
    <OrdersContext.Provider
      value={{
        orders,
        isLoading,
        refetchOrders: refetch,
        createOrder: mutation.mutateAsync,
        selectedOrderId,
        setSelectedOrderId,
      }}
    >
      {children}
    </OrdersContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useOrders() {
  return useContext(OrdersContext);
}