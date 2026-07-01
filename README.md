# Kaching Windows

Kaching Windows es una aplicacion de escritorio para Windows 11 construida con C#, .NET 8 y WPF. Su objetivo es ofrecer una base profesional para gestionar finanzas personales o de pequenos negocios: cuentas, movimientos, presupuestos, alertas y vision ejecutiva.

## Arquitectura

- `src/Kaching.Core`: dominio, modelos, servicios de aplicacion y persistencia.
- `src/Kaching.Windows`: interfaz WPF, tema visual y composicion MVVM.
- `tests/Kaching.Core.Tests`: pruebas automatizadas del nucleo.

La aplicacion evita dependencias innecesarias en la primera version: usa MVVM ligero, almacenamiento local en JSON y servicios desacoplados para que pueda evolucionar hacia base de datos, sincronizacion o integraciones bancarias sin reescribir la interfaz.

## Requisitos

- Windows 11
- .NET 8 SDK

## Ejecutar

```powershell
dotnet build KachingWindows.sln
dotnet run --project src/Kaching.Windows/Kaching.Windows.csproj
```

## Estado

Rama activa de desarrollo: `development`.
