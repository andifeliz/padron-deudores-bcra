import { useState, type FormEvent } from "react";
import { importarDesdeRuta } from "../api/cliente";
import { MensajeError } from "../comunes/MensajeError";
import { Tarjeta } from "../comunes/Tarjeta";

export function ImportarDesdeRuta({ alImportar }: { alImportar: (id: string) => void }) {
  const [archivo, setArchivo] = useState("deudores.txt");
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const enviar = async (evento: FormEvent) => {
    evento.preventDefault();
    setEnviando(true);
    setError(null);
    try {
      alImportar((await importarDesdeRuta(archivo.trim())).id);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setEnviando(false);
    }
  };

  return (
    <Tarjeta titulo="Desde la carpeta de datos" descripcion="Para archivos ya copiados en la carpeta data/ del servidor. Es la opción para el archivo real.">
      <form onSubmit={enviar} className="formulario">
        <label>
          Nombre del archivo
          <input value={archivo} onChange={(e) => setArchivo(e.target.value)} placeholder="deudores.txt" required />
        </label>
        <button className="boton" disabled={enviando}>
          {enviando ? "Enviando…" : "Importar"}
        </button>
      </form>
      <MensajeError mensaje={error} />
    </Tarjeta>
  );
}
