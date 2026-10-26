import { useState, type FormEvent } from "react";
import { Tarjeta } from "../comunes/Tarjeta";

export function SubirArchivo({ alSubir }: { alSubir: (archivo: File) => void }) {
  const [archivo, setArchivo] = useState<File | null>(null);
  const [formulario, setFormulario] = useState(0);

  const enviar = (evento: FormEvent) => {
    evento.preventDefault();
    if (!archivo) {
      return;
    }

    alSubir(archivo);
    setArchivo(null);
    setFormulario((actual) => actual + 1);
  };

  return (
    <Tarjeta
      titulo="Subir un archivo"
      descripcion="Se envía por partes a S3 y después se importa; el avance se sigue en Importaciones. Guarda una copia en S3: para el archivo real, usá «Desde la carpeta de datos»."
    >
      <form key={formulario} onSubmit={enviar} className="formulario">
        <label>
          Archivo
          <input type="file" accept=".txt" onChange={(e) => setArchivo(e.target.files?.[0] ?? null)} required />
        </label>
        <button className="boton" disabled={!archivo}>
          Subir e importar
        </button>
      </form>
    </Tarjeta>
  );
}
