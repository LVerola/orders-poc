import React, { useState } from 'react';
import * as yup from 'yup';
import { useOrders } from '../hooks/useOrders';

type NewOrderProps = {
	open: boolean;
	onClose: () => void;
};

const NewOrder: React.FC<NewOrderProps> = ({ open, onClose }) => {
	const [cliente, setCliente] = useState('');
	const [produto, setProduto] = useState('');
	const [valor, setValor] = useState('');
	const [errors, setErrors] = useState<{ [key: string]: string }>({});

	const { createOrder } = useOrders();

	const orderSchema = yup.object({
		cliente: yup.string().required('Cliente é obrigatório').min(1, 'Cliente é obrigatório'),
		produto: yup.string().required('Produto é obrigatório').min(1, 'Produto é obrigatório'),
		valor: yup
			.string()
			.required('Valor é obrigatório')
			.test('is-valid', 'Valor deve ser positivo', value => {
				if (!value) return false;
				const num = parseFloat(value.replace(/\D/g, '')) / 100;
				return num > 0;
			}),
	});

	const handleSubmit = async (e: React.FormEvent) => {
		try {
			e.preventDefault();
			console.log(valor);
			await orderSchema.validate({ cliente, produto, valor }, { abortEarly: false });
			createOrder({ cliente, produto, valor: parseFloat(valor.replace(/\D/g, '')) / 100 });
			setCliente('');
			setProduto('');
			setValor('');
			setErrors({});
			onClose();
		} catch (err: unknown) {
			if (err instanceof yup.ValidationError) {
				const fieldErrors: { [key: string]: string } = {};
				err.inner.forEach(e => {
					if (e.path) fieldErrors[e.path] = e.message;
				});

				setErrors(fieldErrors);
			}
		}
	};

	if (!open) return null;

	const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
		if (e.target === e.currentTarget) {
			setErrors({});
			onClose();
		}
	};

	return (
		<div
			className="fixed inset-0 bg-black/64 flex items-center justify-center z-50"
			onClick={handleBackdropClick}
		>
			<div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-8 w-full max-w-md relative">
				<h2 className="text-xl font-bold mb-4 text-gray-900 dark:text-white">Nova Ordem</h2>
				<form className="flex flex-col gap-4" onSubmit={handleSubmit}>
					<div>
						<label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-200">Cliente</label>
						<input
							type="text"
							className={`w-full border ${errors.cliente ? 'border-red-500' : 'border-gray-300'} rounded px-3 py-2 focus:outline-none focus:ring focus:border-blue-400 dark:bg-gray-700 dark:text-white`}
							value={cliente}
							onChange={e => setCliente(e.target.value)}
						/>
						{errors.cliente && <span className="text-red-500 text-xs">{errors.cliente}</span>}
					</div>
					<div>
						<label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-200">Produto</label>
						<input
							type="text"
							className={`w-full border ${errors.produto ? 'border-red-500' : 'border-gray-300'} rounded px-3 py-2 focus:outline-none focus:ring focus:border-blue-400 dark:bg-gray-700 dark:text-white`}
							value={produto}
							onChange={e => setProduto(e.target.value)}
						/>
						{errors.produto && <span className="text-red-500 text-xs">{errors.produto}</span>}
					</div>
					<div>
						<label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-200">Valor</label>
						<input
							type="text"
							className={`w-full border ${errors.valor ? 'border-red-500' : 'border-gray-300'} rounded px-3 py-2 focus:outline-none focus:ring focus:border-blue-400 dark:bg-gray-700 dark:text-white`}
							value={valor}
							onChange={e => {
								const raw = e.target.value.replace(/\D/g, '');
								const num = Number(raw) / 100;
								setValor(num.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }));
							}}
							inputMode="numeric"
						/>
						{errors.valor && <span className="text-red-500 text-xs">{errors.valor}</span>}
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
