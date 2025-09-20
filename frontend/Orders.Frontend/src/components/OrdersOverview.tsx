import React from 'react';
import OrderCard from './OrderCard';

type Order = {
	id: string;
	cliente: string;
	produto: string;
	valor: number;
	status: 'Pendente' | 'Processando' | 'Finalizado';
	dataCriacao: string;
};

type OrdersOverviewProps = {
	orders: Order[];
	onSelectOrder: (order: Order) => void;
};

const OrdersOverview: React.FC<OrdersOverviewProps> = ({ orders, onSelectOrder }) => {
	return (
		<div className="bg-white dark:bg-gray-800 rounded-xl shadow-lg p-6 w-full max-w-5xl h-full flex flex-col" style={{ minHeight: '70vh' }}>
			<h2 className="text-2xl font-bold mb-4 text-gray-900 dark:text-white">Ordens</h2>
			<div className="flex-1 overflow-y-auto flex flex-col gap-4">
				{orders.length === 0 ? (
					<p className="text-gray-500">Nenhuma ordem cadastrada.</p>
				) : (
					orders.map(order => (
						<div key={order.id} onClick={() => onSelectOrder(order)} className="cursor-pointer">
							<OrderCard {...order} />
						</div>
					))
				)}
			</div>
		</div>
	);
};

export default OrdersOverview;
