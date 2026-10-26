import { MensajeError } from "../comunes/MensajeError";
import type { Subida } from "./useSubidas";

// Una subida en curso. Cuando termina, su lugar lo ocupa la importación (pendiente, procesando…).
export function TarjetaSubida({ subida }: { subida: Subida }) {
  const fallo = subida.error !== null;

  return (
    <article className="tarjeta importacion" aria-live="polite">
      <header>
        <code>{subida.nombre}</code>
        <span className={`insignia ${fallo ? "estado-fallida" : "estado-subiendo"}`}>{fallo ? "Falló la subida" : `Subiendo ${subida.avance} %`}</span>
      </header>
      {!fallo && <progress max={100} value={subida.avance} aria-label={`Avance de la subida de ${subida.nombre}`} />}
      <MensajeError mensaje={subida.error} />
    </article>
  );
}
