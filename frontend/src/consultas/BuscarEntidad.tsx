import { useState } from "react";
import { obtenerEntidad } from "../api/cliente";
import { Dato } from "../comunes/Dato";
import { montoEnPesos } from "../comunes/formato";
import { MensajeError } from "../comunes/MensajeError";
import { Tarjeta } from "../comunes/Tarjeta";
import { useConsulta } from "../comunes/useConsulta";

export function BuscarEntidad() {
  const [codigo, setCodigo] = useState("");
  const { resultado, error, cargando, enviar } = useConsulta(() => obtenerEntidad(codigo.trim()));

  return (
    <Tarjeta titulo="Entidad" descripcion="Por código de entidad (hasta 5 dígitos: 7 y 00007 son la misma).">
      <form onSubmit={enviar} className="formulario en-linea">
        <input value={codigo} onChange={(e) => setCodigo(e.target.value)} placeholder="00007" aria-label="Código de entidad" required />
        <button className="boton" disabled={cargando}>
          Buscar
        </button>
      </form>
      <MensajeError mensaje={error} />
      {resultado && (
        <dl className="cifras" aria-live="polite">
          <Dato nombre="Código">{resultado.codigoEntidad}</Dato>
          <Dato nombre="Suma de préstamos">{montoEnPesos(resultado.sumaTotalPrestamos)}</Dato>
        </dl>
      )}
    </Tarjeta>
  );
}
