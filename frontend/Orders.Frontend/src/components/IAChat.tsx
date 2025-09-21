import React, { useState, useRef, useEffect } from 'react';
import api from '../services/api';

type IAChatProps = {
    open: boolean;
    onClose: () => void;
};

type Message = {
    role: 'user' | 'ia';
    text: string;
};

const IAChat: React.FC<IAChatProps> = ({ open, onClose }) => {
    const [question, setQuestion] = useState('');
    const [messages, setMessages] = useState<Message[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const chatEndRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (open) {
            setQuestion('');
            setMessages([]);
            setError('');
        }
    }, [open]);

    useEffect(() => {
        chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages, open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!question.trim()) return;
        setMessages(prev => [...prev, { role: 'user', text: question }]);
        setLoading(true);
        setError('');
        try {
            const res = await api.post('/analytics/ask', { question });
            const iaText = res.data.answer || res.data;
            setMessages(prev => [...prev, { role: 'ia', text: iaText }]);
        } catch {
            setMessages(prev => [...prev, { role: 'ia', text: 'Erro ao consultar IA.' }]);
        }
        setLoading(false);
        setQuestion('');
    };

    if (!open) return null;

    const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
        if (e.target === e.currentTarget) {
            onClose();
        }
    };

    return (
        <div
            className="fixed inset-0 bg-black/64 flex items-center justify-center z-50"
            onClick={handleBackdropClick}
        >
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg w-full max-w-md flex flex-col relative h-[500px]">
                <h2 className="text-xl font-bold p-4 border-b text-gray-900 dark:text-white">Chat com a IA</h2>
                <div className="flex-1 overflow-y-auto px-4 py-2 space-y-2">
                    {messages.length === 0 && (
                        <div className="text-gray-500 text-center mt-10">Faça uma pergunta para a IA...</div>
                    )}
                    {messages.map((msg, idx) => (
                        <div
                            key={idx}
                            className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
                        >
                            <div
                                className={`px-4 py-2 rounded-lg max-w-[80%] ${
                                    msg.role === 'user'
                                        ? 'bg-violet-600 text-white'
                                        : 'bg-gray-200 dark:bg-gray-700 text-gray-900 dark:text-white'
                                }`}
                            >
                                {msg.text}
                            </div>
                        </div>
                    ))}
                    <div ref={chatEndRef} />
                </div>
                <form
                    className="p-4 border-t flex gap-2"
                    onSubmit={handleSubmit}
                >
                    <input
                        type="text"
                        className="flex-1 border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring focus:border-blue-400 dark:bg-gray-700 dark:text-white"
                        placeholder="Digite sua pergunta..."
                        value={question}
                        onChange={e => setQuestion(e.target.value)}
                        disabled={loading}
                        required
                    />
                    <button
                        type="submit"
                        className="bg-violet-950 text-white px-4 py-2 rounded shadow hover:bg-violet-900 transition"
                        disabled={loading}
                    >
                        {loading ? '...' : 'Enviar'}
                    </button>
                </form>
                {error && <div className="text-red-500 mt-2 px-4">{error}</div>}
            </div>
        </div>
    );
};

export default IAChat;