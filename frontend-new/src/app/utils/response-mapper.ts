import { DataUploadResult } from '../interfaces/user.interface';

export class ResponseMapper {
  /**
   * Convierte una respuesta del backend (.NET PascalCase) a formato frontend (camelCase)
   */
  static mapDataUploadResult(backendResponse: any): DataUploadResult {
    console.log('🔄 Mapping backend response:', backendResponse);
    
    // Si ya está en formato correcto (mock), devolver tal como está
    if (backendResponse.errorRecords !== undefined || backendResponse.processedRecords !== undefined) {
      console.log('✅ Response already in camelCase format');
      return backendResponse as DataUploadResult;
    }
    
    // Mapear desde PascalCase (backend .NET) a camelCase (frontend)
    const mapped: DataUploadResult = {
      success: backendResponse.Success ?? (backendResponse.ProcessedRecords > 0 || backendResponse.ErrorRecords === 0),
      message: backendResponse.Message || '',
      totalRecords: backendResponse.TotalRecords || 0,
      processedRecords: backendResponse.ProcessedRecords || 0,
      errorRecords: backendResponse.ErrorRecords || 0,
      errors: backendResponse.Errors || [],
      failedRecords: (backendResponse.FailedRecords || []).map((record: any) => ({
        rowNumber: record.RowNumber || 0,
        error: record.Error || '',
        originalData: record.OriginalData || {}
      })),
      status: backendResponse.Status || ''
    };
    
    console.log('✅ Mapped response:', mapped);
    return mapped;
  }
}