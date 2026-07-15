import { useState } from 'react';
import { Dashboard } from './pages/Dashboard';
import { NewScan } from './pages/NewScan';
import { ScanDetail } from './pages/ScanDetail';

type View = 'dashboard' | 'new-scan' | 'scan-detail';

export function App() {
  const [currentView, setCurrentView] = useState<View>('dashboard');
  const [selectedScanId, setSelectedScanId] = useState<string | null>(null);

  const navigateToDashboard = () => {
    setSelectedScanId(null);
    setCurrentView('dashboard');
  };

  const navigateToNewScan = () => {
    setSelectedScanId(null);
    setCurrentView('new-scan');
  };

  const navigateToScanDetail = (scanId: string) => {
    setSelectedScanId(scanId);
    setCurrentView('scan-detail');
  };

  return (
    <main className="app-shell">
      <aside className="sidebar">
        <div>
          <p className="eyebrow">Localhost</p>
          <h1 style={{ cursor: 'pointer' }} onClick={navigateToDashboard}>
            OSINT Toolkit
          </h1>
        </div>
        <nav className="nav-list" aria-label="Main navigation">
          <button
            type="button"
            className={`nav-link-btn ${currentView === 'dashboard' ? 'active' : ''}`}
            aria-current={currentView === 'dashboard' ? 'page' : undefined}
            onClick={navigateToDashboard}
          >
            Dashboard
          </button>
          <button
            type="button"
            className={`nav-link-btn ${currentView === 'new-scan' ? 'active' : ''}`}
            aria-current={currentView === 'new-scan' ? 'page' : undefined}
            onClick={navigateToNewScan}
          >
            New Scan
          </button>
          <button
            type="button"
            className="nav-link-btn disabled-btn"
            disabled
          >
            Reports
          </button>
          <button
            type="button"
            className="nav-link-btn disabled-btn"
            disabled
          >
            Settings
          </button>
        </nav>
        <div className="sidebar-footer">
          <span className="text-muted" style={{ fontSize: '11px', color: '#64748b' }}>
            v0.2 - MVP Build
          </span>
        </div>
      </aside>

      {currentView === 'dashboard' && (
        <Dashboard
          onCreateScanClick={navigateToNewScan}
          onScanSelect={navigateToScanDetail}
        />
      )}

      {currentView === 'new-scan' && (
        <NewScan
          onBack={navigateToDashboard}
          onScanCreated={navigateToScanDetail}
        />
      )}

      {currentView === 'scan-detail' && selectedScanId && (
        <ScanDetail
          scanId={selectedScanId}
          onBack={navigateToDashboard}
          onDeleted={navigateToDashboard}
        />
      )}
    </main>
  );
}
