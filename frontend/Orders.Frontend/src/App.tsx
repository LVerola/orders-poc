import React, { useState } from 'react';
import Header from './components/Header';
import NewOrder from './components/NewOrder';
import OrdersOverview from './components/OrdersOverview';
import OrderDetails from './components/OrderDetails';
import { useOrders } from './hooks/useOrders';

const App: React.FC = () => {
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedOrderId, setSelectedOrderId] = useState<string | null>(null);

  const { orders } = useOrders();

  return (
    <div className="min-h-screen bg-gray-100 dark:bg-gray-900">
      <Header onNewOrder={() => setModalOpen(true)} />
      <main className="flex justify-center items-center p-4 h-[calc(100vh-64px)]">
        <OrdersOverview
          orders={orders}
          onSelectOrder={setSelectedOrderId}
        />
      </main>
      <NewOrder
        open={modalOpen}
        onClose={() => setModalOpen(false)}
      />
      <OrderDetails
        open={!!selectedOrderId}
        orderId={selectedOrderId}
        onClose={() => setSelectedOrderId(null)}
      />
    </div>
  );
};

export default App;