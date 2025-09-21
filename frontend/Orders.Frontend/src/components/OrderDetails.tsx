import React, { useEffect, useState } from 'react';
import { useOrder } from '../hooks/useOrder';

type OrderDetailsProps = {
	open: boolean;
	onClose: () => void;
	orderId: string | null;
};

const statusColors: Record<string, string> = {
	Pendente: 'bg-yellow-400',
	Processando: 'bg-blue-500',
	Finalizado: 'bg-green-500',
};

const OrderDetails: React.FC<OrderDetailsProps> = ({ open, onClose, orderId }) => {
	const { order, setSelectedOrderId } = useOrder();
	const [showHistory, setShowHistory] = useState(false);

	useEffect(() => {
		setSelectedOrderId(orderId);
	}, [orderId, setSelectedOrderId])

	const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
		if (e.target === e.currentTarget) {
			setShowHistory(false);
			onClose();
		}
	};

	if (!open || !orderId) return null;

	if (!order) {
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
		<div
			className="fixed inset-0 bg-black/64 flex items-center justify-center z-50"
			onClick={handleBackdropClick}
		>
			<div className="bg-white dark:bg-gray-700 rounded-lg shadow p-6 flex flex-col gap-2 w-full max-w-md mx-auto">
				<div className="flex items-center justify-between">
					<span className="text-sm text-gray-500">ID: {order.id}</span>
					<span className="flex items-center gap-2">
						<span className={`w-3 h-3 rounded-full ${statusColors[order.status]}`}></span>
						<span className="font-semibold text-gray-700 dark:text-gray-200">{order.status}</span>
					</span>
				</div>
				<div className="mt-2">
					<h3 className="text-lg font-bold text-gray-900 dark:text-white">{order.produto}</h3>
					<p className="text-gray-600 dark:text-gray-300">Cliente: {order.cliente}</p>
					<p className="text-gray-600 dark:text-gray-300">Valor: R$ {order.valor.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}</p>
					<p className="text-xs text-gray-400 mt-2">Criado em: {new Date(order.dataCriacao).toLocaleString('pt-BR')}</p>

					{order.statusHistories && order.statusHistories.length > 0 && (
						<div className="mt-3">
							<button
								type="button"
								className="text-violet-950 dark:text-white text-xs underline focus:outline-none"
								onClick={() => setShowHistory(v => !v)}
							>
								{showHistory ? 'Ocultar histórico' : 'Ver histórico'}
							</button>
							{showHistory && (
								<ul className="mt-2 border rounded bg-gray-50 dark:bg-gray-800 p-2 text-xs">
									{order.statusHistories.map((h) => (
										<li key={h.id} className="flex justify-between items-center py-1 border-b last:border-b-0 border-gray-200 dark:border-gray-700">
											<span className={`px-2 py-1 rounded text-xs font-semibold ${h.status === 'Finalizado' ? 'bg-green-100 text-green-700' : h.status === 'Processando' ? 'bg-yellow-100 text-yellow-700' : 'bg-red-100 text-red-700'}`}>{h.status}</span>
											<span className="text-gray-500 dark:text-gray-300">{new Date(h.dataAlteracao).toLocaleString('pt-BR')}</span>
										</li>
									))}
								</ul>
							)}
						</div>
					)}
				</div>
			</div>
		</div>
	);
};

export default OrderDetails;
