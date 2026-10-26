import { useState } from "react";
import { obtenerTop } from "../api/cliente";
import type { Deudor } from "../api/tipos";
import { montoEnPesos, Situaciones } from "../comunes/formato";
import { InsigniaSituacion } from "../comunes/Insignias";
import { MensajeError } from "../comunes/MensajeError";
import { Tarjeta } from "../comunes/Tarjeta";
import { useConsulta } from "../comunes/useConsulta";

export function TopDeudores() {
  const [cantidad, setCantidad] = useState(10);
  const [situacion, setSituacion] = useState("");
  const { resultado, error, cargando, enviar } = useConsulta(() => obtenerTop(cantidad, situacion === "" ? null : Number(situacion)));

  return (
    <Tarjeta titulo="Mayores deudores" descripcion="Ordenados por suma de préstamos; si empatan, por identificación. El filtro es por código exacto.">
      <form onSubmit={enviar} className="formulario en-linea">
        <label>
          Cantidad
          <input type="number" min={1} max={1000} value={cantidad} onChange={(e) => setCantidad(Number(e.target.value))} required />
        </label>
        <label>
          Situación
          <select value={situacion} onChange={(e) => setSituacion(e.target.value)}>
            <option value="">Todas</option>
            {Object.entries(Situaciones).map(([codigo, nombre]) => (
              <option key={codigo} value={codigo}>
                {codigo} · {nombre}
              </option>
            ))}
          </select>
        </label>
        <button className="boton" disabled={cargando}>
          Ver
        </button>
      </form>
      <MensajeError mensaje={error} />
      {resultado && <TablaDeudores deudores={resultado} />}
    </Tarjeta>
  );
}

function TablaDeudores({ deudores }: { deudores: Deudor[] }) {
  if (deudores.length === 0) {
    return <p className="vacio">No hay deudores con esa situación.</p>;
  }

  return (
    <div className="tabla-contenedor">
      <table>
        <thead>
          <tr>
            <th>#</th>
            <th>Identificación</th>
            <th>Situación máxima</th>
            <th className="numero">Suma de préstamos</th>
          </tr>
        </thead>
        <tbody>
          {deudores.map((deudor, indice) => (
            <tr key={deudor.nroIdentificacion}>
              <td>{indice + 1}</td>
              <td>
                <code>{deudor.nroIdentificacion}</code>
              </td>
              <td>
                <InsigniaSituacion codigo={deudor.situacionMaxima} />
              </td>
              <td className="numero">{montoEnPesos(deudor.sumaTotalPrestamos)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
