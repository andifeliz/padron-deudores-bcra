import { BuscarDeudor } from "./BuscarDeudor";
import { BuscarEntidad } from "./BuscarEntidad";
import { TopDeudores } from "./TopDeudores";

export function Consultas() {
  return (
    <>
      <div className="grilla-2">
        <BuscarDeudor />
        <BuscarEntidad />
      </div>
      <TopDeudores />
      <p className="nota">Los montos se muestran en pesos. El BCRA los informa en miles: 261,0 son $ 261.000.</p>
    </>
  );
}
