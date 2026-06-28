import { useEffect, useState } from 'react';
import { getHealth } from '../api/health';
import { StatusBadge } from '../components/StatusBadge';
import type { HealthResponse } from '../types/health';

type HealthState = {
  status: 'checking' | 'online' | 'offline';
  data?: HealthResponse;
  error?: string;
};

export function Dashboard() {
  const [health, setHealth] = useState<HealthState>({ status: 'checking' });

  useEffect(() => {
    let isMounted = true;

    getHealth()
      .then((data) => {
        if (isMounted) {
          setHealth({ status: 'online', data });
        }
      })
      .catch((error: Error) => {
        if (isMounted) {
          setHealth({ status: 'offline', error: error.message });
        }
      });

    return () => {
      isMounted = false;
    };
  }, []);

  return (
    <main className="app-shell">
      <aside className="sidebar">
        <div>
          <p className="eyebrow">Localhost</p>
          <h1>OSINT Toolkit Local</h1>
        </div>
        <nav className="nav-list" aria-label="Main navigation">
          <a href="/" aria-current="page">Dashboard</a>
          <a href="/">New Scan</a>
          <a href="/">Reports</a>
          <a href="/">Settings</a>
        </nav>
      </aside>

      <section className="content">
        <header className="page-header">
          <div>
            <p className="eyebrow">Sprint 0</p>
            <h2>Project Foundation</h2>
          </div>
          <StatusBadge status={health.status} />
        </header>

        <section className="metrics-grid" aria-label="Foundation status">
          <article className="metric-card">
            <span>Backend API</span>
            <strong>{health.status === 'online' ? 'Connected' : 'Waiting'}</strong>
          </article>
          <article className="metric-card">
            <span>Database</span>
            <strong>PostgreSQL</strong>
          </article>
          <article className="metric-card">
            <span>OSINT Modules</span>
            <strong>Not Started</strong>
          </article>
        </section>

        <section className="panel">
          <div className="panel-header">
            <h3>API Health</h3>
            <span>{health.data?.service ?? 'OSINT Toolkit Local API'}</span>
          </div>
          <dl className="detail-list">
            <div>
              <dt>Status</dt>
              <dd>{health.data?.status ?? health.status}</dd>
            </div>
            <div>
              <dt>Timestamp UTC</dt>
              <dd>{health.data?.timestampUtc ?? '-'}</dd>
            </div>
            <div>
              <dt>Error</dt>
              <dd>{health.error ?? '-'}</dd>
            </div>
          </dl>
        </section>
      </section>
    </main>
  );
}

