# Client Positions – Importador + API (.NET 8, PostgreSQL, Docker)

Projeto para **importar** 10M posições financeiras de uma **API externa** e **consultar** via **API REST**.

- **Console (importer):** consome a API externa em lote, processa e **persiste** no PostgreSQL (EF Core + Npgsql).
- **API (ASP.NET Core):** expõe endpoints para consultar as **últimas posições** por `positionId` e agregados.
- **Docker:** `postgres`, `importer` (job) e `api`.

> **Stack:** .NET 8, EF Core, Npgsql, Polly (resiliência), Docker/Compose.

---

# 0) copiar variáveis
cp .env.example .env
# edite .env e preencha EXTERNALAPI_KEY

# 1) banco
docker compose up -d postgres

# 2) importador (job — aplica migrations, importa e finaliza)
docker compose up --build importer

# 3) conferir contagem
docker exec -it financial-db psql -U admin -d positionsdb -c "SELECT COUNT(*) FROM tb_positions;"

# 4) API
docker compose up --build -d api
# Swagger:
# http://localhost:8080/swagger

No **compose**, a chave vai para o importer via `EXTERNALAPI__KEY`.
Outras configs (batch, mock etc.) ficam em `appsettings` com override por env var.

### Connection Strings

- **Docker (dentro dos containers):** Host=postgres;Port=5432;Database=positionsdb;Username=admin;Password=admin
- **Local (F5):** Host=localhost;Port=5432;Database=positionsdb;Username=admin;Password=admin


## Executar (Docker)

`docker-compose.yml` sobe:

- `postgres` (contêiner do banco)
- `importer` (job que termina após o carregamento)
- `api` (HTTP em `8080`)

# subir banco
docker compose up -d postgres

# rodar importador
docker compose up --build importer

# checar progresso
docker exec -it financial-db psql -U admin -d positionsdb -c "SELECT COUNT(*) FROM tb_positions;"

# subir API
docker compose up --build -d api
# http://localhost:8080/swagger
```


## Decisões de projeto (performance)

- **Importador**
  - Streaming assíncrono + **batch** configurável (`BatchSize`).
  - `AutoDetectChangesEnabled=false` e `NoTracking`.
  - **Resiliência** com Polly (retry exponencial + timeout).
  - **UPSERT** opcional via `INSERT ... ON CONFLICT (PositionId, Date) DO UPDATE`.
  - `MaxItems` configurável (por env var).

- **Banco**
  - PK composta: (`PositionId`, `Date`).
  - Índices recomendados:
    - `(ClientId, PositionId, Date)` – filtros de cliente.
    - `(PositionId, Date)` – últimas por posição.
    - `Value` – top10.

- **API**
  - `DbContext` **NoTracking** por padrão.
  - Queries com `GroupBy + Max(Date)` + `Join` → **eficiente no Postgres**.
  - `ResponseCaching` no `/top10` (dev).

## Troubleshooting

- **API não conecta no DB**  
  Confirme `Host=postgres` **dentro** do container e aguarde `postgres` ficar **healthy**.

- **Importador “demora demais”**  
  É ingestão grande. Use `EXTERNALAPI__MAXITEMS` para smoke test e remova depois.

- **Confirmação de dados**
  ```sql
  SELECT COUNT(*) FROM tb_positions;
  SELECT clientid FROM tb_positions LIMIT 5;
  ```

- **Compose warning “version is obsolete”**  
  Seguro ignorar; ou remova a linha `version:` do YAML.

## Notas
- O importador aplica **migrations** automaticamente.
- As queries seguem o requisito “**agrupe por `positionId` e selecione a última por date**”.
- `Polly` configurado para resiliência no consumo da API externa.
- Docker Compose pronto para **postgres + importer (job) + api**.

## Licença

Uso didático.