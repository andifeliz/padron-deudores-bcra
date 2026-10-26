import { useRef, type KeyboardEvent } from "react";

interface Props<T extends string> {
  pestanias: readonly T[];
  activa: T;
  alElegir: (pestania: T) => void;
}

// Se cambia de pestaña con clic o con las flechas, Inicio y Fin.
export function Pestanias<T extends string>({ pestanias, activa, alElegir }: Props<T>) {
  const botones = useRef<(HTMLButtonElement | null)[]>([]);

  const moverCon = (evento: KeyboardEvent, indice: number) => {
    const destinos: Record<string, number> = {
      ArrowRight: (indice + 1) % pestanias.length,
      ArrowLeft: (indice - 1 + pestanias.length) % pestanias.length,
      Home: 0,
      End: pestanias.length - 1,
    };
    const destino = destinos[evento.key];
    if (destino === undefined) {
      return;
    }

    evento.preventDefault();
    alElegir(pestanias[destino]);
    botones.current[destino]?.focus();
  };

  return (
    <div className="pestanias" role="tablist" aria-label="Secciones">
      {pestanias.map((nombre, indice) => (
        <button
          key={nombre}
          ref={(boton) => {
            botones.current[indice] = boton;
          }}
          id={idPestania(nombre)}
          role="tab"
          aria-selected={nombre === activa}
          aria-controls={idPanel(nombre)}
          tabIndex={nombre === activa ? 0 : -1}
          onClick={() => alElegir(nombre)}
          onKeyDown={(evento) => moverCon(evento, indice)}
        >
          {nombre}
        </button>
      ))}
    </div>
  );
}

export const idPestania = (nombre: string) => `pestania-${nombre.toLowerCase()}`;
export const idPanel = (nombre: string) => `panel-${nombre.toLowerCase()}`;
