import React from 'react';

type HeaderProps = {
	onNewOrder: () => void;
};

const Header: React.FC<HeaderProps> = ({ onNewOrder }) => {
	return (
		<header className="w-full bg-violet-950 text-white py-4 px-6 shadow">
			<div className="max-w-5xl mx-auto flex items-center justify-between">
				<h1 className="text-xl font-bold">Orders Application</h1>
				<button
					className="bg-white text-violet-950 px-4 py-2 rounded shadow hover:bg-blue-100 transition font-semibold"
					onClick={onNewOrder}
				>
					Novo Pedido
				</button>
			</div>
		</header>
	);
};

export default Header;
