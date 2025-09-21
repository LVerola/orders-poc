import React, { useState } from 'react';
import Header from './components/Header';
import NewOrder from './components/NewOrder';
import OrdersOverview from './components/OrdersOverview';
import OrderDetails from './components/OrderDetails';
import { useOrders } from './hooks/useOrders';
import IAChat from './components/IAChat';

const App: React.FC = () => {
  const [modalOpen, setModalOpen] = useState(false);
  const [aiChatOpen, setAIChatOpen] = useState(false);
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
      <IAChat
        open={aiChatOpen}
        onClose={() => setAIChatOpen(false)}
      />
      <OrderDetails
        open={!!selectedOrderId}
        orderId={selectedOrderId}
        onClose={() => setSelectedOrderId(null)}
      />

      <button
        className="fixed bottom-6 right-6 bg-violet-950 text-white rounded-full w-16 h-16 flex items-center justify-center hover:bg-violet-800 transition z-50"
        onClick={() => setAIChatOpen(true)}
        aria-label="Pergunte à IA"
      >
        <svg width="32" height="32" fill="none" viewBox="0 0 24 24">
          <text x="12" y="17" textAnchor="middle" fontSize="20" fill="white">?</text>
        </svg>
      </button>
    </div>
  );
};

export default App;