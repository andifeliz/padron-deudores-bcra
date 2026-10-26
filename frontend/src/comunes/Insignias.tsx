import type { EstadoImportacion } from "../api/tipos";
import { nombreSituacion } from "./formato";

const NombresEstado: Record<EstadoImportacion, string> = {
  pendiente: "Pendiente",
  procesando: "Procesando",
  completada: "Completada",
  fallida: "Fallida",
};

export function InsigniaEstado({ estado }: { estado: EstadoImportacion }) {
  return <span className={`insignia estado-${estado}`}>{NombresEstado[estado]}</span>;
}

export function InsigniaSituacion({ codigo }: { codigo: number }) {
  return (
    <span className={`insignia situacion-${codigo}`}>
      {codigo} · {nombreSituacion(codigo)}
    </span>
  );
}
