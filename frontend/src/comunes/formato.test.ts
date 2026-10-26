import { describe, expect, it } from "vitest";
import { montoEnPesos, nombreSituacion } from "./formato";

const sinEspaciosEspeciales = (texto: string) => texto.replace(/\s/g, " ");

describe("montoEnPesos", () => {
  it.each([
    [261, "$ 261.000"],
    [0.5, "$ 500"],
    [193659024, "$ 193.659.024.000"],
  ])("convierte %s miles en %s", (miles, esperado) => {
    expect(sinEspaciosEspeciales(montoEnPesos(miles))).toBe(esperado);
  });
});

describe("nombreSituacion", () => {
  it("nombra los códigos especiales del BCRA", () => {
    expect(nombreSituacion(11)).toBe("Cubierta con garantía A");
    expect(nombreSituacion(99)).toBe("No clasificado");
  });

  it("muestra el número si el código no existe", () => {
    expect(nombreSituacion(7)).toBe("Código 7");
  });
});
