import { useState } from "react";
import { obtenerDeudor } from "../api/cliente";
import { Dato } from "../comunes/Dato";
import { montoEnPesos } from "../comunes/formato";
import { InsigniaSituacion } from "../comunes/Insignias";
import { MensajeError } from "../comunes/MensajeError";
import { Tarjeta } from "../comunes/Tarjeta";
import { useConsulta } from "../comunes/useConsulta";

export function BuscarDeudor() {
  const [cuit, setCuit] = useState("");
  const { resultado, error, cargando, enviar } = useConsulta(() => obtenerDeudor(cuit.trim()));

  return (
    <Tarjeta titulo="Deudor" descripcion="Por CUIT, CUIL o identificación del exterior (11 letras o números).">
      <form onSubmit={enviar} className="formulario en-linea">
        <input value={cuit} onChange={(e) => setCuit(e.target.value)} placeholder="20123456789" aria-label="Identificación" required />
        <button className="boton" disabled={cargando}>
          Buscar
        </button>
      </form>
      <MensajeError mensaje={error} />
      {resultado && (
        <dl className="cifras" aria-live="polite">
          <Dato nombre="Identificación">{resultado.nroIdentificacion}</Dato>
          <Dato nombre="Situación máxima">
            <InsigniaSituacion codigo={resultado.situacionMaxima} />
          </Dato>
          <Dato nombre="Suma de préstamos">{montoEnPesos(resultado.sumaTotalPrestamos)}</Dato>
        </dl>
      )}
    </Tarjeta>
  );
}
