# Aplicacion 2 (Windows Forms) - Sucursal bancaria + Calculadora del cajero

Turnos de ventanilla con cola circular y evaluador de expresiones (Shunting-Yard) con pilas.

## Estructuras (memoria estatica, sin colecciones nativas)
| Estructura | Archivo | Punteros | Uso en la app |
|---|---|---|---|
| Cola circular | Estructuras/ColaCircular.cs | front, rear, cantidad | Fila de turnos (8) y pantalla de llamados (4) |
| Pila | Estructuras/Pila.cs | top | Pila de operadores (infija->postfija) y pila de operandos (evaluacion) |
| Cola simple | Estructuras/ColaSimple.cs | front, rear | Tokens de entrada y salida postfija |

Metodos de la Pila: `Push`, `Pop`, `Peek`, `IsEmpty`, `IsFull`. Colas: `Enqueue`, `Dequeue`, `Peek`, `IsEmpty`, `IsFull`.

## Estructura del proyecto
- `Estructuras/` Pila, ColaSimple, ColaCircular (genericas sobre `T[]` de tamano fijo)
- `Dominio/` logica de negocio
- `UI/` formularios Windows Forms (FormPrincipal, Controles)
- `Program.cs` punto de entrada

## Ejecutar
Abrir `SucursalBancaria.sln` en Visual Studio 2019/2022 (.NET Framework 4.8, C# 7.3) y presionar F5 (aplicacion Windows Forms).
