// Nombres de los códigos de situación del BCRA (los mismos del enum Situacion del backend).
export const Situaciones: Record<number, string> = {
  1: "Normal",
  2: "Riesgo bajo",
  3: "Riesgo medio",
  4: "Riesgo alto",
  5: "Irrecuperable",
  11: "Cubierta con garantía A",
  21: "En observación",
  22: "En negociación",
  23: "Tratamiento especial",
  99: "No clasificado",
};

const numeros = new Intl.NumberFormat("es-AR");
const pesos = new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS", maximumFractionDigits: 0 });
const fechas = new Intl.DateTimeFormat("es-AR", { dateStyle: "short", timeStyle: "medium" });

export const numero = (valor: number) => numeros.format(valor);

// El BCRA informa los montos en miles de pesos: 261,0 son $ 261.000.
export const montoEnPesos = (miles: number) => pesos.format(miles * 1000);

export const fecha = (iso: string | null) => (iso ? fechas.format(new Date(iso)) : "—");

export const nombreSituacion = (codigo: number) => Situaciones[codigo] ?? `Código ${codigo}`;
