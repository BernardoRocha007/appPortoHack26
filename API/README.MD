# Guia Técnico das APIs do Portal Único Siscomex (DUIMP)

Este documento descreve os 3 módulos de API oficiais do governo que nós simulamos dentro desta pasta `API/`, explicando o que cada endpoint faz, o que ele recebe e o que ele devolve.

---

## 1. Módulo `CADATRIBUTOS` (Regras e Órgãos Anuentes)
> **Objetivo:** Consultar as exigências da Receita, ANVISA e MAPA para uma NCM específica. É o coração do validador Anti-Canal Vermelho.

### `GET /cadatributos/api/ext/ncm/{ncm}/atributos`
* **Para que serve:** Pergunta ao governo quais campos adicionais um produto com essa NCM deve obrigatoriamente preencher.
* **Exemplo de Entrada (URL):**
  * NCM `3004.90.99` (Medicamentos)
* **Exemplo de Resposta (JSON):**
  ```json
  [
    {
      "codigo": "ATT_REG_ANVISA",
      "nome": "Registro na ANVISA",
      "orgao": "ANVISA",
      "obrigatorio": true
    },
    {
      "codigo": "ATT_LOTE",
      "nome": "Número do Lote",
      "orgao": "ANVISA",
      "obrigatorio": true
    },
    {
      "codigo": "ATT_VALIDADE",
      "nome": "Data de Validade",
      "orgao": "ANVISA",
      "obrigatorio": true
    }
  ]
  ```

---

## 2. Módulo `CATP` (Catálogo de Produtos)
> **Objetivo:** Cadastrar a lista de produtos no governo antes do registro da declaração aduaneira (DUIMP).

### `POST /catp/api/ext/produto`
* **Para que serve:** Envia os produtos da Fatura Comercial (em lote) já preenchidos com os seus devidos atributos.
* **Exemplo de Envio (JSON):**
  ```json
  {
    "codigoProduto": "MED-001",
    "descricao": "Paracetamol 500mg - Caixa com 20 comprimidos",
    "ncm": "3004.90.99",
    "cnpjRaiz": "00000000",
    "paisOrigem": "US",
    "atributos": [
      { "codigo": "ATT_REG_ANVISA", "valor": "100430025001" },
      { "codigo": "ATT_LOTE", "valor": "LOTE-2026-X" },
      { "codigo": "ATT_VALIDADE", "valor": "2028-12-31" }
    ]
  }
  ```
* **Exemplo de Resposta:**
  * Retorna o ID único do produto cadastrado no governo e status `APROVADO`.

---

## 3. Módulo `DUIMP-API` (Declaração Única de Importação)
> **Objetivo:** Validar a declaração final e emitir o canal de parametrização (Verde vs Vermelho).

### `POST /duimp-api/api/duimp/diagnostico`
* **Para que serve:** Faz a checagem prévia (*pre-flight*). Avalia se a declaração tem inconformidades impeditivas antes do envio final.
* **Se faltar algum atributo obrigatório (ex: sem lote ou sem registro Anvisa):**
  * Retorna lista de erros: `[ "Item 1: Atributo obrigatório ATT_REG_ANVISA não informado para NCM 3004.90.99" ]`.

### `POST /duimp-api/api/duimp`
* **Para que serve:** Registra a declaração oficial da carga no porto.
* **Exemplo de Resposta:**
  * Número da DUIMP gerado (ex: `26BR0000012345-0`).

### `GET /duimp-api/api/duimp/{numero}/situacao`
* **Para que serve:** Informa a cor do canal da carga:
  * **CANAL VERDE:** Carga desembaraçada automaticamente sem fiscal.
  * **CANAL VERMELHO:** Risco fiscal/sanitário. Contêiner retido para conferência documental e física no porto.

---

## Resumo do Fluxo entre as 3 APIs:

```text
CADATRIBUTOS ──► Nos diz o que a ANVISA/MAPA exige para cada NCM
       │
       ▼
     CATP     ──► Onde cadastramos os produtos com os atributos preenchidos
       │
       ▼
  DUIMP-API   ──► Onde montamos a declaração final e recebemos o Canal Verde
```

---

## Regra de Negócio: Produto vs DUIMP

* **Cadastro de Produto (O "RG"):** É o cadastro mestre permanente do item perante o governo (NCM, fabricante estrangeiro, atributos técnicos exigidos pela Anvisa, MAPA ou Receita). É feito uma única vez por produto.
* **DUIMP (O "Bilhete da Viagem"):** É o registro aduaneiro da carga física que acabou de atracar no porto (navio, contêiner, frete, seguro e quais produtos do catálogo vieram nessa viagem específica).

---

## Fluxo de Negócio do Software (Passo a Passo)

```text
[1] CLIENTE ENVIA A PLANILHA / FATURA
                         │
                         ▼
[2] SOFTWARE CRUZA COM AS REGRAS DO GOVERNO (CADATRIBUTOS)
    • Consulta as regras de anuência de cada NCM
    • Identifica o que Anvisa, MAPA e Receita Federal exigem
                         │
                         ▼
[3] CORRETOR PRÉVIO (Anti-Canal Vermelho)
    • Itens conformes: Validados e liberados
    • Itens pendentes: Alerta na tela para o despachante corrigir no escritório
                         │
                         ▼
[4] PREENCHIMENTO EM LOTE NO CATÁLOGO (CATP)
    • O despachante clica em "Enviar": o sistema cadastra todos os produtos
      e atributos no governo automaticamente, sem digitação manual
                         │
                         ▼
[5] REGISTRO DA DUIMP (DUIMP-API)
    • Declaração transmitida com 100% de conformidade técnica
    • Garantia de parametrização em CANAL VERDE (sem contêiner retido no porto)
```
