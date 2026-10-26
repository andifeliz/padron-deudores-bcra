import { useState, type FormEvent } from "react";

// Estado de un formulario que consulta la API: el resultado, el error y si está esperando la respuesta.
export function useConsulta<T>(consultar: () => Promise<T>) {
  const [resultado, setResultado] = useState<T | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [cargando, setCargando] = useState(false);

  const enviar = async (evento: FormEvent) => {
    evento.preventDefault();
    setCargando(true);
    setError(null);
    setResultado(null);
    try {
      setResultado(await consultar());
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setCargando(false);
    }
  };

  return { resultado, error, cargando, enviar };
}
