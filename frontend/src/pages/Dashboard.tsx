import { useEffect, useMemo, useState } from 'react';
import { getHealth } from '../api/health';
import { getScans } from '../api/scans';
import { StatusBadge } from '../components/StatusBadge';
import type { HealthResponse } from '../types/health';
import type { Scan, TargetType, ScanStatus } from '../types/scans';

type HealthState = {
  status: 'checking' | 'online' | 'offline';
  data?: HealthResponse;
  error?: string;
};

type SortOption = 'newest' | 'oldest' | 'riskHigh' | 'riskLow' | 'targetAz' | 'targetZa';

type DashboardProps = {
  onCreateScanClick: () => void;
  onScanSelect: (scanId: string) => void;
};

export function Dashboard({ onCreateScanClick, onScanSelect }: DashboardProps) {
  const [health, setHealth] = useState<HealthState>({ status: 'checking' });
  const [scans, setScans] = useState<Scan[]>([]);
  const [scansLoading, setScansLoading] = useState(true);
  const [scansError, setScansError] = useState<string | null>(null);
  
  // Filter states
  const [typeFilter, setTypeFilter] = useState<TargetType | ''>('');
  const [statusFilter, setStatusFilter] = useState<ScanStatus | ''>('');
  const [searchQuery, setSearchQuery] = useState('');
  const [sortOption, setSortOption] = useState<SortOption>('newest');

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

  // Fetch scans based on filters
  useEffect(() => {
    let isMounted = true;
    setScansLoading(true);

    const filters = {
      targetType: typeFilter || undefined,
      status: statusFilter || undefined,
    };

    getScans(filters)
      .then((data) => {
        if (isMounted) {
          setScans(data);
          setScansError(null);
        }
      })
      .catch((err: Error) => {
        if (isMounted) {
          setScansError(err.message || 'Failed to fetch scan list.');
        }
      })
      .finally(() => {
        if (isMounted) {
          setScansLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [typeFilter, statusFilter]);

  const formatDateTime = (dateStr: string | null) => {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleString();
  };

  const visibleScans = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    const filtered = query
      ? scans.filter((scan) => scan.target.toLowerCase().includes(query))
      : scans;

    return [...filtered].sort((a, b) => {
      switch (sortOption) {
        case 'oldest':
          return a.createdAt.localeCompare(b.createdAt);
        case 'riskHigh':
          return (b.riskScore ?? -1) - (a.riskScore ?? -1);
        case 'riskLow':
          return (a.riskScore ?? 101) - (b.riskScore ?? 101);
        case 'targetAz':
          return a.target.localeCompare(b.target);
        case 'targetZa':
          return b.target.localeCompare(a.target);
        default:
          return b.createdAt.localeCompare(a.createdAt);
      }
    });
  }, [scans, searchQuery, sortOption]);

  const getRiskLevel = (score: number): 'low' | 'medium' | 'high' => {
    if (score < 34) return 'low';
    if (score < 67) return 'medium';
    return 'high';
  };

  return (
    <section className="content">
      <header className="page-header">
        <div>
          <p className="eyebrow">Localhost Dashboard</p>
          <h2>Scan Overview</h2>
        </div>
        <div className="header-actions">
          <StatusBadge status={health.status} />
          <button type="button" className="btn btn-primary" onClick={onCreateScanClick}>
            New Scan
          </button>
        </div>
      </header>

      <section className="metrics-grid" aria-label="System metrics">
        <article className="metric-card">
          <span>Backend API</span>
          <strong>{health.status === 'online' ? 'Connected' : 'Offline'}</strong>
        </article>
        <article className="metric-card">
          <span>Total Scans</span>
          <strong>{scans.length}</strong>
        </article>
        <article className="metric-card">
          <span>Database</span>
          <strong>PostgreSQL (Port 5433)</strong>
        </article>
      </section>

      <section className="panel">
        <div className="panel-header table-header">
          <h3>Scan History</h3>
          <div className="filter-controls">
            <input
              className="form-input search-input-sm"
              type="search"
              placeholder="Search target..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              aria-label="Search scans by target"
            />

            <select
              className="form-select select-sm"
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value as TargetType | '')}
              aria-label="Filter by Target Type"
            >
              <option value="">All Types</option>
              <option value="Domain">Domain</option>
              <option value="Email">Email</option>
              <option value="Username">Username</option>
              <option value="IpAddress">IP Address</option>
            </select>

            <select
              className="form-select select-sm"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as ScanStatus | '')}
              aria-label="Filter by Status"
            >
              <option value="">All Statuses</option>
              <option value="Pending">Pending</option>
              <option value="Running">Running</option>
              <option value="Completed">Completed</option>
              <option value="Failed">Failed</option>
              <option value="Cancelled">Cancelled</option>
            </select>

            <select
              className="form-select select-sm"
              value={sortOption}
              onChange={(e) => setSortOption(e.target.value as SortOption)}
              aria-label="Sort scans"
            >
              <option value="newest">Newest first</option>
              <option value="oldest">Oldest first</option>
              <option value="riskHigh">Highest risk</option>
              <option value="riskLow">Lowest risk</option>
              <option value="targetAz">Target A-Z</option>
              <option value="targetZa">Target Z-A</option>
            </select>
          </div>
        </div>

        {scansError && (
          <div className="alert alert-danger" style={{ margin: '16px 0 0' }}>
            <strong>Error:</strong> {scansError}
          </div>
        )}

        {scansLoading ? (
          <div className="loading-state">
            <p>Loading scan history...</p>
          </div>
        ) : scans.length === 0 ? (
          <div className="empty-state">
            <p>No scans found. Click "New Scan" to start your first analysis.</p>
            <button type="button" className="btn btn-primary" onClick={onCreateScanClick} style={{ marginTop: '12px' }}>
              Create First Scan
            </button>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="table">
              <thead>
                <tr>
                  <th>Target</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Risk Score</th>
                  <th>Created At</th>
                  <th style={{ textAlign: 'right' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {visibleScans.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="table-empty-row">
                      No scans match the current search or filters.
                    </td>
                  </tr>
                ) : (
                  visibleScans.map((scan) => (
                    <tr key={scan.id} className="clickable-row" onClick={() => onScanSelect(scan.id)}>
                      <td>
                        <span className="target-link">{scan.target}</span>
                      </td>
                      <td>{scan.targetType === 'IpAddress' ? 'IP Address' : scan.targetType}</td>
                      <td>
                        <span className={`badge badge--status-${scan.status.toLowerCase()}`}>
                          {scan.status}
                        </span>
                      </td>
                      <td>
                        {scan.riskScore !== null ? (
                          <span className={`badge risk-badge risk-badge--${getRiskLevel(scan.riskScore)}`}>
                            {scan.riskScore}/100
                          </span>
                        ) : (
                          <span className="badge risk-badge risk-badge--none">N/A</span>
                        )}
                      </td>
                      <td>{formatDateTime(scan.createdAt)}</td>
                      <td style={{ textAlign: 'right' }} onClick={(e) => e.stopPropagation()}>
                        <button
                          type="button"
                          className="btn btn-sm btn-secondary"
                          onClick={() => onScanSelect(scan.id)}
                        >
                          View Details
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
            <p className="table-count">Showing {visibleScans.length} of {scans.length} scan(s)</p>
          </div>
        )}
      </section>
    </section>
  );
}
