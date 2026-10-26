export function MensajeError({ mensaje }: { mensaje: string | null }) {
  if (!mensaje) {
    return null;
  }

  return (
    <p className="mensaje-error" role="alert">
      {mensaje}
    </p>
  );
}
