import { useState, useEffect } from 'react';
import { createScan } from '../api/scans';
import type { TargetType } from '../types/scans';

type NewScanProps = {
  onBack: () => void;
  onScanCreated: (scanId: string) => void;
};

const MODULE_DEFINITIONS = {
  Domain: [
    { id: 'DnsLookup', name: 'DNS Lookup', desc: 'Queries MX, A, AAAA, TXT, NS records' },
    { id: 'WhoisLookup', name: 'WHOIS Lookup', desc: 'Queries registrar, expiry date, and details' },
  ],
  Email: [
    { id: 'EmailValidation', name: 'Email Validation', desc: 'Validates format and checks MX record' },
  ],
  Username: [
    { id: 'UsernameChecker', name: 'Username Checker', desc: 'Checks existence on popular platforms' },
  ],
  IpAddress: [
    { id: 'DnsLookup', name: 'Reverse DNS Lookup', desc: 'Queries PTR record' },
    { id: 'IpReputation', name: 'IP Reputation', desc: 'Checks threat intelligence databases' },
  ],
};

const TARGET_PLACEHOLDERS = {
  Domain: 'example.com',
  Email: 'analyst@example.com',
  Username: 'security_analyst',
  IpAddress: '8.8.8.8',
};

// Target Validation helper
const validateTarget = (target: string, type: TargetType): string | null => {
  if (!target.trim()) return 'Target cannot be empty.';

  switch (type) {
    case 'Domain':
      const domainRegex = /^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$/;
      if (!domainRegex.test(target.trim())) return 'Invalid domain format (e.g., example.com).';
      break;
    case 'Email':
      const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
      if (!emailRegex.test(target.trim())) return 'Invalid email format (e.g., analyst@example.com).';
      break;
    case 'Username':
      const usernameRegex = /^[a-zA-Z0-9_\-\.]{1,100}$/;
      if (!usernameRegex.test(target.trim())) return 'Invalid username (1-100 characters, alphanumeric, _, -, .).';
      break;
    case 'IpAddress':
      // Basic IP v4 / v6 parse test
      const ipv4Regex = /^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$/;
      const ipv6Regex = /^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$|^::$/; // simple regex
      if (!ipv4Regex.test(target.trim()) && !ipv6Regex.test(target.trim())) return 'Invalid IP address format.';
      break;
  }
  return null;
};

export function NewScan({ onBack, onScanCreated }: NewScanProps) {
  const [target, setTarget] = useState('');
  const [targetType, setTargetType] = useState<TargetType>('Domain');
  const [selectedModules, setSelectedModules] = useState<string[]>([]);
  const [validationError, setValidationError] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  // Set default modules when targetType changes
  useEffect(() => {
    const modules = MODULE_DEFINITIONS[targetType].map(m => m.id);
    setSelectedModules(modules);
    setTarget('');
    setValidationError(null);
  }, [targetType]);

  const handleModuleToggle = (moduleId: string) => {
    setSelectedModules(prev =>
      prev.includes(moduleId)
        ? prev.filter(id => id !== moduleId)
        : [...prev, moduleId]
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setValidationError(null);
    setError(null);

    const valErr = validateTarget(target, targetType);
    if (valErr) {
      setValidationError(valErr);
      return;
    }

    if (selectedModules.length === 0) {
      setValidationError('Please select at least one module.');
      return;
    }

    setLoading(true);
    try {
      const scan = await createScan({
        target: target.trim(),
        targetType,
        modules: selectedModules,
      });
      onScanCreated(scan.id);
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('An unexpected error occurred.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="content">
      <header className="page-header">
        <div>
          <p className="eyebrow">Sprint 1 MVP</p>
          <h2>Create New Scan</h2>
        </div>
        <button type="button" className="btn btn-secondary" onClick={onBack} disabled={loading}>
          Back to Dashboard
        </button>
      </header>

      <form className="panel form-container" onSubmit={handleSubmit}>
        <div className="panel-header">
          <h3>Scan Scope Details</h3>
          <span className="text-muted">Fields marked * are required</span>
        </div>

        {error && (
          <div className="alert alert-danger" role="alert">
            <strong>Error:</strong> {error}
          </div>
        )}

        <div className="form-group">
          <label className="form-label">Target Type *</label>
          <div className="radio-grid">
            {(['Domain', 'Email', 'Username', 'IpAddress'] as TargetType[]).map((type) => (
              <label key={type} className={`radio-card ${targetType === type ? 'active' : ''}`}>
                <input
                  type="radio"
                  name="targetType"
                  value={type}
                  checked={targetType === type}
                  onChange={() => setTargetType(type)}
                />
                <span className="radio-card-title">
                  {type === 'IpAddress' ? 'IP Address' : type}
                </span>
              </label>
            ))}
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="target-input" className="form-label">
            Target Value *
          </label>
          <input
            id="target-input"
            type="text"
            className={`form-input ${validationError ? 'form-input-error' : ''}`}
            placeholder={`e.g., ${TARGET_PLACEHOLDERS[targetType]}`}
            value={target}
            onChange={(e) => setTarget(e.target.value)}
            disabled={loading}
          />
          {validationError && <span className="form-error-msg">{validationError}</span>}
        </div>

        <div className="form-group">
          <label className="form-label">Select OSINT Modules *</label>
          <div className="checkbox-grid">
            {MODULE_DEFINITIONS[targetType].map((mod) => (
              <label key={mod.id} className={`checkbox-card ${selectedModules.includes(mod.id) ? 'active' : ''}`}>
                <input
                  type="checkbox"
                  checked={selectedModules.includes(mod.id)}
                  onChange={() => handleModuleToggle(mod.id)}
                  disabled={loading}
                />
                <div className="checkbox-card-content">
                  <span className="checkbox-card-title">{mod.name}</span>
                  <span className="checkbox-card-desc">{mod.desc}</span>
                </div>
              </label>
            ))}
          </div>
        </div>

        <div className="form-actions">
          <button type="button" className="btn btn-secondary" onClick={onBack} disabled={loading}>
            Cancel
          </button>
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Starting Scan...' : 'Start Scan'}
          </button>
        </div>
      </form>
    </section>
  );
}
