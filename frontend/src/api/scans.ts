import { apiGet, apiPost, apiDelete } from './client';
import type { Scan, ScanDetail, CreateScanRequest, TargetType, ScanStatus } from '../types/scans';

export async function createScan(request: CreateScanRequest): Promise<Scan> {
  return apiPost<CreateScanRequest, Scan>('/api/scans', request);
}

export async function getScans(filters?: { targetType?: TargetType; status?: ScanStatus }): Promise<Scan[]> {
  let query = '';
  if (filters) {
    const params = new URLSearchParams();
    if (filters.targetType) params.append('targetType', filters.targetType);
    if (filters.status) params.append('status', filters.status);
    const queryString = params.toString();
    if (queryString) {
      query = `?${queryString}`;
    }
  }
  return apiGet<Scan[]>(`/api/scans${query}`);
}

export async function getScanDetail(scanId: string): Promise<ScanDetail> {
  return apiGet<ScanDetail>(`/api/scans/${scanId}`);
}

export async function deleteScan(scanId: string): Promise<void> {
  return apiDelete(`/api/scans/${scanId}`);
}
