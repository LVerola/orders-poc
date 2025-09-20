import React from 'react';

type HeaderProps = {
	onNewOrder: () => void;
};

const Header: React.FC<HeaderProps> = ({ onNewOrder }) => {
	return (
		<header className="w-full bg-blue-600 text-white py-4 px-6 shadow flex items-center justify-between">
			<h1 className="text-xl font-bold">Orders Application</h1>
			<button
				className="bg-white text-blue-600 px-4 py-2 rounded shadow hover:bg-blue-100 transition font-semibold"
				onClick={onNewOrder}
			>
				Criar nova Ordem
			</button>
		</header>
	);
};

export default Header;
