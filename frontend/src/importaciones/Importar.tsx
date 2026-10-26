import { ImportarDesdeRuta } from "./ImportarDesdeRuta";
import { SubirArchivo } from "./SubirArchivo";

interface Props {
  alImportar: (id: string) => void;
  alSubir: (archivo: File) => void;
}

// Las dos formas de entregar el archivo. En las dos, el estado se sigue en Importaciones.
export function Importar({ alImportar, alSubir }: Props) {
  return (
    <div className="grilla-2">
      <ImportarDesdeRuta alImportar={alImportar} />
      <SubirArchivo alSubir={alSubir} />
    </div>
  );
}
