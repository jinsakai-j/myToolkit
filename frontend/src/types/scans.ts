export type TargetType = 'Domain' | 'Email' | 'Username' | 'IpAddress';

export type ScanStatus = 'Pending' | 'Running' | 'Completed' | 'Failed' | 'Cancelled';

export type ModuleStatus = 'Pending' | 'Skipped' | 'Completed' | 'Failed';

export interface ScanResult {
  id: string;
  scanId: string;
  moduleName: string;
  status: ModuleStatus;
  summary: string | null;
  rawData: string | null;
  createdAt: string;
}

export interface Scan {
  id: string;
  target: string;
  targetType: TargetType;
  status: ScanStatus;
  riskScore: number | null;
  createdAt: string;
  startedAt: string | null;
  completedAt: string | null;
  notes: string | null;
}

export interface ScanDetail extends Scan {
  results: ScanResult[];
}

export interface CreateScanRequest {
  target: string;
  targetType: TargetType;
  modules: string[];
}
