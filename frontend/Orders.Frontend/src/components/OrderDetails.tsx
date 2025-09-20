import React from 'react';

type Order = {
	id: string;
	cliente: string;
	produto: string;
	valor: number;
	status: 'Pendente' | 'Processando' | 'Finalizado';
	dataCriacao: string;
};

type OrderDetailsProps = {
	open: boolean;
	onClose: () => void;
	order: Order | null;
};

const statusColors: Record<string, string> = {
	Pendente: 'bg-yellow-400',
	Processando: 'bg-blue-500',
	Finalizado: 'bg-green-500',
};

const OrderDetails: React.FC<OrderDetailsProps> = ({ open, onClose, order }) => {
	if (!open || !order) return null;

	return (
		<div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
			<div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-8 w-full max-w-md relative">
				<button
					className="absolute top-2 right-2 text-gray-500 hover:text-gray-700"
					onClick={onClose}
				>
					&times;
				</button>
				<h2 className="text-xl font-bold mb-4 text-gray-900 dark:text-white">Detalhes da Ordem</h2>
				<div className="flex items-center gap-2 mb-2">
					<span className={`w-3 h-3 rounded-full ${statusColors[order.status]}`}></span>
					<span className="font-semibold text-gray-700 dark:text-gray-200">{order.status}</span>
				</div>
				<div className="mb-2">
					<span className="text-sm text-gray-500">ID: {order.id}</span>
				</div>
				<div className="mb-2">
					<span className="block text-gray-700 dark:text-gray-300">Cliente: <b>{order.cliente}</b></span>
				</div>
				<div className="mb-2">
					<span className="block text-gray-700 dark:text-gray-300">Produto: <b>{order.produto}</b></span>
				</div>
				<div className="mb-2">
					<span className="block text-gray-700 dark:text-gray-300">Valor: <b>R$ {order.valor.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}</b></span>
				</div>
				<div className="mb-2">
					<span className="block text-xs text-gray-400">Criado em: {new Date(order.dataCriacao).toLocaleString('pt-BR')}</span>
				</div>
			</div>
		</div>
	);
};

export default OrderDetails;
