import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './App.tsx';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { OrderProvider } from './hooks/useOrder.tsx';
import { OrdersProvider } from './hooks/useOrders.tsx';

const queryClient = new QueryClient();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <OrdersProvider>
        <OrderProvider>
          <App />
        </OrderProvider>
      </OrdersProvider>
    </QueryClientProvider>
  </StrictMode>
);