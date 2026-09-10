import { apiGet, apiPost } from './client';
import type { GenerateReportRequest, ScanReport } from '../types/reports';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5080';

export async function generateReport(scanId: string, request: GenerateReportRequest = {}): Promise<ScanReport> {
  return apiPost<GenerateReportRequest, ScanReport>(`/api/scans/${scanId}/reports`, request);
}

export async function getScanReports(scanId: string): Promise<ScanReport[]> {
  return apiGet<ScanReport[]>(`/api/scans/${scanId}/reports`);
}

export function getReportDownloadUrl(reportId: string): string {
  return `${API_BASE_URL}/api/reports/${reportId}/download`;
}