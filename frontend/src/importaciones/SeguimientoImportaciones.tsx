import { useState, type FormEvent } from "react";
import { obtenerImportacion } from "../api/cliente";
import type { Importacion } from "../api/tipos";
import { InsigniaEstado } from "../comunes/Insignias";
import { MensajeError } from "../comunes/MensajeError";
import { Tarjeta } from "../comunes/Tarjeta";
import { DetalleImportacion } from "./DetalleImportacion";
import { TarjetaSubida } from "./TarjetaSubida";
import type { Subida } from "./useSubidas";

interface Props {
  ids: string[];
  importaciones: Record<string, Importacion>;
  errores: Record<string, string>;
  agregar: (id: string) => void;
  subidas: Subida[];
}

export function SeguimientoImportaciones({ ids, importaciones, errores, agregar, subidas }: Props) {
  const [buscado, setBuscado] = useState("");
  const [filtro, setFiltro] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [buscando, setBuscando] = useState(false);

  // Solo se suma a la lista si existe; después se muestra únicamente esa hasta tocar "Ver todas".
  const buscar = async (evento: FormEvent) => {
    evento.preventDefault();
    const id = buscado.trim();
    setBuscando(true);
    setError(null);
    try {
      await obtenerImportacion(id);
      agregar(id);
      setFiltro(id);
      setBuscado("");
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setBuscando(false);
    }
  };

  const visibles = filtro === null ? ids : ids.filter((id) => id === filtro);

  return (
    <>
      <Tarjeta titulo="Buscar una importación" descripcion="Las importaciones pedidas desde este navegador aparecen abajo. Para ver una en particular, pegá su id.">
        <form onSubmit={buscar} className="formulario en-linea">
          <input value={buscado} onChange={(e) => setBuscado(e.target.value)} placeholder="Id de la importación" aria-label="Id de la importación" required />
          <button className="boton" disabled={buscando}>
            Buscar
          </button>
        </form>
        <MensajeError mensaje={error} />
      </Tarjeta>

      {filtro !== null && (
        <div className="filtro-activo">
          <span>
            Mostrando solo la importación <code>{filtro}</code>
          </span>
          <button className="boton-secundario" onClick={() => setFiltro(null)}>
            Ver todas
          </button>
        </div>
      )}

      {ids.length === 0 && subidas.length === 0 && <p className="vacio">Todavía no hay importaciones. Pedí una desde la sección Importar.</p>}

      {filtro === null && subidas.map((subida) => <TarjetaSubida key={subida.clave} subida={subida} />)}

      {visibles.map((id) => (
        <article key={id} className="tarjeta importacion" aria-live="polite">
          <header>
            <code title={id}>{id}</code>
            {importaciones[id] && <InsigniaEstado estado={importaciones[id].estado} />}
          </header>
          <MensajeError mensaje={errores[id] ?? null} />
          {importaciones[id] && <DetalleImportacion importacion={importaciones[id]} />}
        </article>
      ))}
    </>
  );
}
