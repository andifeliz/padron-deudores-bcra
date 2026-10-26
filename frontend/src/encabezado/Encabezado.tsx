import { useEffect, useState } from "react";
import { estaLista } from "../api/cliente";

const IntervaloMs = 15000;

interface Salud {
  importador?: boolean;
  consulta?: boolean;
}

// Muestra si cada API puede atender (/health/ready: el proceso y la base responden).
export function Encabezado() {
  const [salud, setSalud] = useState<Salud>({});

  useEffect(() => {
    const revisar = async () => setSalud({ importador: await estaLista("importador"), consulta: await estaLista("consulta") });
    void revisar();
    const reloj = setInterval(revisar, IntervaloMs);
    return () => clearInterval(reloj);
  }, []);

  return (
    <header className="encabezado">
      <div>
        <h1>Padrón de deudores</h1>
        <p>Administración de importaciones y consultas del BCRA</p>
      </div>
      <ul className="salud" aria-label="Estado de las APIs">
        <EstadoApi nombre="Importador" lista={salud.importador} />
        <EstadoApi nombre="Consulta" lista={salud.consulta} />
      </ul>
    </header>
  );
}

function EstadoApi({ nombre, lista }: { nombre: string; lista?: boolean }) {
  const [clase, texto] = lista === undefined ? ["", "revisando…"] : lista ? ["ok", "en línea"] : ["caida", "sin conexión"];
  return (
    <li className={`salud-api ${clase}`}>
      <span className="punto" aria-hidden /> {nombre}: {texto}
    </li>
  );
}
