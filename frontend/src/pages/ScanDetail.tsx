import { useEffect, useState } from 'react';
import { getScanDetail, deleteScan } from '../api/scans';
import type { ScanDetail as ScanDetailType } from '../types/scans';

type ScanDetailProps = {
  scanId: string;
  onBack: () => void;
  onDeleted: () => void;
};

export function ScanDetail({ scanId, onBack, onDeleted }: ScanDetailProps) {
  const [scan, setScan] = useState<ScanDetailType | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedResultId, setExpandedResultId] = useState<string | null>(null);
  const [deleteLoading, setDeleteLoading] = useState(false);

  useEffect(() => {
    let isMounted = true;

    getScanDetail(scanId)
      .then((data) => {
        if (isMounted) {
          setScan(data);
          setError(null);
        }
      })
      .catch((err: Error) => {
        if (isMounted) {
          setError(err.message || 'Failed to load scan details.');
        }
      })
      .finally(() => {
        if (isMounted) {
          setLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [scanId]);

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this scan and all its results? This action cannot be undone.')) {
      return;
    }

    setDeleteLoading(true);
    try {
      await deleteScan(scanId);
      onDeleted();
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message || 'Failed to delete scan.');
      } else {
        setError('Failed to delete scan.');
      }
      setDeleteLoading(false);
    }
  };

  const toggleRawData = (resultId: string) => {
    setExpandedResultId(expandedResultId === resultId ? null : resultId);
  };

  const formatDateTime = (dateStr: string | null) => {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleString();
  };

  if (loading) {
    return (
      <section className="content">
        <div className="loading-state">
          <p>Loading scan details...</p>
        </div>
      </section>
    );
  }

  if (error || !scan) {
    return (
      <section className="content">
        <header className="page-header">
          <h2>Scan Not Found</h2>
          <button type="button" className="btn btn-secondary" onClick={onBack}>
            Back to Dashboard
          </button>
        </header>
        <div className="alert alert-danger" role="alert">
          <strong>Error:</strong> {error || 'Scan details could not be retrieved.'}
        </div>
      </section>
    );
  }

  return (
    <section className="content">
      <header className="page-header">
        <div>
          <p className="eyebrow">Scan Detail</p>
          <h2>{scan.target}</h2>
        </div>
        <div className="header-actions">
          <button type="button" className="btn btn-secondary" onClick={onBack} disabled={deleteLoading}>
            Back
          </button>
          <button type="button" className="btn btn-danger" onClick={handleDelete} disabled={deleteLoading}>
            {deleteLoading ? 'Deleting...' : 'Delete Scan'}
          </button>
        </div>
      </header>

      <section className="detail-grid">
        <article className="panel detail-panel">
          <div className="panel-header">
            <h3>Overview</h3>
          </div>
          <dl className="detail-list">
            <div>
              <dt>Target</dt>
              <dd><strong>{scan.target}</strong></dd>
            </div>
            <div>
              <dt>Target Type</dt>
              <dd>{scan.targetType === 'IpAddress' ? 'IP Address' : scan.targetType}</dd>
            </div>
            <div>
              <dt>Scan Status</dt>
              <dd>
                <span className={`badge badge--status-${scan.status.toLowerCase()}`}>
                  {scan.status}
                </span>
              </dd>
            </div>
            <div>
              <dt>Risk Score</dt>
              <dd>{scan.riskScore !== null ? `${scan.riskScore}/100` : 'N/A'}</dd>
            </div>
            <div>
              <dt>Created At</dt>
              <dd>{formatDateTime(scan.createdAt)}</dd>
            </div>
            {scan.notes && (
              <div>
                <dt>Notes</dt>
                <dd>{scan.notes}</dd>
              </div>
            )}
          </dl>
        </article>

        <article className="panel results-panel">
          <div className="panel-header">
            <h3>OSINT Module Results ({scan.results.length})</h3>
          </div>

          {scan.results.length === 0 ? (
            <div className="empty-state">
              <p>No modules were run for this scan.</p>
            </div>
          ) : (
            <div className="results-list">
              {scan.results.map((result) => {
                const isExpanded = expandedResultId === result.id;
                return (
                  <div key={result.id} className="result-card">
                    <div className="result-card-header">
                      <div className="result-card-info">
                        <h4>{result.moduleName}</h4>
                        <span className={`badge badge--module-${result.status.toLowerCase()}`}>
                          {result.status}
                        </span>
                      </div>
                      <button
                        type="button"
                        className="btn btn-sm btn-secondary"
                        onClick={() => toggleRawData(result.id)}
                      >
                        {isExpanded ? 'Hide Raw Data' : 'View Raw Data'}
                      </button>
                    </div>

                    <div className="result-card-body">
                      <p className="result-summary">
                        {result.summary || 'No summary available.'}
                      </p>
                      <span className="result-date">
                        Executed: {formatDateTime(result.createdAt)}
                      </span>
                    </div>

                    {isExpanded && (
                      <div className="result-card-raw">
                        <h5>Raw JSON Output</h5>
                        <pre>
                          <code>
                            {result.rawData ? JSON.stringify(JSON.parse(result.rawData), null, 2) : '{}'}
                          </code>
                        </pre>
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          )}
        </article>
      </section>
    </section>
  );
}
