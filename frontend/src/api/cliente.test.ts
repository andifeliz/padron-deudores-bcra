import { describe, expect, it } from "vitest";
import { mensajeDelProblema } from "./cliente";

describe("mensajeDelProblema", () => {
  it("prioriza el error de validación", () => {
    const problema = { title: "One or more validation errors occurred.", errors: { cuit: ["La identificación tiene 11 letras o números."] } };
    expect(mensajeDelProblema(problema, 400)).toBe("La identificación tiene 11 letras o números.");
  });

  it("usa el detalle y, si no hay, el título", () => {
    expect(mensajeDelProblema({ title: "Archivo no encontrado", detail: "No existe x.txt." }, 404)).toBe("No existe x.txt.");
    expect(mensajeDelProblema({ title: "Deudor no encontrado" }, 404)).toBe("Deudor no encontrado");
  });

  it("sin cuerpo, informa el código", () => {
    expect(mensajeDelProblema(null, 502)).toBe("Error 502");
  });
});
