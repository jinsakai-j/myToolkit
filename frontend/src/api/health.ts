import { apiGet } from './client';
import type { HealthResponse } from '../types/health';

export function getHealth(): Promise<HealthResponse> {
  return apiGet<HealthResponse>('/api/health');
}

