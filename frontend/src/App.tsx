import { useEffect, useState, type ReactNode } from "react";
import { ErrorInesperado } from "./comunes/ErrorInesperado";
import { idPanel, idPestania, Pestanias } from "./comunes/Pestanias";
import { Consultas } from "./consultas/Consultas";
import { Encabezado } from "./encabezado/Encabezado";
import { Importar } from "./importaciones/Importar";
import { SeguimientoImportaciones } from "./importaciones/SeguimientoImportaciones";
import { useImportaciones } from "./importaciones/useImportaciones";
import { useSubidas } from "./importaciones/useSubidas";

const Secciones = ["Importar", "Importaciones", "Consultas"] as const;
type Seccion = (typeof Secciones)[number];

// La sección queda en la dirección (#consultas): se puede compartir y sobrevive a una recarga.
const seccionDeLaDireccion = (): Seccion =>
  Secciones.find((nombre) => `#${nombre.toLowerCase()}` === window.location.hash) ?? "Importar";

export function App() {
  const [seccion, setSeccion] = useState<Seccion>(seccionDeLaDireccion);
  const seguimiento = useImportaciones();
  const { subidas, subir } = useSubidas(seguimiento.agregar);

  // Si cambia la dirección (atrás/adelante del navegador o un enlace), se muestra esa sección.
  useEffect(() => {
    const alCambiar = () => setSeccion(seccionDeLaDireccion());
    window.addEventListener("hashchange", alCambiar);
    return () => window.removeEventListener("hashchange", alCambiar);
  }, []);

  const elegir = (nombre: Seccion) => {
    setSeccion(nombre);
    window.history.pushState(null, "", `#${nombre.toLowerCase()}`);
  };

  // Pedida la importación (por ruta o subida), se pasa a la sección donde se sigue su estado.
  const alImportar = (id: string) => {
    seguimiento.agregar(id);
    elegir("Importaciones");
  };

  const alSubir = (archivo: File) => {
    subir(archivo);
    elegir("Importaciones");
  };

  const contenido: Record<Seccion, ReactNode> = {
    Importar: <Importar alImportar={alImportar} alSubir={alSubir} />,
    Importaciones: <SeguimientoImportaciones {...seguimiento} subidas={subidas} />,
    Consultas: <Consultas />,
  };

  // Las secciones ocultas siguen cargadas: el resultado de una consulta no se pierde al cambiar.
  return (
    <>
      <Encabezado />
      <Pestanias pestanias={Secciones} activa={seccion} alElegir={elegir} />
      <main>
        {Secciones.map((nombre) => (
          <section key={nombre} id={idPanel(nombre)} role="tabpanel" aria-labelledby={idPestania(nombre)} hidden={nombre !== seccion} className="panel">
            <ErrorInesperado>{contenido[nombre]}</ErrorInesperado>
          </section>
        ))}
      </main>
    </>
  );
}
