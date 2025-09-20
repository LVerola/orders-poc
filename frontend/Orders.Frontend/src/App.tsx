import React, { useState } from 'react';
import Header from './components/Header';
import NewOrder from './components/NewOrder';
import OrdersOverview from './components/OrdersOverview';
import OrderDetails from './components/OrderDetails';

// Tipo da ordem
type Order = {
  id: string;
  cliente: string;
  produto: string;
  valor: number;
  status: 'Pendente' | 'Processando' | 'Finalizado';
  dataCriacao: string;
};

const mockOrders: Order[] = [
  {
    id: 'a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21',
    cliente: 'João da Silva',
    produto: 'Notebook',
    valor: 4500.00,
    status: 'Finalizado',
    dataCriacao: '2025-09-18T14:22:00Z',
  },
  // Adicione mais ordens mock se quiser
];

const App: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>(mockOrders);
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);

  // Função para criar nova ordem
  const handleCreateOrder = (order: Omit<Order, 'id' | 'status' | 'dataCriacao'>) => {
    const newOrder: Order = {
      id: crypto.randomUUID(),
      status: 'Pendente',
      dataCriacao: new Date().toISOString(),
      ...order,
    };
    setOrders([newOrder, ...orders]);
    setModalOpen(false);
  };

  return (
    <div className="min-h-screen bg-gray-100 dark:bg-gray-900">
      <Header onNewOrder={() => setModalOpen(true)} />
      <main className="flex justify-center items-center p-4 h-[calc(100vh-64px)]">
        <OrdersOverview
          orders={orders}
          onSelectOrder={setSelectedOrder}
        />
      </main>
      <NewOrder
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onCreate={handleCreateOrder}
      />
      <OrderDetails
        open={!!selectedOrder}
        order={selectedOrder}
        onClose={() => setSelectedOrder(null)}
      />
    </div>
  );
};

export default App;