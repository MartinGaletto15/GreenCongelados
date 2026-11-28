# 0001 — Concurrencia de stock y transiciones de estado

**Fecha:** 2026-11-24
**Dónde:** `OrderService`, `ProductRepository`, `OrderRepository`, `CartItemRepository`
**Stack:** .NET 8 · EF Core 8 · SQL Server

## Qué pasaba

Las reglas de stock y de estado del pedido vivían en el dominio y se ejecutaban
en un ciclo **leer → validar en memoria → escribir**. No había token de
concurrencia en el modelo y `GenericRepository.UpdateAsync` hacía
`DbSet.Update(entity)`, que marca todas las columnas: EF generaba
`UPDATE ... WHERE Id = @id` sin comparar el valor leído. Dos transacciones que
leyeron el mismo stock escribían sin conflicto y **ganaba la última, en
silencio**.

| Caso | Qué pasaba |
|---|---|
| Dos pedidos simultáneos por la última unidad | se vendían 2 unidades de 1 |
| Doble cancelación de la misma orden | el stock se devolvía dos veces y quedaba inflado para siempre |
| Admin editando stock mientras se procesa un pedido | el snapshot viejo pisaba el valor nuevo |
| Dos altas con el mismo email o cupón | 500 en vez de 409 |
| Dos checkouts del mismo carrito | pedido duplicado |

Los dos primeros son los que me importaban: corrompen datos sin que nadie se
entere y el daño se acumula. Los otros fallan a la vista, los puede arreglar una
persona y no justificaban la misma inversión.

## Qué hice

Meto la condición en el `WHERE` y dejo que el motor la evalúe bajo el lock de
fila, sin ventana entre validar y escribir. Es un compare-and-swap en SQL, solo
para las dos operaciones cuya falla es una condición de negocio terminal:

```sql
UPDATE [p] SET [p].[CurrentStock] = [p].[CurrentStock] - @q
FROM [Products] AS [p]
WHERE [p].[IdProduct] = @id
  AND [p].[product_status] = 0     -- Available
  AND [p].[CurrentStock] >= @q
```

- **Descuento de stock en checkout** → `ProductRepository.TryDecreaseStockAsync`
  (`WHERE Status = Available AND CurrentStock >= @q`).
- **Cancelación de orden** → `OrderRepository.TryCancelAsync`
  (`WHERE OrderStatus <> 'Cancelled' AND OrderStatus <> 'Delivered'`); solo el
  request que saca `rowsAffected == 1` devuelve stock.

`rowsAffected == 0` significa "en este instante no se podía" y sale como 409 de
negocio (sin stock / ya cancelada).

**No agregué `RowVersion`.** El conflicto de stock no es reintentable: reintentar
solo repite la lectura y vuelve a fallar. Un token optimista detectaría el
problema pero me obligaría a un retry loop, ETags y una UX de conflicto en cada
endpoint, todo para no resolver nada que el `WHERE` no resuelva ya. Trabajando
solo, cada pieza extra de esas la termino manteniendo yo, así que la opción más
chica gana casi siempre.

## Qué descarté y por qué

- **Token optimista (`RowVersion` + `[Timestamp]`)** — detecta el conflicto pero
  no lo resuelve, y "sin stock" no es un caso reintentable.
- **Lock pesimista (`UPDLOCK` / `Serializable`)** — serializa y bloquea filas,
  con riesgo de deadlocks y acoplado a SQL Server.
- **No hacer nada** — el stock inflado no se recupera y el error es invisible.
- **Cola o lock distribuido por producto** — demasiado para el volumen actual
  (decenas de pedidos por día). Queda en el backlog por si algún día hace falta.

## Cómo quedó

