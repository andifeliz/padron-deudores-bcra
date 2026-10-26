import type { Importacion } from "../api/tipos";
import { Dato } from "../comunes/Dato";
import { fecha, numero } from "../comunes/formato";
import { MensajeError } from "../comunes/MensajeError";

export function DetalleImportacion({ importacion }: { importacion: Importacion }) {
  const { resumen } = importacion;

  return (
    <>
      <dl className="datos">
        <Dato nombre="Origen">{importacion.origen === "upload" ? "Subida a S3" : "Carpeta de datos"}</Dato>
        <Dato nombre="Archivo">{importacion.archivo}</Dato>
        <Dato nombre="Pedida">{fecha(importacion.creadaEn)}</Dato>
        <Dato nombre="Terminó">{fecha(importacion.finalizadaEn)}</Dato>
      </dl>
      {resumen && (
        <dl className="cifras">
          <Dato nombre="Líneas leídas">{numero(resumen.lineasLeidas)}</Dato>
          <Dato nombre="Líneas inválidas">{numero(resumen.lineasInvalidas)}</Dato>
          <Dato nombre="Deudores">{numero(resumen.deudores)}</Dato>
          <Dato nombre="Entidades">{numero(resumen.entidades)}</Dato>
          <Dato nombre="Duración">{numero(resumen.duracionSegundos)} s</Dato>
          <Dato nombre="Líneas por período">
            {Object.entries(resumen.lineasPorPeriodo)
              .map(([periodo, lineas]) => `${periodo}: ${numero(lineas)}`)
              .join(" · ")}
          </Dato>
        </dl>
      )}
      <MensajeError mensaje={importacion.error} />
    </>
  );
}
