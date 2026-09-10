export interface ScanReport {
  id: string;
  scanId: string;
  fileName: string;
  generatedAt: string;
}

export interface GenerateReportRequest {
  fileName?: string;
}