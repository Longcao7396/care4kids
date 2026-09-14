import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

/**
 * Mounts a single global listener for the `giveaid:auth:expired` event
 * (dispatched from services/api.js on 401). When fired, it navigates
 * to /login via React Router — preserving the location the user was
 * trying to reach via location.state.from — so it can be honored after
 * they re-authenticate.
 *
 * Place this once near the root of the app, inside a <Router>.
 */
export default function AuthBootstrap() {
  const navigate = useNavigate();

  useEffect(() => {
    const handler = (e) => {
      const from = e?.detail?.from || window.location.pathname;
      navigate('/login', { replace: true, state: { from } });
    };
    window.addEventListener('giveaid:auth:expired', handler);
    return () => window.removeEventListener('giveaid:auth:expired', handler);
  }, [navigate]);

  return null;
}
