import { useEffect, useState } from 'react';
import { getScanDetail, deleteScan, updateScanNotes } from '../api/scans';
import { generateReport, getScanReports, getReportDownloadUrl } from '../api/reports';
import type { ScanDetail as ScanDetailType } from '../types/scans';
import type { ScanReport } from '../types/reports';

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
  const [reports, setReports] = useState<ScanReport[]>([]);
  const [reportsLoading, setReportsLoading] = useState(false);
  const [reportGenLoading, setReportGenLoading] = useState(false);
  const [reportError, setReportError] = useState<string | null>(null);
  const [notesValue, setNotesValue] = useState('');
  const [notesEditing, setNotesEditing] = useState(false);
  const [notesSaving, setNotesSaving] = useState(false);
  const [notesError, setNotesError] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    getScanDetail(scanId)
      .then((data) => {
        if (isMounted) {
          setScan(data);
          setNotesValue(data.notes ?? '');
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

  const loadReports = async () => {
    setReportsLoading(true);
    setReportError(null);
    try {
      const data = await getScanReports(scanId);
      setReports(data);
    } catch (err: unknown) {
      if (err instanceof Error) {
        setReportError(err.message);
      }
    } finally {
      setReportsLoading(false);
    }
  };

  useEffect(() => {
    loadReports();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [scanId]);

  const handleGenerateReport = async () => {
    setReportGenLoading(true);
    setReportError(null);
    try {
      await generateReport(scanId);
      await loadReports();
    } catch (err: unknown) {
      if (err instanceof Error) {
        setReportError(err.message || 'Failed to generate report.');
      } else {
        setReportError('Failed to generate report.');
      }
    } finally {
      setReportGenLoading(false);
    }
  };

  const handleEditNotes = () => {
    setNotesValue(scan?.notes ?? '');
    setNotesError(null);
    setNotesEditing(true);
  };

  const handleSaveNotes = async () => {
    setNotesSaving(true);
    setNotesError(null);
    try {
      const updated = await updateScanNotes(scanId, { notes: notesValue.trim() || null });
      setScan((current) => (current ? { ...current, notes: updated.notes } : current));
      setNotesEditing(false);
    } catch (err: unknown) {
      if (err instanceof Error) {
        setNotesError(err.message || 'Failed to save notes.');
      } else {
        setNotesError('Failed to save notes.');
      }
    } finally {
      setNotesSaving(false);
    }
  };

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
            <div>
              <dt>Notes</dt>
              <dd>
                {notesEditing ? (
                  <div className="notes-editor">
                    <textarea
                      value={notesValue}
                      onChange={(e) => setNotesValue(e.target.value)}
                      rows={3}
                      placeholder="Add notes about this scan..."
                    />
                    {notesError && (
                      <div className="alert alert-danger" role="alert">
                        <strong>Error:</strong> {notesError}
                      </div>
                    )}
                    <div className="notes-editor-actions">
                      <button
                        type="button"
                        className="btn btn-sm btn-primary"
                        onClick={handleSaveNotes}
                        disabled={notesSaving}
                      >
                        {notesSaving ? 'Saving...' : 'Save'}
                      </button>
                      <button
                        type="button"
                        className="btn btn-sm btn-secondary"
                        onClick={() => {
                          setNotesEditing(false);
                          setNotesError(null);
                        }}
                        disabled={notesSaving}
                      >
                        Cancel
                      </button>
                    </div>
                  </div>
                ) : (
                  <span className="notes-display">
                    {scan.notes || <em className="text-muted">No notes yet.</em>}
                    <button
                      type="button"
                      className="btn btn-sm btn-secondary notes-edit-btn"
                      onClick={handleEditNotes}
                    >
                      Edit
                    </button>
                  </span>
                )}
              </dd>
            </div>
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

        <article className="panel reports-panel">
          <div className="panel-header">
            <h3>Reports ({reports.length})</h3>
            <button
              type="button"
              className="btn btn-sm btn-primary"
              onClick={handleGenerateReport}
              disabled={reportGenLoading || reportsLoading}
            >
              {reportGenLoading ? 'Generating...' : 'Generate Report'}
            </button>
          </div>

          {reportError && (
            <div className="alert alert-danger" role="alert">
              <strong>Error:</strong> {reportError}
            </div>
          )}

          {reportsLoading ? (
            <p className="reports-loading">Loading reports...</p>
          ) : reports.length === 0 ? (
            <div className="empty-state">
              <p>No reports yet. Generate a PDF report for this scan.</p>
            </div>
          ) : (
            <ul className="reports-list">
              {reports.map((report) => (
                <li key={report.id} className="report-item">
                  <div className="report-item-info">
                    <span className="report-item-name">{report.fileName}</span>
                    <span className="report-item-date">
                      Generated: {formatDateTime(report.generatedAt)}
                    </span>
                  </div>
                  <a
                    className="btn btn-sm btn-secondary"
                    href={getReportDownloadUrl(report.id)}
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    Download
                  </a>
                </li>
              ))}
            </ul>
          )}
        </article>
      </section>
    </section>
  );
}
