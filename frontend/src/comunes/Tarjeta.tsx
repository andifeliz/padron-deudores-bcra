import type { ReactNode } from "react";

interface Props {
  titulo: string;
  descripcion?: string;
  children: ReactNode;
}

export function Tarjeta({ titulo, descripcion, children }: Props) {
  return (
    <section className="tarjeta">
      <h2>{titulo}</h2>
      {descripcion && <p className="descripcion">{descripcion}</p>}
      {children}
    </section>
  );
}
