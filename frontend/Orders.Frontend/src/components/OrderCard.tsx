import React from 'react';

type OrderCardProps = {
  id: string;
  cliente: string;
  produto: string;
  valor: number;
  status: 'Pendente' | 'Processando' | 'Finalizado';
  dataCriacao: string;
};

const statusColors: Record<string, string> = {
  Pendente: 'bg-yellow-400',
  Processando: 'bg-blue-500',
  Finalizado: 'bg-green-500',
};

const OrderCard: React.FC<OrderCardProps> = ({ id, cliente, produto, valor, status, dataCriacao }) => {
  return (
    <div className="bg-white dark:bg-gray-700 rounded-lg shadow p-6 flex flex-col gap-2 w-full max-w-md mx-auto">
      <div className="flex items-center justify-between">
        <span className="text-sm text-gray-500">ID: {id}</span>
        <span className="flex items-center gap-2">
          <span className={`w-3 h-3 rounded-full ${statusColors[status]}`}></span>
          <span className="font-semibold text-gray-700 dark:text-gray-200">{status}</span>
        </span>
      </div>
      <div className="mt-2">
        <h3 className="text-lg font-bold text-gray-900 dark:text-white">{produto}</h3>
        <p className="text-gray-600 dark:text-gray-300">Cliente: {cliente}</p>
        <p className="text-gray-600 dark:text-gray-300">Valor: R$ {valor.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}</p>
        <p className="text-xs text-gray-400 mt-2">Criado em: {new Date(dataCriacao).toLocaleString('pt-BR')}</p>
      </div>
    </div>
  );
};

export default OrderCard;