import React, { useState } from 'react';

type NewOrderProps = {
	open: boolean;
	onClose: () => void;
	onCreate: (order: { cliente: string; produto: string; valor: number }) => void;
};

const NewOrder: React.FC<NewOrderProps> = ({ open, onClose, onCreate }) => {
	const [cliente, setCliente] = useState('');
	const [produto, setProduto] = useState('');
	const [valor, setValor] = useState('');

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();
		onCreate({ cliente, produto, valor: parseFloat(valor.replace(/\D/g, '')) / 100 });
		setCliente('');
		setProduto('');
		setValor('');
	};

	if (!open) return null;

	return (
		<div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
			<div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-8 w-full max-w-md relative">
				<button
					className="absolute top-2 right-2 text-gray-500 hover:text-gray-700"
					onClick={onClose}
				>
					&times;
				</button>
				<h2 className="text-xl font-bold mb-4 text-gray-900 dark:text-white">Nova Ordem</h2>
				<form className="flex flex-col gap-4" onSubmit={handleSubmit}>
					<div>
						<label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-200">Cliente</label>
						<input
							type="text"
							className="w-full border rounded px-3 py-2 focus:outline-none focus:ring focus:border-blue-400 dark:bg-gray-700 dark:text-white"
							value={cliente}
							onChange={e => setCliente(e.target.value)}
							required
						/>
					</div>
					<div>
						<label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-200">Produto</label>
						<input
							type="text"
							className="w-full border rounded px-3 py-2 focus:outline-none focus:ring focus:border-blue-400 dark:bg-gray-700 dark:text-white"
							value={produto}
							onChange={e => setProduto(e.target.value)}
							required
						/>
					</div>
					<div>
						<label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-200">Valor</label>
						<input
							type="text"
							className="w-full border rounded px-3 py-2 focus:outline-none focus:ring focus:border-blue-400 dark:bg-gray-700 dark:text-white"
							value={valor}
							onChange={e => {
								const raw = e.target.value.replace(/\D/g, '');
								const num = Number(raw) / 100;
								setValor(num.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }));
							}}
							required
							inputMode="numeric"
							pattern="[0-9,.]*"
						/>
					</div>
					<button
						type="submit"
						className="bg-green-600 text-white px-4 py-2 rounded shadow hover:bg-green-700 transition"
					>
						Salvar
					</button>
				</form>
			</div>
		</div>
	);
};

export default NewOrder;
