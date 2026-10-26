import type { Deudor, Entidad, Importacion, ImportacionAceptada, Problema } from "./tipos";

// Las dos APIs se llaman a través del mismo origen: nginx (o Vite, en desarrollo) las reenvía.
const Importador = "/api/importador";
const Consulta = "/api/consulta";

// Error de la API con un mensaje listo para mostrar.
export class ErrorApi extends Error {
  constructor(readonly status: number, mensaje: string) {
    super(mensaje);
  }
}

// El mensaje más útil de un error de la API: el de validación, el detalle o el título, en ese orden.
export function mensajeDelProblema(problema: Problema | null, status: number): string {
  const validacion = Object.values(problema?.errors ?? {}).flat()[0];
  return validacion ?? problema?.detail ?? problema?.title ?? `Error ${status}`;
}

async function leerProblema(respuesta: Response): Promise<string> {
  const problema = (await respuesta.json().catch(() => null)) as Problema | null;
  return mensajeDelProblema(problema, respuesta.status);
}

async function pedir<T>(url: string, opciones?: RequestInit): Promise<T> {
  const respuesta = await fetch(url, opciones);
  if (!respuesta.ok) {
    throw new ErrorApi(respuesta.status, await leerProblema(respuesta));
  }
  return respuesta.json() as Promise<T>;
}

export const importarDesdeRuta = (archivo: string) =>
  pedir<ImportacionAceptada>(`${Importador}/importaciones`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ archivo }),
  });

export const obtenerImportacion = (id: string) => pedir<Importacion>(`${Importador}/importaciones/${encodeURIComponent(id)}`);

export const obtenerDeudor = (cuit: string) => pedir<Deudor>(`${Consulta}/deudores/${encodeURIComponent(cuit)}`);

export const obtenerEntidad = (codigo: string) => pedir<Entidad>(`${Consulta}/entidades/${encodeURIComponent(codigo)}`);

export const obtenerTop = (n: number, situacion: number | null) =>
  pedir<Deudor[]>(`${Consulta}/deudores/top/${n}${situacion === null ? "" : `?situacion=${situacion}`}`);

export async function estaLista(api: "importador" | "consulta"): Promise<boolean> {
  try {
    return (await fetch(`${api === "importador" ? Importador : Consulta}/health/ready`)).ok;
  } catch {
    return false;
  }
}

// Usa XMLHttpRequest para informar el avance de la subida.
export function subirArchivo(archivo: File, alAvanzar: (porcentaje: number) => void): Promise<ImportacionAceptada> {
  return new Promise((resolver, rechazar) => {
    const pedido = new XMLHttpRequest();
    pedido.open("POST", `${Importador}/upload?nombre=${encodeURIComponent(archivo.name)}`);
    pedido.setRequestHeader("Content-Type", "application/octet-stream");
    pedido.upload.onprogress = (evento) => {
      if (evento.lengthComputable) {
        alAvanzar(Math.round((evento.loaded / evento.total) * 100));
      }
    };
    pedido.onload = () => {
      const cuerpo = leerJson(pedido.responseText);
      if (pedido.status >= 200 && pedido.status < 300) {
        resolver(cuerpo as ImportacionAceptada);
      } else {
        rechazar(new ErrorApi(pedido.status, mensajeDelProblema(cuerpo as Problema | null, pedido.status)));
      }
    };
    pedido.onerror = () => rechazar(new ErrorApi(0, "No se pudo conectar con la API."));
    pedido.send(archivo);
  });
}

function leerJson(texto: string): unknown {
  try {
    return JSON.parse(texto);
  } catch {
    return null;
  }
}
