/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useContext, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
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

type OrderContextType = {
  order: Order | undefined;
  isLoading: boolean;
  error: unknown;
  refetch: () => void;
  setSelectedOrderId: (id: string | null) => void;
};

const OrderContext = createContext<OrderContextType>({} as OrderContextType);

export function OrderProvider({ children }: { children: React.ReactNode }) {
  const [selectedOrderId, setSelectedOrderId] = useState<string | null>(null);

  const { data: order, isLoading, error, refetch } = useQuery({
    queryKey: ['order', selectedOrderId],
    queryFn: async () => {
      const res = await api.get<Order>(`/orders/${selectedOrderId}`);
      return res.data;
    },
    enabled: !!selectedOrderId,
    staleTime: 0,
  });

  return (
    <OrderContext.Provider value={{
      order,
      isLoading,
      error,
      refetch,
      setSelectedOrderId
    }}>
      {children}
    </OrderContext.Provider>
  );
}

export function useOrder() {
  return useContext(OrderContext);
}
