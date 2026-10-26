import { useCallback, useEffect, useRef, useState } from "react";
import { subirArchivo } from "../api/cliente";

// Un archivo que todavía viaja hacia la API: la importación recién existe cuando la subida termina.
export interface Subida {
  clave: number;
  nombre: string;
  avance: number;
  error: string | null;
}

// Las subidas en curso de toda la app: siguen aunque se cambie de sección.
// Al terminar, la tarjeta de la subida se reemplaza por la importación (alTerminar recibe su id).
export function useSubidas(alTerminar: (id: string) => void) {
  const [subidas, setSubidas] = useState<Subida[]>([]);
  const ultimaClave = useRef(0);
  const subiendo = subidas.some((subida) => subida.error === null);

  // Cerrar o recargar la página corta las subidas en curso: el navegador pide confirmación.
  useEffect(() => {
    if (!subiendo) {
      return;
    }

    const avisar = (evento: BeforeUnloadEvent) => evento.preventDefault();
    window.addEventListener("beforeunload", avisar);
    return () => window.removeEventListener("beforeunload", avisar);
  }, [subiendo]);

  const subir = useCallback(
    (archivo: File) => {
      const clave = ++ultimaClave.current;
      const actualizar = (cambios: Partial<Subida>) =>
        setSubidas((actuales) => actuales.map((subida) => (subida.clave === clave ? { ...subida, ...cambios } : subida)));

      setSubidas((actuales) => [{ clave, nombre: archivo.name, avance: 0, error: null }, ...actuales]);
      subirArchivo(archivo, (avance) => actualizar({ avance }))
        .then(({ id }) => {
          setSubidas((actuales) => actuales.filter((subida) => subida.clave !== clave));
          alTerminar(id);
        })
        .catch((error: Error) => actualizar({ error: error.message }));
    },
    [alTerminar],
  );

  return { subidas, subir };
}