| Archivo | Qué hace ahora |
|---|---|
| `IProductRepository` / `ProductRepository` | `GetByIdReadOnlyAsync`, `TryDecreaseStockAsync`, `IncreaseStockAsync` |
| `IOrderRepository` / `OrderRepository` | `TryCancelAsync`, `GetByIdWithDetailsNoTrackingAsync` |
| `Domain/Entities/Order.cs` | `Order.Create` ya no muta `Product` |
| `OrderService` / `CartItemService` | `CreateAsync`, `EnsureStockOrThrowAsync`, `CancelOrderAsync`, `ClearCartAsync` |

Si vuelvo a tocar esto, acordarme de:

1. **La transacción abre antes del primer decremento**, no después de las
   lecturas.
2. **Los decrementos van ordenados por `IdProduct`**, para no trabar dos pedidos
   que comparten dos o más productos.
3. **Las lecturas de validación usan `AsNoTracking`.** Un producto trackeado y
   mutado haría que el `SaveChanges` posterior reescriba `CurrentStock` con el
   snapshot y anule el UPDATE condicional.
4. **No llamar `UpdateAsync(product)` después de un `ExecuteUpdate`**: la
   sentencia ya persistió el cambio.
5. **`Order.Create` ya no reduce stock.** Eso es de la persistencia, que es la
   única capa que puede hacerlo atómico. `SyncStatusWithStock` sigue en el
   dominio como documentación; la condición que manda es el `WHERE`.

## Qué me cuesta

**A favor**

- Se terminaron la sobreventa y la doble devolución, las dos fallas mudas.
- Sin cambios de esquema, migraciones ni servicios nuevos.
- Sin retry loops, ETags ni UX de conflicto.
- La regla se lee en el `WHERE`: la sentencia se explica sola.

**Lo que acepto**

- **La regla de stock quedó en dos capas** (C# y SQL). Manda el `WHERE`; el
  dominio queda como espejo documentado.
- **Doble clic en "cancelar" devuelve 409 `ALREADY_CANCELLED`** en vez de ser un
  no-op. Es correcto y visible; si molesta, después lo suavizo.
- **`TryDecreaseStockAsync` son dos sentencias** (decremento + sincronización de
  estado), con una ventana de microsegundos donde el stock es 0 y el estado
  sigue `Available`. No hace daño: el decremento valida `CurrentStock`, no el
  estado.
- **El precio se lee fuera de la transacción:** se usa el que vio el usuario al
  hacer checkout, no el vigente al commit.

**Lo que dejo afuera (por ahora)**

- Lost update entre dos admins editando el mismo producto.
- `ClearCartAsync` vacía todo el carrito, incluido un ítem agregado desde otra
  pestaña.
- Idempotencia de checkout (`Idempotency-Key`).
- Unicidad devolviendo 500 en vez de 409.

## Cómo lo verifiqué

Contra SQL Server 2025, con una base temporal creada y borrada (la de desarrollo
no la toqué):

- Migraciones sobre base limpia, sin pendientes ni drift.
- T-SQL mirado a mano: la condición aparece en el `WHERE`, el enum se traduce a
  `int` y `OrderStatus` a `nvarchar`.
- 14 checks funcionales: stock 0, estado `OutOfStock`, stock no negativo,
  `Hidden` sigue oculto, rollback del carrito, restauración una sola vez.
- Concurrencia real, conexiones separadas y arranque simultáneo por barrera:
  5 rondas de 2 checkouts por la última unidad → 1 ganador por ronda y 0
  sobreventas; 5 rondas de 2 cancelaciones simultáneas → 0 devoluciones dobles;
  3 checkouts con 2 productos → sin deadlock.

## Cuándo lo reviso

- Aparecen ediciones concurrentes reales del mismo producto (dos admins).
- Paso a varias instancias con escritura intensiva.
- Sumo pago online, donde una sobreventa deja de ser recuperable.
- El volumen justifica serializar el checkout por producto.

## Dónde está

- `src/Infrastructure/Repositories/ProductRepository.cs`
- `src/Application/Services/OrderService.cs`
- `Notas.txt` — FASE 1, "Concurrencia atómica" (quedó reemplazado en parte por
  esta nota).
- `README.md` — "Limitaciones conocidas" y estado del proyecto.
