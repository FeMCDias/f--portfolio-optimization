# Portfolio Optimization

Ferramenta de otimização de carteiras em F# e Python, simulando 25 ativos de um universo de 30 do Dow Jones. O objetivo é maximizar o Sharpe Ratio no período de agosto a dezembro de 2024 (in-sample) e validar o desempenho no primeiro trimestre de 2025 (out-of-sample).

**Linguagens utilizadas**

* F#: cálculos funcionais e paralelizados
* Python (yfinance SDK): download sob demanda de preços históricos

---

## Visão Geral

1. Geração de todas as combinações de 25 em 30 ações (\~142.506 combinações).
2. Simulação de 1.000 vetores de pesos válidos por combinação (long-only, máximo 20% por ativo), usando paralelismo seguro em F#.
3. Cálculo de métricas: retornos diários, volatilidade e Sharpe Ratio anualizado para o conjunto in-sample.
4. Validação out-of-sample: aplica a carteira vencedora ao período de 1º de janeiro a 31 de março de 2025, calculando Sharpe Ratio e retorno total.

---

## Requisitos

| Software        |   Versão Utilizada | Observações                           |
| --------------- | --------------: | ------------------------------------- |
| .NET SDK        | 9.0 ou superior | compatível com Windows, macOS e Linux |
| F# via .NET SDK | 9.0 ou superior | incluído no SDK                       |
| Python          | 3.8 ou superior | pip, yfinance para download           |
| Git             |             2.x | para clonar o repositório             |

---

## Instalação
macOS – brew

```bash
brew install --cask dotnet-sdk
```

Windows – winget

```bash
winget install --id Microsoft.DotNet.SDK.9
```

Ubuntu

```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --channel 9.0
```

## Estrutura do Repositório

```
f--portfolio-optimization/
├── .gitignore
├── README.md
├── data/           # preços de 01/08/2024 a 31/12/2024
├── data_q1/        # preços de 01/01/2025 a 31/03/2025
├── scripts/        # script Python para baixar dados
│   ├── requirements.txt
│   └── download_data.py
├── src/
│   ├── processing/ # DataLoader.fs (leitura de CSV e cálculo de retornos)
│   ├── metrics/    # Metrics.fs (média, desvio, covariância, Sharpe)
│   ├── simulation/ # Simulation.fs (geração de pesos e simulações)
│   └── orchestrator/ # Main.fs e PortfolioOptimization.fsproj
└── output/         # best_portfolio.csv e run_summary.json
```

---

## Como usar

1. Clone o repositório:

   ```bash
   git clone https://github.com/FeMCDias/f--portfolio-optimization.git
   cd f--portfolio-optimization
   ```

2. Baixe os dados históricos:

   ```bash
   cd scripts
   python3 -m venv .venv
   source .venv/bin/activate      # Windows: .venv\Scripts\activate
   pip install -r requirements.txt
   python download_data.py        # preencherá data/ e data_q1/
   ```

3. Compile e execute a otimização:

   ```bash
   cd ../src/orchestrator
   dotnet restore
   dotnet build -c Release
   dotnet run
   ```

Isso irá gerar em `output/`:

* `best_portfolio.csv`: 25 tickers e pesos otimizados
* `run_summary.json`: métricas in-sample e out-of-sample, tempos de execução e contagens de simulações

---

## Saídas detalhadas

* **best\_portfolio.csv**: cada linha contém um ticker e seu peso correspondente.
* **run\_summary.json**: estrutura JSON com:

  * alocação dos ativos
  * métricas in-sample (Sharpe, tempos, contagens)
  * métricas out-of-sample (Sharpe e retorno total)

Exemplo:

```json
{
  "allocation": [ { "ticker": "AAPL", "weight": 0.078181 }, ... ],
  "metrics": {
    "inSampleSharpe": 3.06,
    "startTime": "2025-05-12T23:04:01Z",
    "midTime": "2025-05-12T23:13:17Z",
    "inSampleDurationSeconds": 556.125,
    "totalCombos": 142506,
    "simsPerCombo": 1000,
    "totalSimulations": 142506000,
    "processorCount": 8
  },
  "outOfSample": {
    "q1Sharpe": -0.26,
    "q1TotalReturn": -0.0107
  }
}
```

---

---

## Resultados Obtidos

* In-sample Sharpe: **3.0568**

* Start: **5/12/2025 11:04:01 PM**

* Mid: **5/12/2025 11:13:17 PM**

* Duração in-sample: **00:09:16**

* Out-of-sample Q1 2025 Sharpe: **-0.2584**

* Out-of-sample Q1 2025 Total Return: **-1.07%**

**Observação:** É esperado que o Sharpe out-of-sample possa ser negativo; desempenho passado não garante resultados futuros.

---

## Componentes principais

* **DataLoader.fs**: carrega arquivos CSV e calcula retornos diários.
* **Metrics.fs**: funções puras para estatísticas e Sharpe Ratio.
* **Simulation.fs**: gera e simula carteiras em paralelo.
* **Main.fs**: coordena o fluxo completo e grava os resultados.

---

## Desempenho

* Utiliza `Array.Parallel.map` em F# para simulação de pesos.
* Em CPU de 8 núcleos (Mac M1), leva cerca de 9 minutos para o in-sample (Release build).

---

## Auxílio de IA

* **ChatGPT**: documentação, debugging mais complexos, readme.md.
Tudo que foi auxiliado foi revisado por mim e testado.
