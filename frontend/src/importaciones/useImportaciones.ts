import { useCallback, useEffect, useRef, useState } from "react";
import { ErrorApi, obtenerImportacion } from "../api/cliente";
import type { Importacion } from "../api/tipos";

// La API no lista importaciones: este navegador recuerda los ids que pidió y consulta su estado.
const Clave = "padron.importaciones";
const IntervaloMs = 2000;

function leerIds(): string[] {
  try {
    return JSON.parse(localStorage.getItem(Clave) ?? "[]") as string[];
  } catch {
    return [];
  }
}

function guardarIds(ids: string[]) {
  try {
    localStorage.setItem(Clave, JSON.stringify(ids));
  } catch {
    // Sin almacenamiento (por ejemplo, navegación privada): el historial dura hasta recargar la página.
  }
}

const terminada = (importacion?: Importacion) => importacion?.estado === "completada" || importacion?.estado === "fallida";

export function useImportaciones() {
  const [ids, setIds] = useState<string[]>(leerIds);
  const [importaciones, setImportaciones] = useState<Record<string, Importacion>>({});
  const [errores, setErrores] = useState<Record<string, string>>({});

  // Lo último que se sabe de cada una, para decidir a cuáles seguir consultando sin reiniciar el reloj.
  const conocidas = useRef<Record<string, Importacion>>({});
  const inexistentes = useRef(new Set<string>());

  const agregar = useCallback((id: string) => setIds((actuales) => [id, ...actuales.filter((x) => x !== id)]), []);

  useEffect(() => guardarIds(ids), [ids]);

  // Cada 2 segundos consulta las que todavía no terminaron; las terminadas y las inexistentes ya no cambian.
  useEffect(() => {
    const actualizar = async () => {
      const pendientes = ids.filter((id) => !terminada(conocidas.current[id]) && !inexistentes.current.has(id));
      for (const id of pendientes) {
        try {
          conocidas.current = { ...conocidas.current, [id]: await obtenerImportacion(id) };
          setImportaciones(conocidas.current);
        } catch (error) {
          if (error instanceof ErrorApi && error.status === 404) {
            inexistentes.current.add(id);
          }
          setErrores((actuales) => ({ ...actuales, [id]: (error as Error).message }));
        }
      }
    };

    void actualizar();
    const reloj = setInterval(actualizar, IntervaloMs);
    return () => clearInterval(reloj);
  }, [ids]);

  return { ids, importaciones, errores, agregar };
}
