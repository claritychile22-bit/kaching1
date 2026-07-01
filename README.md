# Kaching Windows for Shopify

Kaching Windows es una aplicacion de escritorio para Windows 11 construida con C#, .NET 8 y WPF para monitorizar ventas de Shopify en tiempo real.

La aplicacion se conecta a la Shopify Admin GraphQL API, revisa pedidos pagados en intervalos configurables, reproduce `kaching.wav`, muestra notificaciones de Windows y conserva un historial local de ventas.

## Funcionalidades

- Configuracion de tienda, Admin API token e intervalo de revision.
- `ShopifyService` para consultar pedidos con la Admin GraphQL API.
- Monitor de pedidos nuevos sin duplicar ventas ya registradas.
- Sonido `kaching.wav` al detectar nuevas ventas.
- Notificaciones de Windows desde la bandeja del sistema.
- Historial de ventas recientes.
- Ventas del dia y total vendido.
- Icono en la bandeja del sistema con opcion de abrir o salir.
- Inicio automatico con Windows mediante HKCU Run.

## Arquitectura

- `src/Kaching.Core`: modelos Shopify, servicios de aplicacion, monitor, analitica y persistencia local.
- `src/Kaching.Windows`: interfaz WPF, MVVM, bandeja, sonido, notificaciones e inicio automatico.
- `tests/Kaching.Core.Tests`: pruebas automatizadas del nucleo.

## Shopify

La app usa la GraphQL Admin API `2026-07`. El token debe tener permisos de lectura de pedidos (`read_orders`) y se envia como `X-Shopify-Access-Token`.

## Requisitos

- Windows 11
- .NET 8 SDK
- Tienda Shopify con Admin API token valido

## Ejecutar

```powershell
dotnet build KachingWindows.sln
dotnet run --project src/Kaching.Windows/Kaching.Windows.csproj
```

## Estado

Rama de trabajo: `shopify-monitor`.
