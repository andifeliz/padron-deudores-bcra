import type { ReactNode } from "react";

// Un par nombre y valor dentro de una lista de datos (<dl>).
export function Dato({ nombre, children }: { nombre: string; children: ReactNode }) {
  return (
    <div>
      <dt>{nombre}</dt>
      <dd>{children}</dd>
    </div>
  );
}
