import { Component, type ErrorInfo, type ReactNode } from "react";

interface Estado {
  fallo: boolean;
}

// Si un componente falla al dibujarse, muestra un aviso en lugar de dejar la pantalla en blanco.
export class ErrorInesperado extends Component<{ children: ReactNode }, Estado> {
  state: Estado = { fallo: false };

  static getDerivedStateFromError(): Estado {
    return { fallo: true };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error("Error inesperado en el panel", error, info.componentStack);
  }

  render() {
    if (!this.state.fallo) {
      return this.props.children;
    }

    return (
      <section className="tarjeta" role="alert">
        <h2>Algo salió mal</h2>
        <p className="descripcion">El panel encontró un error inesperado. Recargá la página para seguir.</p>
        <button className="boton" onClick={() => window.location.reload()}>
          Recargar
        </button>
      </section>
    );
  }
}
