# 0003 — Estado de producto: derivado del stock, salvo oculto y eliminado

**Decidida:** ~2026-03-22 (saco `IsActive`) · 2026-03-25 (`Deleted`)
**Escrita:** 2026-11-24
**Dónde:** `Product`, `ProductStatus`, `ProductService`, `ProductRepository`
**Relacionada:** [0001](0001-concurrencia-de-stock.md) — la misma derivación,
replicada en SQL.

## Qué pasaba

Un producto tenía que contestar tres preguntas distintas:

1. ¿Se puede comprar ahora? → depende del stock.
2. ¿Quiero mostrarlo en el catálogo? → lo decido yo como admin.
3. ¿Lo eliminé? → permanente, sin perder el historial de pedidos.

El modelo inicial tenía un `IsActive` que mezclaba las tres: un producto sin
stock y uno oculto eran lo mismo, así que no podía ocultar algo que sí tenía
stock ni distinguir "agotado" de "despublicado". Lo saqué y puse un enum
(`Available`, `OutOfStock`, `Hidden`, `Deleted`).

## Qué hice

El estado se guarda en la columna `product_status` y tiene **dos orígenes
distintos**:

- **`Available` / `OutOfStock` son derivados.** No se eligen a mano: son
  `CurrentStock > 0 ? Available : OutOfStock`, con una sola definición en el
  dominio (`Product.SyncStatusWithStock`). Se recalcula al crear el producto, al
  ajustar stock, al publicar y al reducir o reponer stock.
- **`Hidden` / `Deleted` son explícitos y el stock no los toca.** `AdjustStock`
  e `IncreaseStock` no republican un producto oculto (solo `Publish` lo hace);
  `Delete` es permanente y bloquea cualquier cambio posterior
  (`EnsureNotDeleted`).

Guardo el estado en vez de calcularlo al leer para poder filtrar y listar en SQL
(`WHERE Status != Deleted`) sin repetir lógica. Las rutas que usan
`ExecuteUpdate` replican la derivación en SQL (ver 0001).

## Qué descarté y por qué

- **`IsActive` + disponibilidad calculada al leer** — mezclaba tres conceptos y
  obligaba a recalcular en cada consulta.
- **Propiedad calculada no persistida** — no se puede filtrar en SQL ni paginar
  por estado.
- **Trigger en la base que mantenga el estado** — esconde la regla de negocio
  fuera del dominio y después cuesta testearla y depurarla. Si algo se rompe, lo
  quiero ver en el código, no en un trigger que no me acuerdo que existe.
- **Un solo enum sin separar oculto de eliminado** — ocultar tiene que ser
  reversible; eliminar tiene que preservar el historial y no volver atrás.

## Qué me cuesta

**A favor**

- El cliente consulta por estado sin lógica extra y los listados excluyen
  eliminados con una sola condición.
- "Agotado" nunca se elige a mano: es un hecho del stock, no una opinión.
- Ocultar y eliminar no se pisan con una edición de stock.

**Lo que acepto**

- **La regla stock → estado está duplicada** en C# y en SQL. La de SQL manda en
  checkout (0001); la del dominio es el espejo para el resto.
- **El estado derivado es un dato guardado**, así que una escritura directa en
  la base lo puede dejar inconsistente (por ejemplo `Available` con stock 0). No
  hay `CHECK CONSTRAINT` que lo impida. La app es el único cliente de la base,
  así que lo dejo pasar.
- **`Available` con stock 0 es un estado transitorio posible** durante los
  microsegundos entre el decremento y la sincronización (0001).
- **El enum es cerrado:** sumar un estado (ponele, "en preparación") toca el
  dominio, los filtros y los DTOs.

## Cuándo lo reviso

- Aparece stock por sucursal o multi-almacén (`CurrentStock` deja de ser un
  número).
- Agrego reserva de stock (un estado entre disponible y vendido).
- Necesito un flujo de borrador/aprobación de productos.
- Habilito acceso directo a la base, importación masiva o un panel que escriba
  el estado a mano.

## Dónde está

- `src/Domain/Entities/Product.cs`
- `src/Domain/Entities/Enums/ProductStatus.cs`
- `src/Infrastructure/Repositories/ProductRepository.cs`
- `src/Infrastructure/Migrations/20260322230514_RemoveProductIsActive.cs`
- `src/Infrastructure/Migrations/20260325013747_AddProductStatusDeleted.cs`
