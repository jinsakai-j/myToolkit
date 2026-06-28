type StatusBadgeProps = {
  status: 'checking' | 'online' | 'offline';
};

const labels: Record<StatusBadgeProps['status'], string> = {
  checking: 'Checking',
  online: 'Online',
  offline: 'Offline',
};

export function StatusBadge({ status }: StatusBadgeProps) {
  return <span className={`status-badge status-badge--${status}`}>{labels[status]}</span>;
}

