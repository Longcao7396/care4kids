import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './styles/App.css';
import 'bootstrap-icons/font/bootstrap-icons.css';
import { API_BASE_URL, BACKEND_CANDIDATES } from './config';

console.log('%c[GiveAID Client]', 'color:#0bf;font-weight:bold', `API base: ${API_BASE_URL}`);
console.log('[GiveAID Client] Fallback URLs:', BACKEND_CANDIDATES);

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
