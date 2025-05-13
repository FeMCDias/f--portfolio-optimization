module Main

open System
open System.IO
open DataLoader
open Simulation
open Metrics

/// Combina todas as combinações de k elementos de uma lista.
let rec combinations k lst =
    match k, lst with
    | 0, _    -> [ [] ]
    | _, []   -> []
    | k, x::xs ->
        let includeX = combinations (k-1) xs |> List.map (fun tail -> x :: tail)
        let excludeX = combinations k xs
        includeX @ excludeX

[<EntryPoint>]
let main _ =
    // 1) Registro de início
    let startTime = DateTime.UtcNow

    // 2) Caminhos
    let root    = Path.Combine(__SOURCE_DIRECTORY__, "..", "..")
    let dataDir = Path.Combine(root, "data")
    let q1Dir   = Path.Combine(root, "data_q1")
    let outDir  = Path.Combine(root, "output")
    Directory.CreateDirectory(outDir) |> ignore

    // 3) Universo de 30 tickers
    let tickers = [|
      "AAPL";"AMGN";"AXP";"BA";"CAT";"CRM";"CSCO";"CVX";"DIS";"DOW";
      "GS";"HD";"HON";"IBM";"INTC";"JNJ";"JPM";"KO";"MCD";"MMM";
      "MRK";"MSFT";"NKE";"PG";"TRV";"UNH";"V";"VZ";"WBA";"WMT"
    |]

    // 4) Carrega retornos in-sample
    let returnsMap    = loadAll dataDir tickers

    // 5) Combinações e contagens
    let comboLists       = combinations 25 (Array.toList tickers)
    let combos           = comboLists |> List.map List.toArray |> List.toArray
    let simsPerCombo     = 1000
    let totalCombos      = combos.Length
    let totalSimulations = totalCombos * simsPerCombo

    // 6) Simulações in-sample em paralelo
    let results =
        combos
        |> Array.Parallel.map (fun combo ->
            let rets = combo |> Array.map (fun t -> returnsMap.[t])
            let sims = simulate rets simsPerCombo 0.20
            let bestW, bestS = sims |> Array.maxBy snd
            combo, bestW, bestS)

    // 7) Seleciona melhor in-sample
    let bestCombo, bestWeights, bestSharpe =
        results |> Array.maxBy (fun (_,_,s) -> s)

    // 8) Tempo de meio e duração in-sample
    let midTime  = DateTime.UtcNow
    let duration = midTime - startTime

    // 9) Grava CSV in-sample
    let portfolioLines =
        Array.zip bestCombo bestWeights
        |> Array.map (fun (t,w) -> sprintf "%s,%.6f" t w)
    File.WriteAllLines(Path.Combine(outDir, "best_portfolio.csv"),
        Array.append [| "Ticker,Weight" |] portfolioLines)

    // 10) Exibe in-sample no console
    printfn "In-sample Sharpe: %.4f" bestSharpe
    printfn "Start: %O    Mid: %O    Duration: %O"
        startTime midTime duration

    // 11) Gera JSON in-sample
    let allocEntries =
        bestCombo
        |> Array.mapi (fun i t ->
            sprintf "    { \"ticker\": \"%s\", \"weight\": %.6f }"
                t bestWeights.[i])
        |> String.concat ",\n"

    let summaryJson =
        "{\n" +
        "  \"allocation\": [\n" +
        allocEntries + "\n" +
        "  ],\n" +
        "  \"metrics\": {\n" +
        (sprintf "    \"inSampleSharpe\": %.6f,\n" bestSharpe) +
        (sprintf "    \"startTime\": \"%s\",\n" (startTime.ToString("o"))) +
        (sprintf "    \"midTime\": \"%s\",\n" (midTime.ToString("o"))) +
        (sprintf "    \"inSampleDurationSeconds\": %.3f,\n" duration.TotalSeconds) +
        (sprintf "    \"totalCombos\": %d,\n" totalCombos) +
        (sprintf "    \"simsPerCombo\": %d,\n" simsPerCombo) +
        (sprintf "    \"totalSimulations\": %d,\n" totalSimulations) +
        (sprintf "    \"processorCount\": %d\n" Environment.ProcessorCount) +
        "  }\n" +
        "}"

    File.WriteAllText(Path.Combine(outDir, "run_summary.json"), summaryJson)

    // 12) Avaliação out-of-sample Q1 2025
    let q1ReturnsMap = loadAll q1Dir tickers
    let q1Returns    = bestCombo |> Array.map (fun t -> q1ReturnsMap.[t])
    let q1Sharpe     = portfolioSharpe q1Returns bestWeights
    let q1DailyRets  = portfolioDailyReturns q1Returns bestWeights
    let q1TotalReturn =
        q1DailyRets
        |> Array.fold (fun acc r -> acc * (1.0 + r)) 1.0
        |> fun v -> v - 1.0

    // 13) Imprime out-of-sample no console
    printfn "Out-of-sample Q1 2025 Sharpe: %.4f" q1Sharpe
    printfn "Out-of-sample Q1 2025 Total Return: %.2f%%" (q1TotalReturn * 100.0)

    // 14) Atualiza JSON adicionando out-of-sample e grava
    let fullSummaryJson =
        summaryJson.TrimEnd('}') +
        (sprintf """
  ,
  "outOfSample": {
    "q1Sharpe": %.6f,
    "q1TotalReturn": %.6f
  }
}""" q1Sharpe q1TotalReturn)

    File.WriteAllText(Path.Combine(outDir, "run_summary.json"), fullSummaryJson)

    // 15) Retorna sucesso
    0
