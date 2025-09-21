import React from 'react';

type Order = {
	id: string;
	cliente: string;
	produto: string;
	valor: number;
	status: 'Pendente' | 'Processando' | 'Finalizado';
	dataCriacao: string;
};

type OrdersOverviewProps = {
	orders: Order[] | undefined;
	onSelectOrder: (orderId: string) => void;
};

const OrdersOverview: React.FC<OrdersOverviewProps> = ({ orders, onSelectOrder }) => {
	if (!orders) {
		return (
			<div className="flex justify-center items-center w-full h-full">
				<div className="flex flex-col items-center">
					<div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-purple-950 dark:border-white mb-2"></div>
					<span className="text-purple-950 dark:text-white">Carregando...</span>
				</div>
			</div>
		);
	}

	return (
		<div className="bg-white dark:bg-gray-800 rounded-xl shadow-lg p-6 w-full max-w-5xl h-full flex flex-col" style={{ minHeight: '70vh' }}>
			<h2 className="text-2xl font-bold mb-4 text-gray-900 dark:text-white">Listagem de Pedidos</h2>
			<div className="flex-1 overflow-x-auto">
				{orders.length === 0 ? (
					<p className="text-gray-500">Nenhuma ordem cadastrada.</p>
				) : (
					<table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
						<thead>
							<tr className="bg-gray-100 dark:bg-gray-700">
								<th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Cliente</th>
								<th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Produto</th>
								<th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Valor</th>
								<th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Status</th>
								<th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Data</th>
							</tr>
						</thead>
						<tbody>
							{orders.map(order => (
								<tr
									key={order.id}
									onClick={() => onSelectOrder(order.id)}
									className="cursor-pointer hover:bg-blue-50 dark:hover:bg-gray-600 transition"
								>
									<td className="px-4 py-2 text-gray-900 dark:text-white">{order.cliente}</td>
									<td className="px-4 py-2 text-gray-900 dark:text-white">{order.produto}</td>
									<td className="px-4 py-2 text-gray-900 dark:text-white">{order.valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</td>
									<td className="px-4 py-2">
										<span className={`px-2 py-1 rounded text-xs font-semibold ${order.status === 'Finalizado' ? 'bg-green-100 text-green-700' : order.status === 'Processando' ? 'bg-yellow-100 text-yellow-700' : 'bg-red-100 text-red-700'}`}>
											{order.status}
										</span>
									</td>
									<td className="px-4 py-2 text-gray-900 dark:text-white">{new Date(order.dataCriacao).toLocaleDateString('pt-BR')}</td>
								</tr>
							))}
						</tbody>
					</table>
				)}
			</div>
		</div>
	);
};

export default OrdersOverview;
