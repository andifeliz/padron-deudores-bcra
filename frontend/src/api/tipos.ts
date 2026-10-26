// Lo que devuelven las APIs. Los montos vienen en miles de pesos, como en el archivo del BCRA.

export type EstadoImportacion = "pendiente" | "procesando" | "completada" | "fallida";

export interface ImportacionAceptada {
  id: string;
  estado: EstadoImportacion;
}

export interface ResumenImportacion {
  lineasLeidas: number;
  lineasInvalidas: number;
  deudores: number;
  entidades: number;
  lineasPorPeriodo: Record<string, number>;
  codigosSituacionDesconocidos: Record<string, number>;
  duracionSegundos: number;
}

export interface Importacion {
  id: string;
  origen: "ruta" | "upload";
  archivo: string;
  estado: EstadoImportacion;
  creadaEn: string;
  iniciadaEn: string | null;
  finalizadaEn: string | null;
  resumen: ResumenImportacion | null;
  error: string | null;
}

export interface Deudor {
  nroIdentificacion: string;
  situacionMaxima: number;
  sumaTotalPrestamos: number;
}

export interface Entidad {
  codigoEntidad: string;
  sumaTotalPrestamos: number;
}

// Formato estándar de errores de las APIs (Problem Details, RFC 9457).
export interface Problema {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}
